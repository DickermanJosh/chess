using Core;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameState GameState { get; private set; }
    private IPlayer whitePlayer;
    private IPlayer blackPlayer;
    public string OpponentName { get; set; }
    public PieceColor MyColor { get; private set; }

    private static GameManager _instance;
    public static GameManager Instance => _instance;

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
            whitePlayer = new LocalPlayer(PieceColor.White);
            blackPlayer = new NetworkPlayer(PieceColor.Black);
        }
        else
        {
            whitePlayer = new NetworkPlayer(PieceColor.White);
            blackPlayer = new LocalPlayer(PieceColor.Black);
        }

        Debug.Log($"[Client GameManager] Started match as [{MyColor}] against [{OpponentName}]");
    }

    // psuedo update loop
    public void UpdateGameLoop()
    {
        // If it’s White’s turn
        Move nextMove = whitePlayer.GetMove(GameState);
        if (nextMove != null)
        {
            // ApplyMoveLocally(nextMove);
            // or send it to server if it’s the local player’s move, etc.
        }

        // If it’s Black’s turn
        Move nextBlackMove = blackPlayer.GetMove(GameState);
        if (nextBlackMove != null)
        {
            // ApplyMoveLocally(nextBlackMove);
            // ...
        }
    }
}
