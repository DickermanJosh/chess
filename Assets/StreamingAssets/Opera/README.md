# Native Opera packages

From the sibling opera-engine repository, run:

    cargo build --manifest-path rust/Cargo.toml --release --locked --bin opera-uci
    python3 scripts/install_unity_engine.py --unity-project ../chess

Generated platform/architecture directories contain the executable and checksum
manifest and are ignored by Git. See the project README for setup and testing.
