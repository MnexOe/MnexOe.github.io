using Godot;
using System.Collections.Generic;
using AchtungDieSpurve.Config;
using AchtungDieSpurve.Interfaces;

namespace AchtungDieSpurve.Scoreboard
{
	public class ScoreboardManager : Control
	{
		private enum State { Idle, Countdown, Playing, Paused }

		private const int DefaultTarget = 30;

		private Viewport             _gameViewport;
		private CountdownManager     _countdown;
		private Node                 _gameInstance;
		private readonly List<Label> _scoreLabels = new List<Label>();
		private int[]                _scores      = System.Array.Empty<int>();
		private State                _state       = State.Idle;

		public override void _Ready()
		{
			PauseMode = PauseModeEnum.Process;

			_gameViewport = GetNode<Viewport>("Layout/GameArea/GameViewport");

			var countdownScene = ResourceLoader.Load<PackedScene>("res://Scenes/Countdown.tscn");
			_countdown = (CountdownManager)countdownScene.Instance();
			_gameViewport.AddChild(_countdown);
			_countdown.Connect(nameof(CountdownManager.CountdownFinished), this, nameof(OnCountdownFinished));

			int target = (GameData.Instance?.TargetScore ?? 0) > 0
				? GameData.Instance.TargetScore
				: DefaultTarget;

			GetNode<Label>("Layout/ScorePanel/Margin/Content/RaceToLabel").Text =
				$"You race to {target}";

			PopulatePlayers();
		}

		public override void _Input(InputEvent @event)
		{
			if (!(@event is InputEventKey key) || !key.Pressed || key.Echo)
				return;

			switch ((KeyList)(int)key.Scancode)
			{
				case KeyList.Space:
					HandleSpace();
					GetTree().SetInputAsHandled();
					break;
				case KeyList.Escape:
					GetTree().Paused = false;
					GetTree().ChangeScene("res://Scenes/Lobby.tscn");
					GetTree().SetInputAsHandled();
					break;
			}
		}

		public void RoundEnded(int winnerPlayerId)
		{
			var players = GameData.Instance?.Players;
			if (players == null) return;

			for (int i = 0; i < players.Count; i++)
			{
				if (players[i].PlayerId != winnerPlayerId) continue;
				_scores[i]++;
				if (i < _scoreLabels.Count)
					_scoreLabels[i].Text = _scores[i].ToString();
				break;
			}

			GetTree().Paused = false;
			_gameInstance    = null;
			_state           = State.Idle;
		}

		private void HandleSpace()
		{
			switch (_state)
			{
				case State.Idle:
					_state = State.Countdown;
					_countdown.StartCountdown();
					break;

				case State.Playing:
					_state = State.Paused;
					GetTree().Paused = true;
					break;

				case State.Paused:
					_state = State.Countdown;
					_countdown.StartCountdown();
					break;

				// Ignore Space while countdown is already running
			}
		}

		private void OnCountdownFinished()
		{
			if (_gameInstance != null)
			{
				GetTree().Paused = false;
			}
			else
			{
				StartRound();
			}

			_state = State.Playing;
		}

		private void PopulatePlayers()
		{
			var playerList = GetNode<VBoxContainer>("Layout/ScorePanel/Margin/Content/PlayerList");
			var players    = GameData.Instance?.Players ?? new List<IPlayerConfig>();

			_scores = new int[players.Count];

			foreach (var player in players)
			{
				var row = new HBoxContainer();
				playerList.AddChild(row);

				var nameLabel = new Label { Text = player.PlayerName };
				nameLabel.SizeFlagsHorizontal = (int)SizeFlags.ExpandFill;
				nameLabel.AddColorOverride("font_color", player.PlayerColor);
				row.AddChild(nameLabel);

				var scoreLabel = new Label { Text = "0" };
				scoreLabel.Align       = Label.AlignEnum.Right;
				scoreLabel.RectMinSize = new Vector2(36, 0);
				scoreLabel.AddColorOverride("font_color", player.PlayerColor);
				_scoreLabels.Add(scoreLabel);
				row.AddChild(scoreLabel);
			}
		}

		private void StartRound()
		{
			foreach (Node child in _gameViewport.GetChildren())
				if (child != _countdown)
					child.QueueFree();

			var packed = ResourceLoader.Load<PackedScene>("res://main.tscn");
			if (packed == null)
			{
				GD.PushWarning("ScoreboardManager: main.tscn not found");
				return;
			}

			_gameInstance = packed.Instance();
			_gameViewport.AddChild(_gameInstance);

			// Keep countdown rendered on top of the game
			_gameViewport.MoveChild(_countdown, _gameViewport.GetChildCount() - 1);
		}
	}
}
