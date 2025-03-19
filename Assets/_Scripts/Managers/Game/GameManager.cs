using System;
using System.Collections.Generic;
using Core;
using Managers;
using Render;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameState GameState { get; set; }
    private Player whitePlayer;
    private Player blackPlayer;
    public string OpponentName { get; set; }
    public PieceColor MyColor { get; private set; }

    private static GameManager _instance;
    public static GameManager Instance => _instance;

    public event Action StateUpdated;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(_instance);
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartOnlineGame(PieceColor myColor)
    {
        MyColor = myColor;
        GameState = new GameState();

        if (myColor == PieceColor.White)
        {
            whitePlayer = new Player(PieceColor.White);
            blackPlayer = new Player(PieceColor.Black);
        }
        else
        {
            whitePlayer = new Player(PieceColor.White);
            blackPlayer = new Player(PieceColor.Black);
        }

        Debug.Log($"[Client GameManager] Started match as [{MyColor}] against [{OpponentName}]");
    }

    public void UpdateGameStateFromFen(string fen)
    {
        // Load the piece segment in this client's gamestate and render it on the board
        List<int> changedSquares = FENUtils.ParseFenString(GameState, fen);
        BoardRenderer.Instance.RenderChangedSquares(changedSquares, GameState.Board);

        StateUpdated?.Invoke();
    }

    public bool IsMyTurn()
    {
        return MyColor == GameState.ColorToMove;
    }

    public Player GetMyPlayer()
    {
        if (MyColor == PieceColor.White)
        {
            return whitePlayer;
        }

        return blackPlayer;
    }

    public void ResetToDefault()
{
    GameState = null;
    OpponentName = "";
    MyColor = PieceColor.None;

    whitePlayer = null;
    blackPlayer = null;

    StateUpdated = null;

    Debug.Log("[GameManager] Reset to default state.");
}
}
