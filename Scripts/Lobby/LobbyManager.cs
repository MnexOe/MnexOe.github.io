using Godot;
using System.Collections.Generic;
using AchtungDieSpurve.Config;
using AchtungDieSpurve.Input;
using AchtungDieSpurve.Interfaces;

namespace AchtungDieSpurve.Lobby
{
	public class LobbyManager : Control
	{
		private const int MaxPlayers = 6;

		private static readonly Color[] PlayerColors =
		{
			new Color(1f,   0.3f, 0.3f),  // Red
			new Color(0.3f, 0.5f, 1f),    // Blue
			new Color(1f,   1f,   0.2f),  // Yellow
			new Color(0.3f, 1f,   0.3f),  // Green
			new Color(1f,   0.3f, 1f),    // Magenta
			new Color(0.3f, 1f,   1f),    // Cyan
		};

		private static readonly string[] PlayerNames =
			{ "Player 1", "Player 2", "Player 3", "Player 4", "Player 5", "Player 6" };

		private static readonly KeyList[] DefaultLeftKeys =
			{ KeyList.Q, KeyList.O, KeyList.C, KeyList.Left, KeyList.Kp1, KeyList.Z };

		private static readonly KeyList[] DefaultRightKeys =
			{ KeyList.W, KeyList.P, KeyList.V, KeyList.Right, KeyList.Kp3, KeyList.X };

		private static readonly int[] TargetScoreOptions = { 3, 5, 10, 15, 20 };

		// ── state ─────────────────────────────────────────────────────────────
		private readonly bool[]     _slotActive = new bool[MaxPlayers];
		private readonly KeyList[]  _leftKeys   = (KeyList[])DefaultLeftKeys.Clone();
		private readonly KeyList[]  _rightKeys  = (KeyList[])DefaultRightKeys.Clone();
		private int _targetScoreIndex = 2; // default: 10

		// ── rebind state ──────────────────────────────────────────────────────
		private int    _rebindSlot   = -1;
		private bool   _rebindIsLeft;
		private Button _rebindButton;

		// ── UI refs ───────────────────────────────────────────────────────────
		private readonly ColorRect[] _colorSwatches  = new ColorRect[MaxPlayers];
		private readonly Button[]    _joinButtons    = new Button[MaxPlayers];
		private readonly Button[]    _leftKeyButtons = new Button[MaxPlayers];
		private readonly Button[]    _rightKeyButtons = new Button[MaxPlayers];
		private Label  _targetScoreLabel;
		private Button _startButton;

		// ─────────────────────────────────────────────────────────────────────

		public override void _Ready()
		{
			GD.Print("LobbyManager ready");
			SetAnchorsAndMarginsPreset(LayoutPreset.Wide);
			BuildUI();
		}

		public override void _Input(InputEvent @event)
		{
			if (_rebindSlot < 0) return;
			if (!(@event is InputEventKey keyEvent) || !keyEvent.Pressed || keyEvent.Echo) return;

			var key = (KeyList)keyEvent.Scancode;

			if (key == KeyList.Escape)
			{
				CancelRebind();
			}
			else
			{
				if (_rebindIsLeft) _leftKeys[_rebindSlot]  = key;
				else               _rightKeys[_rebindSlot] = key;

				_rebindButton.Text     = KeyLabel(key);
				_rebindButton.Modulate = Colors.White;
				_rebindSlot            = -1;
				_rebindButton          = null;
			}

			GetTree().SetInputAsHandled();
		}

		// ── UI construction ───────────────────────────────────────────────────

		private void BuildUI()
		{
			var margin = new MarginContainer();
			AddChild(margin);
			margin.SetAnchorsAndMarginsPreset(LayoutPreset.Wide);
			margin.AddConstantOverride("margin_left",   60);
			margin.AddConstantOverride("margin_right",  60);
			margin.AddConstantOverride("margin_top",    30);
			margin.AddConstantOverride("margin_bottom", 30);

			var root = new VBoxContainer();
			root.AddConstantOverride("separation", 12);
			margin.AddChild(root);

			// Title
			var title = new Label { Text = "ACHTUNG DIE SPURVE" };
			title.Align = Label.AlignEnum.Center;
			root.AddChild(title);

			root.AddChild(new HSeparator());

			// Column headers
			root.AddChild(BuildHeaderRow());

			// Player slot rows
			for (int i = 0; i < MaxPlayers; i++)
				root.AddChild(BuildSlotRow(i));

			root.AddChild(new HSeparator());

			// Target score selector
			root.AddChild(BuildTargetScoreRow());

			root.AddChild(new HSeparator());

			// Start button
			_startButton = new Button { Text = "START MATCH", Disabled = true };
			_startButton.Connect("pressed", this, nameof(OnStartPressed));
			root.AddChild(_startButton);
		}

		private HBoxContainer BuildHeaderRow()
		{
			var row = new HBoxContainer();
			MakeCell(row, "",            30);   // swatch placeholder
			MakeCell(row, "Player",      130);
			MakeCell(row, "Turn Left",   110);
			MakeCell(row, "Turn Right",  110);
			MakeCell(row, "",            100);  // join button placeholder
			return row;
		}

		private HBoxContainer BuildSlotRow(int i)
		{
			var row = new HBoxContainer();
			row.AddConstantOverride("separation", 8);

			// Color swatch
			var swatch = new ColorRect();
			swatch.Color          = PlayerColors[i];
			swatch.RectMinSize    = new Vector2(24, 24);
			swatch.SizeFlagsHorizontal = (int)SizeFlags.ShrinkCenter;
			swatch.Modulate       = new Color(0.4f, 0.4f, 0.4f); // dimmed until joined
			_colorSwatches[i]     = swatch;
			row.AddChild(swatch);

			// Player name
			var name = new Label { Text = PlayerNames[i] };
			name.RectMinSize = new Vector2(130, 0);
			row.AddChild(name);

			// Left key button
			var leftBtn = new Button { Text = KeyLabel(_leftKeys[i]) };
			leftBtn.RectMinSize = new Vector2(100, 0);
			leftBtn.Connect("pressed", this, nameof(OnLeftKeyPressed),
				new Godot.Collections.Array { i });
			_leftKeyButtons[i] = leftBtn;
			row.AddChild(leftBtn);

			// Right key button
			var rightBtn = new Button { Text = KeyLabel(_rightKeys[i]) };
			rightBtn.RectMinSize = new Vector2(100, 0);
			rightBtn.Connect("pressed", this, nameof(OnRightKeyPressed),
				new Godot.Collections.Array { i });
			_rightKeyButtons[i] = rightBtn;
			row.AddChild(rightBtn);

			// Join / Leave button
			var joinBtn = new Button { Text = "Join" };
			joinBtn.RectMinSize = new Vector2(90, 0);
			joinBtn.Connect("pressed", this, nameof(OnJoinPressed),
				new Godot.Collections.Array { i });
			_joinButtons[i] = joinBtn;
			row.AddChild(joinBtn);

			return row;
		}

		private HBoxContainer BuildTargetScoreRow()
		{
			var row = new HBoxContainer();
			row.AddConstantOverride("separation", 8);

			var label = new Label { Text = "Target Score:" };
			row.AddChild(label);

			var decBtn = new Button { Text = "◀" };
			decBtn.Connect("pressed", this, nameof(OnTargetScoreDec));
			row.AddChild(decBtn);

			_targetScoreLabel = new Label
			{
				Text         = TargetScoreOptions[_targetScoreIndex].ToString(),
				RectMinSize  = new Vector2(36, 0),
				Align        = Label.AlignEnum.Center,
			};
			row.AddChild(_targetScoreLabel);

			var incBtn = new Button { Text = "▶" };
			incBtn.Connect("pressed", this, nameof(OnTargetScoreInc));
			row.AddChild(incBtn);

			return row;
		}

		// ── signal handlers ───────────────────────────────────────────────────

		private void OnJoinPressed(int index)
		{
			_slotActive[index]          = !_slotActive[index];
			_joinButtons[index].Text    = _slotActive[index] ? "Leave" : "Join";
			_colorSwatches[index].Modulate = _slotActive[index]
				? Colors.White
				: new Color(0.4f, 0.4f, 0.4f);
			RefreshStartButton();
		}

		private void OnLeftKeyPressed(int index)  => StartRebind(index, true,  _leftKeyButtons[index]);
		private void OnRightKeyPressed(int index) => StartRebind(index, false, _rightKeyButtons[index]);

		private void OnTargetScoreDec()
		{
			if (_targetScoreIndex > 0)
			{
				_targetScoreIndex--;
				_targetScoreLabel.Text = TargetScoreOptions[_targetScoreIndex].ToString();
			}
		}

		private void OnTargetScoreInc()
		{
			if (_targetScoreIndex < TargetScoreOptions.Length - 1)
			{
				_targetScoreIndex++;
				_targetScoreLabel.Text = TargetScoreOptions[_targetScoreIndex].ToString();
			}
		}

		private void OnStartPressed()
		{
			var players = new List<IPlayerConfig>();
			for (int i = 0; i < MaxPlayers; i++)
			{
				if (!_slotActive[i]) continue;
				var input = new KeyboardPlayerInput(i, _leftKeys[i], _rightKeys[i]);
				players.Add(new PlayerConfig(i, PlayerNames[i], PlayerColors[i], input));
			}

			GameData.Instance.Players     = players;
			GameData.Instance.TargetScore = TargetScoreOptions[_targetScoreIndex];

			GetTree().ChangeScene("res://Scenes/Countdown.tscn");
		}

		// ── rebind helpers ────────────────────────────────────────────────────

		private void StartRebind(int index, bool isLeft, Button button)
		{
			if (_rebindSlot >= 0) CancelRebind();

			_rebindSlot    = index;
			_rebindIsLeft  = isLeft;
			_rebindButton  = button;
			button.Text    = "Press key…";
			button.Modulate = new Color(1f, 1f, 0.2f);
		}

		private void CancelRebind()
		{
			if (_rebindButton != null)
			{
				var key = _rebindIsLeft ? _leftKeys[_rebindSlot] : _rightKeys[_rebindSlot];
				_rebindButton.Text    = KeyLabel(key);
				_rebindButton.Modulate = Colors.White;
			}
			_rebindSlot   = -1;
			_rebindButton = null;
		}

		// ── utility ───────────────────────────────────────────────────────────

		private void RefreshStartButton()
		{
			int count = 0;
			foreach (var active in _slotActive)
				if (active) count++;
			_startButton.Disabled = count < 2;
		}

		private static void MakeCell(HBoxContainer parent, string text, float width)
		{
			var label = new Label { Text = text, RectMinSize = new Vector2(width, 0) };
			parent.AddChild(label);
		}

		private static string KeyLabel(KeyList key) =>
			OS.GetKeystringFromScancode((int)key);
	}
}
