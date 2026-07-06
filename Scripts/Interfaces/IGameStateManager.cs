namespace AchtungDieSpurve.Interfaces
{
    public interface IGameStateManager
    {
        GamePhase CurrentPhase { get; }
        void TransitionTo(GamePhase phase);
    }
}
