using Godot;
using AchtungDieSpurve.Interfaces;

namespace AchtungDieSpurve.Config
{
    public class PlayerConfig : IPlayerConfig
    {
        public int PlayerId { get; }
        public string PlayerName { get; }
        public Color PlayerColor { get; }
        public IPlayerInput Input { get; }

        public PlayerConfig(int playerId, string playerName, Color playerColor, IPlayerInput input)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            PlayerColor = playerColor;
            Input = input;
        }
    }
}
