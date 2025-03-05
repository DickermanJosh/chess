using Core;
using UnityEngine;

public struct GameState
{
    public Board Board { get; }
    public string FEN { get; set; }
    public bool IsWhiteToMove { get; set; }
    public bool IsGameOver { get; set; }
}
