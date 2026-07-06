using Godot;

namespace AchtungDieSpurve.Interfaces
{
	public interface IArena
	{
		Vector2 Size { get; }
		bool IsOutOfBounds(Vector2 position);
		Vector2 GetRandomSpawnPosition();
		float GetRandomSpawnAngle();
	}
}
