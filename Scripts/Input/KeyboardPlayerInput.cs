using Godot;
using AchtungDieSpurve.Interfaces;

namespace AchtungDieSpurve.Input
{
    public class KeyboardPlayerInput : IPlayerInput
    {
        public int PlayerId { get; }
        public KeyList LeftKey { get; }
        public KeyList RightKey { get; }

        public KeyboardPlayerInput(int playerId, KeyList leftKey, KeyList rightKey)
        {
            PlayerId = playerId;
            LeftKey = leftKey;
            RightKey = rightKey;
        }

        public bool IsTurningLeft()  => Godot.Input.IsKeyPressed((int)LeftKey);
        public bool IsTurningRight() => Godot.Input.IsKeyPressed((int)RightKey);
    }
}
