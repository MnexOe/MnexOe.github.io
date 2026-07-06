using Godot;

namespace AchtungDieSpurve.Interfaces
{
    public interface ICollidable
    {
        bool Intersects(Vector2 point, float radius);
    }
}
