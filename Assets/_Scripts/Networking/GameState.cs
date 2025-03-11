using Core;
using UnityEngine;

public class GameState
{
    public Board Board { get; set; }
    public string FEN { get; set; }
    public PieceColor ColorToMove { get; set; }
    public bool IsGameOver { get; set; }

    public GameState()
    {
        Board = new Board(64);
        Board.Init();

        FEN = FENUtils.StartFen;
        Board.LoadFEN(FEN);

        ColorToMove = PieceColor.White;
        IsGameOver = false;
    }

    public void UpdateGameState(string FEN, bool isGameOver)
    {
        IsGameOver = isGameOver;

        if (IsGameOver) { return; }

        Board.LoadFEN(FEN);

        if (ColorToMove == PieceColor.White)
        {
            ColorToMove = PieceColor.Black;
        }
        else
        {
            ColorToMove = PieceColor.White;
        }
    }
}
