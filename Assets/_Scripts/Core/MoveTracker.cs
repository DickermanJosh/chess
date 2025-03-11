using NUnit.Framework;
using UnityEngine;

public class MoveTracker : MonoBehaviour
{
    public bool isWhiteToMove = true;

    public System.Collections.Generic.List<Move> moves;

    private static MoveTracker _instance;
    public static MoveTracker Instance => _instance;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(_instance);
        }

        _instance = this;
    }

    public void AddMove(Move move)
    {
        moves.Add(move);
    }

    public Move GetLastMove()
    {
        return moves[moves.Count - 1];
    }


}
