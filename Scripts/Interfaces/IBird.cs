using Godot;

namespace AchtungDieSpurve.Interfaces
{
	public interface IBird
	{
		int PlayerId { get; }
		bool IsAlive { get; }
		float Speed { get; }
		float TurnRate { get; }
		Vector2 Position { get; }
		float Angle { get; }

		void TurnLeft();
		void TurnRight();
		void Eliminate();
		void Reset(Vector2 spawnPosition, float spawnAngle);
	}
}
