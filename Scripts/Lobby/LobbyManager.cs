using Godot;
using System.Collections.Generic;
using AchtungDieSpurve.Config;
using AchtungDieSpurve.Input;
using AchtungDieSpurve.Interfaces;

namespace AchtungDieSpurve.Lobby
{
	public class LobbyManager : Control
	{
		private const int MaxPlayers = 4;

		private static readonly Color[] PlayerColors =
		{
			new Color("#9B5DE5"),
			new Color("#FEE440"),
			new Color("#00BBF9"),
			new Color("#F15BB5"),
		};

		private static readonly string[] PlayerNames =
		{
			"DuckDuckGo",
			"Flappy McFlapface",
			"Tornado Toucan",
			"Eggward Featherhands"
		};

		private static readonly int[] TargetScoreOptions = { 10, 20, 30, 50, 100 };

		private readonly KeyList[] _leftKeys = new KeyList[MaxPlayers];
		private readonly KeyList[] _rightKeys = new KeyList[MaxPlayers];
		private readonly bool[] _hasLeftKey = new bool[MaxPlayers];
		private readonly bool[] _hasRightKey = new bool[MaxPlayers];

		private readonly PanelContainer[] _playerRows = new PanelContainer[MaxPlayers];
		private readonly Label[] _nameLabels = new Label[MaxPlayers];
		private readonly Label[] _leftKeyLabels = new Label[MaxPlayers];
		private readonly Label[] _rightKeyLabels = new Label[MaxPlayers];

		private readonly HashSet<KeyList> _usedKeys = new HashSet<KeyList>();

		private int _selectedPlayer = -1;
		private int _selectionStep = 0; // 0 = none, 1 = left key, 2 = right key

		private int _targetScoreIndex = 2;

		private Label _targetScoreLabel;
		private Button _startButton;

		public override void _Ready()
		{
			LoadUiReferences();
			ConnectPlayerRows();

			UpdateAllPlayerRows();
			UpdateStartButton();
		}

		public override void _Input(InputEvent @event)
		{
			if (_selectedPlayer < 0 || _selectionStep == 0)
				return;

			if (!(@event is InputEventKey keyEvent))
				return;

			if (!keyEvent.Pressed || keyEvent.Echo)
				return;

			KeyList key = (KeyList)keyEvent.Scancode;

			// Ignore bare modifier keys
			if (key == KeyList.Shift || key == KeyList.Control || key == KeyList.Alt || key == KeyList.Meta)
				return;

			if (_usedKeys.Contains(key))
				return;

			if (_selectionStep == 1)
			{
				_leftKeys[_selectedPlayer] = key;
				_hasLeftKey[_selectedPlayer] = true;
				_usedKeys.Add(key);
				_selectionStep = 2;
			}
			else if (_selectionStep == 2)
			{
				_rightKeys[_selectedPlayer] = key;
				_hasRightKey[_selectedPlayer] = true;
				_usedKeys.Add(key);
				_selectedPlayer = -1;
				_selectionStep = 0;
			}

			UpdateAllPlayerRows();
			UpdateStartButton();
		}

		private void LoadUiReferences()
		{
			string basePath = "MarginContainer/VBoxContainer";

			for (int i = 0; i < MaxPlayers; i++)
			{
				string rowPath = $"{basePath}/TableRow/PlayerRows/Player{i + 1}Row";

				_playerRows[i] = GetNode<PanelContainer>(rowPath);
				_nameLabels[i] = GetNode<Label>($"{rowPath}/HBoxContainer/NameLabel");
				_leftKeyLabels[i] = GetNode<Label>($"{rowPath}/HBoxContainer/LeftKeyLabel");
				_rightKeyLabels[i] = GetNode<Label>($"{rowPath}/HBoxContainer/RightKeyLabel");

				_nameLabels[i].Text = PlayerNames[i];

				_playerRows[i].MouseFilter = MouseFilterEnum.Stop;
				_nameLabels[i].MouseFilter = MouseFilterEnum.Ignore;
				_leftKeyLabels[i].MouseFilter = MouseFilterEnum.Ignore;
				_rightKeyLabels[i].MouseFilter = MouseFilterEnum.Ignore;
			}

			_targetScoreLabel = GetNode<Label>($"{basePath}/TargetScoreRow/TargetScoreLabel");
			_startButton = GetNode<Button>($"{basePath}/StartButton");

			_targetScoreLabel.Text = TargetScoreOptions[_targetScoreIndex].ToString();

			GetNode<Button>($"{basePath}/TargetScoreRow/DecreaseTargetScoreButton")
				.Connect("pressed", this, nameof(OnTargetScoreDecrease));

			GetNode<Button>($"{basePath}/TargetScoreRow/IncreaseTargetScoreButton")
				.Connect("pressed", this, nameof(OnTargetScoreIncrease));

			_startButton.Connect("pressed", this, nameof(OnStartPressed));
		}

		private void ConnectPlayerRows()
		{
			for (int i = 0; i < MaxPlayers; i++)
			{
				_playerRows[i].Connect(
					"gui_input",
					this,
					nameof(OnPlayerRowGuiInput),
					new Godot.Collections.Array { i }
				);
			}
		}

		private void OnPlayerRowGuiInput(InputEvent @event, int playerIndex)
		{
			if (!(@event is InputEventMouseButton mouseEvent))
				return;

			if (!mouseEvent.Pressed)
				return;

			if (mouseEvent.ButtonIndex != (int)ButtonList.Left)
				return;

			OnPlayerRowClicked(playerIndex);
		}

		private void OnPlayerRowClicked(int playerIndex)
		{
			bool hasControls = _hasLeftKey[playerIndex] || _hasRightKey[playerIndex];

			if (hasControls)
			{
				ClearPlayerControls(playerIndex);
				if (_selectedPlayer == playerIndex)
				{
					_selectedPlayer = -1;
					_selectionStep = 0;
				}
			}
			else
			{
				if (_selectedPlayer == playerIndex)
				{
					_selectedPlayer = -1;
					_selectionStep = 0;
				}
				else
				{
					_selectedPlayer = playerIndex;
					_selectionStep = 1;
				}
			}

			UpdateAllPlayerRows();
			UpdateStartButton();
		}

		private void ClearPlayerControls(int playerIndex)
		{
			if (_hasLeftKey[playerIndex])
				_usedKeys.Remove(_leftKeys[playerIndex]);

			if (_hasRightKey[playerIndex])
				_usedKeys.Remove(_rightKeys[playerIndex]);

			_hasLeftKey[playerIndex] = false;
			_hasRightKey[playerIndex] = false;
		}

		private void UpdateAllPlayerRows()
		{
			for (int i = 0; i < MaxPlayers; i++)
				UpdatePlayerRow(i);
		}

		private void UpdatePlayerRow(int index)
		{
			bool currentlySelecting = _selectedPlayer == index;

			if (_hasLeftKey[index])
				_leftKeyLabels[index].Text = KeyLabel(_leftKeys[index]);
			else if (currentlySelecting && _selectionStep == 1)
				_leftKeyLabels[index].Text = "Press a key...";
			else
				_leftKeyLabels[index].Text = "-";

			if (_hasRightKey[index])
				_rightKeyLabels[index].Text = KeyLabel(_rightKeys[index]);
			else if (currentlySelecting && _selectionStep == 2)
				_rightKeyLabels[index].Text = "Press a key...";
			else
				_rightKeyLabels[index].Text = "-";

			_playerRows[index].Modulate = currentlySelecting
				? new Color(1f, 1f, 0.75f)
				: Colors.White;
		}

		private void UpdateStartButton()
		{
			_startButton.Disabled = GetParticipatingPlayerCount() < 1;
		}

		private int GetParticipatingPlayerCount()
		{
			int count = 0;

			for (int i = 0; i < MaxPlayers; i++)
			{
				if (IsParticipating(i))
					count++;
			}

			return count;
		}

		private bool IsParticipating(int index)
		{
			return _hasLeftKey[index] && _hasRightKey[index];
		}

		private void OnTargetScoreDecrease()
		{
			if (_targetScoreIndex <= 0)
				return;

			_targetScoreIndex--;
			_targetScoreLabel.Text = TargetScoreOptions[_targetScoreIndex].ToString();
		}

		private void OnTargetScoreIncrease()
		{
			if (_targetScoreIndex >= TargetScoreOptions.Length - 1)
				return;

			_targetScoreIndex++;
			_targetScoreLabel.Text = TargetScoreOptions[_targetScoreIndex].ToString();
		}

		private void OnStartPressed()
		{
			var players = new List<IPlayerConfig>();

			for (int i = 0; i < MaxPlayers; i++)
			{
				if (!IsParticipating(i))
					continue;

				var input = new KeyboardPlayerInput(i, _leftKeys[i], _rightKeys[i]);
				players.Add(new PlayerConfig(i, PlayerNames[i], PlayerColors[i], input));
			}

			if (players.Count < 1)
				return;

			GameData.Instance.Players = players;
			GameData.Instance.TargetScore = TargetScoreOptions[_targetScoreIndex];

			GetTree().ChangeScene("res://Scenes/Scoreboard.tscn");
		}

		private static string KeyLabel(KeyList key)
		{
			return OS.GetScancodeString((uint)key);
		}
	}
}
