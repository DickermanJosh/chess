using System;
using UnityEngine;

public class GenerateGraphicalBoard : MonoBehaviour
{
    public GameObject lightSquare;

    public GameObject darkSquare;

    public Transform board;

    private static GenerateGraphicalBoard _instance;
    public static GenerateGraphicalBoard Instance => _instance;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
    }

    private void Start()
    {
        InitBoard();
        FENHandler.SetBoardFromFen(FENHandler.StartFen);

        Debug.Log("FEN: " + FENHandler.GetCurrentFenPos());
    }

    private void InitBoard()
    {
        var count = 0;
        // Board gets init from a1 - b1 - c1 - d1 - e1 - f1 - g1 - h1 - a2 ...
        // Saved in array as    0    1    2    3    4    5    6    7    8  ...
        for (var file = 0; file < 8; file++)
        {
            for (var rank = 0; rank < 8; rank++)
            {
                var isLightSquare = (file + rank) % 2 != 0; // bool to declare color
                GameObject squareObject;

                if (isLightSquare)
                {
                    squareObject = Instantiate(lightSquare, new Vector2(-3.5f + file, -3.5f + rank),
                        Quaternion.identity);
                }
                else
                {
                    squareObject = Instantiate(darkSquare, new Vector2(-3.5f + file, -3.5f + rank),
                        Quaternion.identity);
                }

                //  Saving the Square component of the instantiated square into the board array
                var squareComponent = squareObject.GetComponent<Square>();
                if (squareComponent != null)
                {
                    // Converting the file to a char value for ease of use inside the inspector
                    var fileAsChar = file switch
                    {
                        0 => 'A',
                        1 => 'B',
                        2 => 'C',
                        3 => 'D',
                        4 => 'E',
                        5 => 'F',
                        6 => 'G',
                        7 => 'H',
                        _ => 'z' // Invalid case
                    };
                    
                    // Defining all of the necessary information inside the square script of the instantiated square object
                    squareComponent.numOfSquaresToEdge = PrecomputeAvailableDistances(rank, file);
                    squareComponent.pos = new Vector2(file, rank);
                    squareComponent.rank = rank; // WAS RANK + 1
                    squareComponent.file = fileAsChar;
                    squareComponent.isLightSquare = isLightSquare;
                    squareComponent.BoardPosInArray = count;
                    // Adding the Square script to the board array
                    BoardManager.Board[count] = squareComponent;
                }
                else
                {
                    Debug.LogWarning("Square component not found on the GameObject.");
                    return;
                }

                squareObject.transform.SetParent(board);
                count++;
            }
        }

        if (BoardManager.Board.Length != 64)
        {
            Debug.Log("Squares in arr: " + BoardManager.Board.Length);
        }
    }

    private static int[] PrecomputeAvailableDistances(int rank, int file)
    {
        var numUp = 7 - rank;
        var numRight = 7 - file;

        int[] numSquaresToEdge =
        {
            numUp,                      // Up
            rank,                       // Down
            file,                       // Left
            numRight,                   // Right
            Math.Min(numUp, numRight),  // Up-Right
            Math.Min(rank, file),       // Down-Left
            Math.Min(numUp, file),      // Up-Left
            Math.Min(rank, numRight)    // Down-Right
        };
        return numSquaresToEdge;
    }
}