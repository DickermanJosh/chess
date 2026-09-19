# Chess · Opera playtest

Unity 6000.0.54f1. The local Opera engine integration is on `opera-integration`.

Open `build/Opera Desktop/Opera Chess.app` on this Mac and choose **Play Opera**.
Click a piece, then a highlighted square. Start a new game as White or Black,
choose 0.25/1/3 seconds of engine thinking, and select a piece when promoting.
New game, Main menu and quitting stop the owned engine process. No chess server
is needed to play Opera. Its current handcrafted evaluation/search is unchanged.

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
The reference data uses python-chess/chess 1.11.2, seed 20260918, and the engine
repo's two saved Stockfish games. Special positions cover castling, en passant,
pins, pawn edge files, promotions and terminal states.

The board rules are shared with the existing online code. The Opera adapter
uses local game state and redirected UCI pipes; it does not connect to TCP.
Online multiplayer and the unfinished Local Game menu remain separate work.

This is a local development build, not a notarized distribution release.
