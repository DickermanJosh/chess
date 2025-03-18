using Core;

public interface IPlayer
{
    // Called when it's this player's turn. Should eventually produce a valid move.
    // This could be synchronous or asynchronous depending on your design.
    // For example, you can have: Move GetMove(GameState state) 
    // or an async Task<Move> GetMoveAsync(GameState state)
    // Move GetMove(GameState gameState);

    void OnMove(Move move);

    PieceColor Color { get; }
}
