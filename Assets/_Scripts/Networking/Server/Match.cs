public class Match
{
    public RemotePlayerConnection WhitePlayer { get; private set; }
    public RemotePlayerConnection BlackPlayer { get; private set; }

    // or "Board" if that's your structure
    private GameState gameState;

    public Match(RemotePlayerConnection p1, RemotePlayerConnection p2)
    {
        // For example, let p1 be White, p2 be Black:
        WhitePlayer = p1;
        BlackPlayer = p2;

        // Initialize your game logic
        gameState = new GameState();
        gameState.Init();

        // Notify each player
        WhitePlayer.Send($"MATCH_START|WHITE|{BlackPlayer.PlayerName}");
        BlackPlayer.Send($"MATCH_START|BLACK|{WhitePlayer.PlayerName}");
    }

    public void OnPlayerMove(RemotePlayerConnection player, string moveData)
    {
        // TODO: Validate the move, update gameState,
        // broadcast new FEN or updated board to both players, etc.
    }
}
