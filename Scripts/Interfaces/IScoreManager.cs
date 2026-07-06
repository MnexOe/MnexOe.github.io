namespace AchtungDieSpurve.Interfaces
{
    public interface IScoreManager
    {
        int TargetScore { get; set; }

        int GetScore(int playerId);
        void AwardPoints(int playerId, int points);
        bool HasWinner();
        int GetWinnerId();
        void Reset();
    }
}
