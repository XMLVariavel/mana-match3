using System;

namespace BibleMatch3
{
    [Serializable]
    public sealed class RankingEntry
    {
        public string Uid;
        public string Nome;
        public string AvatarId;
        public string Modo;
        public string TemporadaId;
        public string ChallengeId;
        public int Score;
        public int MelhorCombo;
        public long AtualizadoEmUnix;
    }
}
