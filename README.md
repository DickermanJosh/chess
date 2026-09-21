# Chess · Opera playtest

Unity 6000.0.54f1. The local Opera engine integration is on `opera-integration`.

Open `build/Opera Desktop/Opera Chess.app` on this Mac and choose **Play computer**.
Drag a piece onto a marked square, or click a piece and then its destination. Start a new game as White or Black,
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

## Appearance and controls

Menus and the play panel share a walnut, parchment, and aged-brass palette.
The board has a wooden frame and EB Garamond coordinates along the bottom and
left edges, staying upright and reversing correctly when playing Black. Player and Opera medallions sit above and below the board,
with a small indicator for the side to move.

- Drag to lift a piece; release on a legal destination to place it.
- Drop on the origin, outside the board, over a control, or on an illegal
  square to return the piece without making a move. Selection stays available.
- Escape, right-click, loss of application focus, changing turns, and entering
  review cancel a selection or drag.
- Sage dots mark quiet moves; rings mark captures, including en passant.
  The selected piece and an eligible castling rook get a sage outline.
- Amber squares show the last move independently of current selection,
  including when stepping through history.
- Select a king and click its rook, or drag the king onto the rook, to castle.
  The usual king destination also works. Both gestures use the shared legal
  move checks and send normal UCI/server destinations.
- Wooden lift, placement, capture, castling, return, and button sounds are
  synthesized locally, with damped attacks and three variations. **Sound on/off**
  in the play panel persists your preference. Review navigation is silent
  apart from button feedback.

The local-game menu remains disabled until that existing scene has a playable
implementation. Online play still uses the existing server connection.

Typography is bundled with its [SIL Open Font License](Assets/Resources/Opera/OFL.txt).
[EB Garamond source](https://github.com/google/fonts/tree/main/ofl/ebgaramond).
The textures, markers, avatars, and sound synthesis are original code; there
are no new runtime network or asset-service dependencies.

**Validation:** **Opera > Validate presentation rules** runs the existing 494
move/FEN and 603 SAN/history fixtures, perft, and 15 gesture cases covering
both castling sides/colors, lost rights, blocked paths, check, attacked transit
and destination squares, plus capture markers and independent highlight state.
The normal integration validation also includes these checks.

On this Windows checkout, validation used an isolated copy with installed Unity
6000.0.41f1 and test-framework 1.4.5 because the requested editor 6000.0.54f1
was not installed and package 1.5.1 did not resolve. The source project's
version and package manifest are unchanged. Rules and presentation checks pass.
Rendered previews additionally exercised selection, origin/illegal/off-board
drops, pointer ownership, turn-change cancellation, and castling markers.
They were rendered with an isolated preview camera; final URP appearance,
audio listening, and a real engine game should also be checked in the intended
editor. No native Windows Opera executable was present in this checkout.

Visual references: [menu](docs/appearance/menu.png), [board](docs/appearance/board.png).
