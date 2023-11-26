using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PawnPromotionHandler : MonoBehaviour
{
    private static PawnPromotionHandler _instance;
    public static PawnPromotionHandler Instance => _instance;
    
    public delegate void PromotionCompletedHandler();
    public event PromotionCompletedHandler OnPromotionCompleted;
    
    public int promotedSquare;
    
    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }
    public GameObject WhitePromotionTable;
    public GameObject BlackPromotionTable;

    
    public void TogglePromotionTable(bool white)
    {
        if (!white)
            BlackPromotionTable.SetActive(!BlackPromotionTable.activeSelf);
        else
            WhitePromotionTable.SetActive(!WhitePromotionTable.activeSelf);
    }

    public void WhiteQueen()
    {
        BoardManager.Board[promotedSquare].RemovePieceFromSquare();
        BoardManager.Board[promotedSquare].AddPieceToSquare("Queen", true, UIManager.isFlipped);
        TogglePromotionTable(true);
        OnPromotionCompleted?.Invoke();
    }
    public void BlackQueen()
    {
        BoardManager.Board[promotedSquare].RemovePieceFromSquare();
        BoardManager.Board[promotedSquare].AddPieceToSquare("Queen", false, UIManager.isFlipped);
        TogglePromotionTable(false);
        OnPromotionCompleted?.Invoke();
    }
    public void WhiteRook()
    {
        BoardManager.Board[promotedSquare].RemovePieceFromSquare();
        BoardManager.Board[promotedSquare].AddPieceToSquare("Rook", true, UIManager.isFlipped);
        TogglePromotionTable(true);
        OnPromotionCompleted?.Invoke();
    }
    public void BlackRook()
    {
        BoardManager.Board[promotedSquare].RemovePieceFromSquare();
        BoardManager.Board[promotedSquare].AddPieceToSquare("Rook", false, UIManager.isFlipped);
        TogglePromotionTable(false);
        OnPromotionCompleted?.Invoke();
    }
    public void WhiteBishop()
    {
        BoardManager.Board[promotedSquare].RemovePieceFromSquare();
        BoardManager.Board[promotedSquare].AddPieceToSquare("Bishop", true, UIManager.isFlipped);
        TogglePromotionTable(true);
        OnPromotionCompleted?.Invoke();
    }
    public void BlackBishop()
    {
        BoardManager.Board[promotedSquare].RemovePieceFromSquare();
        BoardManager.Board[promotedSquare].AddPieceToSquare("Bishop", false, UIManager.isFlipped);
        TogglePromotionTable(false);
        OnPromotionCompleted?.Invoke();
    }
    public void WhiteKnight()
    {
        BoardManager.Board[promotedSquare].RemovePieceFromSquare();
        BoardManager.Board[promotedSquare].AddPieceToSquare("Knight", true, UIManager.isFlipped);
        TogglePromotionTable(true);
        OnPromotionCompleted?.Invoke();
    }
    public void BlackKnight()
    {
        BoardManager.Board[promotedSquare].RemovePieceFromSquare();
        BoardManager.Board[promotedSquare].AddPieceToSquare("Knight", false, UIManager.isFlipped);
        TogglePromotionTable(false);
        OnPromotionCompleted?.Invoke();
    }
}
