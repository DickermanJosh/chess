using Core;
using UnityEditor;
using UnityEngine;
public class Match
{
    public RemotePlayerConnection WhitePlayer { get; private set; }
    public RemotePlayerConnection BlackPlayer { get; private set; }

    private GameState gameState;
    public MoveTracker moveTracker;

    public Match(RemotePlayerConnection p1, RemotePlayerConnection p2)
    {
        WhitePlayer = p1;
        BlackPlayer = p2;

        // Init server-side gamestate
        gameState = new GameState();
        moveTracker = new MoveTracker();

        // Notify each player, clients will handle setting up a new client-side gamestate
        WhitePlayer.Send(ServerMessageHelper.GetMatchStart(BlackPlayer, PieceColor.White));
        BlackPlayer.Send(ServerMessageHelper.GetMatchStart(WhitePlayer, PieceColor.Black));
    }

    public void OnPlayerMove(RemotePlayerConnection player, string moveData)
    {
        // TODO: Validate the move, update gameState,
        // broadcast new FEN or updated board to both players, etc.

        // Check to make sure the move is coming from the correct player
        if ((gameState.ColorToMove == PieceColor.White && player == BlackPlayer) ||
            (gameState.ColorToMove == PieceColor.Black && player == WhitePlayer)) { return; }

        string[] tokens = moveData.Split('|');
        string f = tokens[1];
        string t = tokens[2];
        Debug.Log($"[Match] OnPlayerMove(): [to] = {t} [from] = {f}");

        // TODO: Check for pawn promotions

        Square from = gameState.Board.GetSquareFromNotation(f);
        Square to = gameState.Board.GetSquareFromNotation(t);

        if (from == null || to == null)
        {
            ServerMessageHelper.Log($"Error casting to squares from notation OnPlayerMove()");
            return;
        }

        Move move = new Move(from, to, gameState.ColorToMove);


        gameState.ValidateUpdateAndAppleMove(move);

        // Send the message to both clients
        WhitePlayer.Send($"FEN|{gameState.CurrentFen}|{move}");
        BlackPlayer.Send($"FEN|{gameState.CurrentFen}|{move}");
    }


    public bool IsPlayerInThisMatch(RemotePlayerConnection player)
    {
        return player == WhitePlayer || player == BlackPlayer;
    }

    /// <summary>
    /// Destroys this match instance
    /// TODO: Differentiate MATCH_END messages for checkmate, resignation, etc
    /// </summary>
    public void End()
    {
        ServerMessageHelper.Log("End() hit");
        WhitePlayer.Send("MATCH_END");
        BlackPlayer.Send("MATCH_END");
        ServerMessageHelper.Log("MATCH_END sent");

        WhitePlayer.IsInMatch = false;
        BlackPlayer.IsInMatch = false;

        ServerMessageHelper.Log($"Match ended between {WhitePlayer.PlayerName} and {BlackPlayer.PlayerName}");
    }

}
