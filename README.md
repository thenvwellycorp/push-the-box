# Push the Box – Mini Puzzle (Unity 2D)

A polished, modular 2D Sokoban puzzle game built for mobile (Android portrait orientation) and desktop/editor play in Unity & C#.

---

## Quick Start
1. Open the project in Unity Editor (Unity 6 / 2022+ / 6000.x).
2. Open the scene `Assets/Scenes/Game.unity`.
3. Press **Play** in the Unity Editor.
4. Use **WASD** or **Arrow Keys** to move, **Z** / **U** to Undo, and **R** to Restart.
5. On mobile/touch, use the **Virtual D-Pad** or **Swipe Gestures**.

---

## Key Features
- **Grid-Based Gameplay**: Strict discrete tile logic (`Vector2Int`) separated from smooth interpolated world rendering (`Vector3`).
- **Data-Driven Levels**: Levels are configured as `LevelData` ScriptableObjects with customizable dimensions, walls, boxes, targets, and star thresholds.
- **Undo System (Memento Pattern)**: Full multi-step undo history (`GameSnapshot` stack) tracking player and box positions and move count.
- **Deadlock Detection**: Automatically detects corner traps when a box is pushed into a non-target wall corner, alerting the player to undo or restart.
- **Save System**: Persists highest unlocked level, star ratings (1–3 stars), best move counts, and audio settings locally using JSON & PlayerPrefs.
- **Procedural Visuals & Audio**: Includes custom-generated 2D sprites (Player, Box, BoxOnTarget, Target, Wall, Floor, Star) and procedural audio synthesis fallbacks so the game is 100% playable immediately without external asset dependencies.
- **Mobile-Ready**: Portrait-oriented layout with auto-framing camera, responsive UI canvas (1080x1920), virtual D-pad, and touch swipe gesture detection.

---

## Editor Tools Menu
Under the top menu bar **PushTheBox**:
- **PushTheBox -> Setup Complete Game Scene**: Rebuilds the game scene and all prefab bindings.
- **PushTheBox -> Generate All Assets and Levels**: Re-generates procedural art sprites and 5 level assets.
- **PushTheBox -> Run Verification Tests**: Runs automated validation tests on mechanics, undo, deadlock, and save persistence.
