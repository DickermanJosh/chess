using System;
using System.Globalization;
using System.Linq;

namespace Opera
{
    /// <summary>UCI scores are relative to the root side to move, never to the GUI player.</summary>
    public sealed class UciSearchInfo
    {
        public int Depth { get; private set; }
        public int? Centipawns { get; private set; }
        public int? Mate { get; private set; }
        public bool HasScore => Centipawns.HasValue || Mate.HasValue;
        public bool IsBound { get; private set; }
        public string[] Pv { get; private set; } = Array.Empty<string>();
        public static UciSearchInfo Parse(string line)
        {
            if (!line.StartsWith("info ", StringComparison.Ordinal) || line.StartsWith("info string ", StringComparison.Ordinal)) return null;
            string[] words = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var info = new UciSearchInfo();
            for (int i = 1; i < words.Length; ++i)
            {
                if (words[i] == "pv") { info.Pv = words.Skip(i + 1).ToArray(); break; }
                if (words[i] == "depth" && i + 1 < words.Length && int.TryParse(words[i + 1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int depth)) info.Depth = depth;
                if (words[i] == "score" && i + 2 < words.Length && int.TryParse(words[i + 2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int score))
                {
                    if (words[i + 1] == "cp") info.Centipawns = score;
                    if (words[i + 1] == "mate") info.Mate = score;
                }
                if (words[i] == "lowerbound" || words[i] == "upperbound") info.IsBound = true;
            }
            return info.HasScore ? info : null;
        }
        public double WhiteScore(bool whiteToMove) => (whiteToMove ? 1 : -1) *
            (Mate.HasValue ? (Mate.Value > 0 ? 100 : -100) : Centipawns.GetValueOrDefault() / 100.0);
        public string WhiteLabel(bool whiteToMove)
        {
            if (!HasScore) return "—";
            int sign = whiteToMove ? 1 : -1;
            if (Mate.HasValue) return (WhiteScore(whiteToMove) > 0 ? "+M" : "-M") + Math.Abs(Mate.Value).ToString(CultureInfo.InvariantCulture);
            return (sign * Centipawns.Value / 100.0).ToString("+0.00;-0.00;0.00", CultureInfo.InvariantCulture) + (IsBound ? " (bound)" : "");
        }
    }
}
