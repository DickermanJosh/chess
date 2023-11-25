using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button flipBoardButton;
    public Camera viewCamera;
    public static bool isFlipped;

    private void Awake()
    {
        isFlipped = false;
    }

    public void FlipViewAxis()
    {
        var roll = 0;
        if (!isFlipped)
            roll = 180;
        
        foreach (var square in BoardManager.Board)
        {
            if (!square.isOccupied) continue;
            // Rotating each piece 180 degrees around the z axis 
            var pieceType = square.pieceOnSquare;
            var isWhite = square.isPieceWhite;
            BoardManager.Board[square.BoardPosInArray].RemovePieceFromSquare();
            BoardManager.Board[square.BoardPosInArray].AddPieceToSquare(pieceType, isWhite, !isFlipped);
        }
        
        viewCamera.transform.rotation = Quaternion.Euler(0, 0, roll);
        isFlipped = !isFlipped;
    }

}
