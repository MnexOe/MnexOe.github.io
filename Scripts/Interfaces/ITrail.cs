using Godot;
using System.Collections.Generic;

namespace AchtungDieSpurve.Interfaces
{
    public interface ITrail
    {
        int OwnerId { get; }
        bool IsGapActive { get; }
        IReadOnlyList<Vector2> Segments { get; }

        void AddSegment(Vector2 position);
        void Clear();
    }
}
