using Godot;

namespace AchtungDieSpurve.Interfaces
{
    public interface IPlayerConfig
    {
        int PlayerId { get; }
        string PlayerName { get; }
        Color PlayerColor { get; }
        IPlayerInput Input { get; }
    }
}
