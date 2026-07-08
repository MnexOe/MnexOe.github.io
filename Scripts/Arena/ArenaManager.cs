using Godot;
using System;
using System.Collections.Generic;
using AchtungDieSpurve.Config;
using AchtungDieSpurve.Input;
using AchtungDieSpurve.Interfaces;
using AchtungDieSpurve.Scoreboard;

namespace AchtungDieSpurve.Arena
{
	public class ArenaManager : Node2D
	{
		private const float WallMargin = 80f;
		private const float MinSpawnDistance = 150f;
		private const float MapWidth = 1024f;
		private const float MapHeight = 600f;
private Random _rng;
		private int _eliminationRank;
		private HashSet<int> _alivePlayers;
		private Dictionary<int, int> _pendingPoints;
		private bool _roundEnded;

		public override void _Ready()
		{
			_rng = new Random();

			var players = GameData.Instance?.Players;
			if (players == null || players.Count == 0)
				return;

			_eliminationRank = 0;
			_roundEnded = false;
			_alivePlayers = new HashSet<int>();
			_pendingPoints = new Dictionary<int, int>();

			foreach (var p in players)
				_alivePlayers.Add(p.PlayerId);

			var spurvenScene = ResourceLoader.Load<PackedScene>("res://Scenes/spurven.tscn");
			var drawerScene  = ResourceLoader.Load<PackedScene>("res://Scenes/Drawer.tscn");

			SpawnPlayers(players, spurvenScene, drawerScene);
		}

		private void SpawnPlayers(
			List<IPlayerConfig> players,
			PackedScene spurvenScene,
			PackedScene drawerScene)
		{
			var positions = GenerateSpawnPositions(players.Count);

			for (int i = 0; i < players.Count; i++)
			{
				var player = players[i];
				float angle = (float)(_rng.NextDouble() * Mathf.Tau);

				var bird   = spurvenScene.Instance();
				var drawer = (Drawer)drawerScene.Instance();
				bird.AddChild(drawer);

				var birdNode = (Node2D)bird;
				birdNode.Position = positions[i];
				birdNode.Rotation = angle;

				bird.Set("player_id", player.PlayerId);

				if (player.Input is KeyboardPlayerInput kpi)
				{
					bird.Set("left_key",  (int)kpi.LeftKey);
					bird.Set("right_key", (int)kpi.RightKey);
				}

				AddChild(bird); // triggers _Ready() on bird and drawer

				birdNode.ZIndex = 1;
				drawer.SetColor(player.PlayerColor);
				bird.Connect("died", this, nameof(OnBirdDied));
			}
		}

		private void OnBirdDied(int playerId)
		{
			if (_roundEnded || !_alivePlayers.Contains(playerId))
				return;

			_alivePlayers.Remove(playerId);
			_pendingPoints[playerId] = _eliminationRank++;

			if (_alivePlayers.Count == 1)
			{
				foreach (int survivorId in _alivePlayers)
					_pendingPoints[survivorId] = _eliminationRank++;
				EndRound();
			}
			else if (_alivePlayers.Count == 0)
			{
				EndRound();
			}
		}

		private void EndRound()
		{
			_roundEnded = true;
			var scoreboard = GetTree().Root.GetNodeOrNull<ScoreboardManager>("Scoreboard");
			scoreboard?.RoundEnded(_pendingPoints);
		}

		private List<Vector2> GenerateSpawnPositions(int count)
		{
			float minX = WallMargin;
			float maxX = MapWidth  - WallMargin;
			float minY = WallMargin;
			float maxY = MapHeight - WallMargin;

			var chosen = new List<Vector2>(count);

			for (int i = 0; i < count; i++)
			{
				Vector2 pos;
				int tries = 0;

				do
				{
					float x = minX + (float)(_rng.NextDouble() * (maxX - minX));
					float y = minY + (float)(_rng.NextDouble() * (maxY - minY));
					pos = new Vector2(x, y);
					tries++;
				}
				while (!IsFarEnough(pos, chosen) && tries < 200);

				chosen.Add(pos);
			}

			return chosen;
		}

		private static bool IsFarEnough(Vector2 candidate, List<Vector2> existing)
		{
			foreach (var pos in existing)
				if (candidate.DistanceTo(pos) < MinSpawnDistance)
					return false;
			return true;
		}
	}
}
