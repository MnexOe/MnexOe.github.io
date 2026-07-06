using Godot;

namespace AchtungDieSpurve.Interfaces
{
    public interface IPlayerInput
    {
        int PlayerId { get; }
        bool IsTurningLeft();
        bool IsTurningRight();
    }
}
