using System.Collections.Generic;

namespace AchtungDieSpurve.Interfaces
{
    public interface IMatchManager
    {
        int PlayerCount { get; }
        int CurrentRound { get; }
        IReadOnlyList<int> AlivePlayers { get; }

        void StartMatch(IReadOnlyList<IPlayerConfig> players);
        void StartRound();
        void EliminatePlayer(int playerId);
        void EndRound();
        void EndMatch();
    }
}
