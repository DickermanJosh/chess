using System.Collections.Generic;
using UnityEngine;

public class MoveTracker
{
    public List<Move> moves;

    public MoveTracker()
    {
        moves = new List<Move>();
    }

    public void AddMove(Move move)
    {
        moves.Add(move);
    }

    public Move GetLastMove()
    {
        return moves.Count == 0 ? null : moves[moves.Count - 1];
    }
}
