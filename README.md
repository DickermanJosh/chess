# Chess · Opera playtest

Unity 6000.0.54f1. The local Opera engine integration is on `opera-integration`.

Open `build/Opera Desktop/Opera Chess.app` on this Mac and choose **Play Opera**.
Click a piece, then a highlighted square. Start a new game as White or Black,
choose 0.25/1/3 seconds of engine thinking, and select a piece when promoting.
New game, Main menu and quitting stop the owned engine process. No chess server
is needed to play Opera. The app selects Opera's Morphy style, with heavy emphasis on activity, development and initiative.

## Review, resume, export and analysis

The right panel contains the full scrollable SAN transcript. Click a move or use
**Start / Back / Next / Live** to review. Left/Right and Home/End work too.
Reviewing leaves the live game intact; a pending engine reply updates the move
list while the board stays on the position you selected. **Resume from here**
automatically saves the original PGN, restores that exact position (including
castling, en passant, clocks and repetition), and replaces its continuation.
Your selected player color stays the same.

**Export game · PGN** saves a standard PGN and copies it to the clipboard.
**Show exports** opens the folder, normally `Documents/Opera Chess/Games`.
Send the `.pgn` file to the next development session. It includes every move,
the result, engine revision/hash, the viewed position and any available
White-relative evaluation annotations. New game, resume, menu and app exit
also archive a nonempty game.

Toggle **Analysis** to show/hide the evaluation meter and up to three candidate
lines. Analysis follows the displayed position on either player's turn and in
review. **Positive scores favour White; negative scores favour Black**, even
when playing Black. Scores use pawns; `+M3` reports mate for White in three.
Depth is shown per candidate. The meter is a visual scale, not a win probability.
An independent Opera process refines candidate lines using bounded searches
with excluded root moves; these are not exact equal-depth MultiPV rankings.
Hiding analysis stops its process. The playing search is independent, and stale
analysis/move callbacks are rejected after navigation, resume or a new game.

## Refresh the engine

From the sibling `opera-engine` checkout:

```sh
cargo build --manifest-path rust/Cargo.toml --release --locked --bin opera-uci
python3 scripts/install_unity_engine.py --unity-project ../chess
```

The generated native binary is ignored by Git. Reinstall it on a fresh checkout
or after rebuilding the engine. It lives in
`Assets/StreamingAssets/Opera/<platform>-<architecture>/opera-uci` (`.exe` on
Windows). Platforms: `macOS`, `Windows`, `Linux`; architectures: `arm64`,
`x86_64`. Each combination needs its matching native build. `OPERA_ENGINE_PATH`
can override the executable during development. This session validates macOS
on Apple Silicon; Windows/Linux app builds still need validation.

## Check and build

Open `Assets/Scenes/Init.unity` and enter Play mode for the normal menu flow.
The editor **Opera** menu provides rule/client validation and a macOS playtest
build. **Prepare AI scene** intentionally replaces AIGame with the board from
OnlineGame plus the Opera controller; use it only when regenerating that scene.

`OperaIntegrationTools.Validate` also runs in Unity batchmode. It verifies 494
independent reference move/FEN rows, start-position depth-3 and Kiwipete depth-2
perft, repetition, real engine replies, cancellation, restart and process cleanup.
Review validation adds 603 independent SAN fixtures and checks branching,
non-mutating history, special-move rights, repetition after resume, PGN results,
score perspective, streamed analysis and alternative root moves.
`OperaIntegrationTools.ValidateAndBuildMac` runs the checks and builds the app.

The reference data uses python-chess/chess 1.11.2, seed 20260918, and the engine
repo's two saved Stockfish games. Special positions cover castling, en passant,
pins, pawn edge files, promotions and terminal states.

The board rules are shared with the existing online code. The Opera adapter
uses local game state and redirected UCI pipes; it does not connect to TCP.
Online multiplayer and the unfinished Local Game menu remain separate work.

This is a local development build, not a notarized distribution release.
