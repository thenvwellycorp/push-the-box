using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using PushTheBox.Core;
using PushTheBox.Gameplay;
using PushTheBox.Level;
using PushTheBox.Save;

namespace PushTheBox.EditorTools
{
    public static class GameplayTests
    {
        [MenuItem("PushTheBox/Run Verification Tests")]
        public static void RunTests()
        {
            Debug.Log("================ STARTING VERIFICATION TESTS ================");

            TestSaveSystem();
            TestGridAndPushMechanics();
            TestUndoSystem();
            TestDeadlockDetection();
            TestLevelDataSolvability();

            Debug.Log("================ ALL VERIFICATION TESTS PASSED SUCCESSFULLY ================");
        }

        private static void TestSaveSystem()
        {
            Debug.Log("[TEST] Testing SaveManager...");
            GameObject go = new GameObject("Test_SaveManager");
            SaveManager sm = go.AddComponent<SaveManager>();

            sm.ResetAllProgress();
            Assert(sm.IsLevelUnlocked(1), "Level 1 must be unlocked by default.");
            Assert(!sm.IsLevelUnlocked(2), "Level 2 must be locked initially.");

            sm.SaveLevelResult(1, 3, 2);
            Assert(sm.IsLevelUnlocked(2), "Level 2 must be unlocked after clearing Level 1.");

            var progress = sm.GetLevelProgress(1);
            Assert(progress != null && progress.stars == 3 && progress.bestMoves == 2, "Progress stars and best moves must be recorded.");

            Object.DestroyImmediate(go);
            Debug.Log("[TEST] SaveManager passed.");
        }

        private static void TestGridAndPushMechanics()
        {
            Debug.Log("[TEST] Testing Grid and Push Mechanics...");

            GameObject root = new GameObject("Test_GridRoot");
            GridManager gm = root.AddComponent<GridManager>();
            gm.InitializeGrid(5, 5);

            // Setup walls: perimeter
            for (int x = 0; x < 5; x++)
            {
                gm.RegisterWall(new Vector2Int(x, 0));
                gm.RegisterWall(new Vector2Int(x, 4));
            }
            for (int y = 1; y < 4; y++)
            {
                gm.RegisterWall(new Vector2Int(0, y));
                gm.RegisterWall(new Vector2Int(4, y));
            }

            // Target at (3, 2)
            GameObject targetGo = new GameObject("Target");
            TargetController target = targetGo.AddComponent<TargetController>();
            target.Initialize(new Vector2Int(3, 2), gm.GridToWorld(new Vector2Int(3, 2)));
            gm.RegisterTarget(target);

            // Box at (2, 2)
            GameObject boxGo = new GameObject("Box");
            BoxController box = boxGo.AddComponent<BoxController>();
            box.Initialize(1, new Vector2Int(2, 2), gm.GridToWorld(new Vector2Int(2, 2)));
            gm.RegisterBox(box);

            // Player at (1, 2)
            GameObject playerGo = new GameObject("Player");
            PlayerController player = playerGo.AddComponent<PlayerController>();
            player.Initialize(new Vector2Int(1, 2), gm.GridToWorld(new Vector2Int(1, 2)));

            Assert(!gm.AreAllBoxesOnTargets(), "Box should not be on target initially.");

            // Player tries to move LEFT into wall (0, 2) -> should fail
            bool movedIntoWall = player.TryMove(Vector2Int.left);
            Assert(!movedIntoWall, "Player moving into wall must return false.");
            Assert(player.GridPosition == new Vector2Int(1, 2), "Player position should not change on invalid move.");

            // Player pushes Box RIGHT -> should succeed
            bool pushedRight = player.TryMove(Vector2Int.right);
            Assert(pushedRight, "Pushing box into empty target must return true.");
            Assert(player.GridPosition == new Vector2Int(2, 2), "Player should move to (2, 2).");
            Assert(box.GridPosition == new Vector2Int(3, 2), "Box should move to (3, 2).");
            Assert(gm.AreAllBoxesOnTargets(), "All boxes should now be on target!");

            // Cleanup
            Object.DestroyImmediate(playerGo);
            Object.DestroyImmediate(boxGo);
            Object.DestroyImmediate(targetGo);
            Object.DestroyImmediate(root);

            Debug.Log("[TEST] Grid and Push Mechanics passed.");
        }

        private static void TestUndoSystem()
        {
            Debug.Log("[TEST] Testing Undo System...");

            GameObject root = new GameObject("Test_UndoRoot");
            UndoManager undoMgr = root.AddComponent<UndoManager>();
            undoMgr.Clear();

            Assert(!undoMgr.CanUndo, "UndoManager should initially have no undo steps.");

            var boxPos = new Dictionary<int, Vector2Int> { { 1, new Vector2Int(2, 2) } };
            undoMgr.Record(new GameSnapshot(new Vector2Int(1, 2), boxPos, 0));

            Assert(undoMgr.CanUndo, "UndoManager should now allow undo.");

            GameSnapshot popped = undoMgr.Pop();
            Assert(popped != null, "Popped snapshot must not be null.");
            Assert(popped.PlayerPosition == new Vector2Int(1, 2), "Snapshot PlayerPosition must match.");
            Assert(popped.BoxPositions[1] == new Vector2Int(2, 2), "Snapshot BoxPosition must match.");
            Assert(!undoMgr.CanUndo, "UndoManager history should be empty after pop.");

            Object.DestroyImmediate(root);
            Debug.Log("[TEST] Undo System passed.");
        }

        private static void TestDeadlockDetection()
        {
            Debug.Log("[TEST] Testing Deadlock Detection...");

            GameObject root = new GameObject("Test_DeadlockRoot");
            GridManager gm = root.AddComponent<GridManager>();
            DeadlockDetector detector = root.AddComponent<DeadlockDetector>();

            gm.InitializeGrid(5, 5);

            // Wall at North and East of (1, 3)
            gm.RegisterWall(new Vector2Int(1, 4)); // Up
            gm.RegisterWall(new Vector2Int(2, 3)); // Right

            // Box at (1, 3) (Not a target!)
            GameObject boxGo = new GameObject("Box");
            BoxController box = boxGo.AddComponent<BoxController>();
            box.Initialize(1, new Vector2Int(1, 3), gm.GridToWorld(new Vector2Int(1, 3)));
            gm.RegisterBox(box);

            detector.CheckDeadlocks();
            Assert(detector.IsDeadlocked, "Box cornered between Up and Right walls must be marked deadlocked.");

            // Cleanup
            Object.DestroyImmediate(boxGo);
            Object.DestroyImmediate(root);
            Debug.Log("[TEST] Deadlock Detection passed.");
        }

        private static void TestLevelDataSolvability()
        {
            Debug.Log("[TEST] Testing Level Data Integrity...");

            for (int i = 1; i <= 5; i++)
            {
                LevelData ld = AssetDatabase.LoadAssetAtPath<LevelData>($"Assets/ScriptableObjects/Levels/Level_{i}.asset");
                Assert(ld != null, $"Level_{i} asset must exist.");
                Assert(ld.boxes.Length > 0, $"Level_{i} must have at least one box.");
                Assert(ld.boxes.Length == ld.targets.Length, $"Level_{i} must have equal number of boxes and targets.");
                Assert(ld.width >= 5 && ld.height >= 5, $"Level_{i} dimensions must be at least 5x5.");
                Assert(ld.threeStarMoves > 0 && ld.twoStarMoves > ld.threeStarMoves, $"Level_{i} star thresholds must be configured.");
            }

            Debug.Log("[TEST] Level Data Integrity passed.");
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new System.Exception($"[ASSERTION FAILED] {message}");
            }
        }
    }
}
