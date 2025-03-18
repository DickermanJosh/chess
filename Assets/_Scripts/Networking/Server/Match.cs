using Core;
using UnityEngine;
public class Match
{
    public RemotePlayerConnection WhitePlayer { get; private set; }
    public RemotePlayerConnection BlackPlayer { get; private set; }

    private GameState gameState;

    public Match(RemotePlayerConnection p1, RemotePlayerConnection p2)
    {
        WhitePlayer = p1;
        BlackPlayer = p2;

        // Init server-side gamestate
        gameState = new GameState();

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

        if (!LegalMovesHandler.IsMoveLegal(gameState, to, from)) { return; }

        // After confirming that the move is legal, make the move in the match's GameState
        gameState.Board.ApplyMove(from, to);

        gameState.UpdateMoveOrder();

        // Generate new FEN string from the newly updated board
        string newFen = FENUtils.GenerateFen(gameState);

        // Send the message to both clients
        WhitePlayer.Send($"FEN|{newFen}");
        BlackPlayer.Send($"FEN|{newFen}");
    }
}
