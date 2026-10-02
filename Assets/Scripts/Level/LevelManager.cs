using System;
using System.Collections.Generic;
using UnityEngine;
using PushTheBox.Core;
using PushTheBox.Gameplay;
using PushTheBox.Save;

namespace PushTheBox.Level
{
    public struct LevelCompletionData
    {
        public int levelId;
        public string levelName;
        public int moves;
        public int stars;
        public bool hasNextLevel;
    }

    /// <summary>
    /// Central manager coordinating level lifecycle: loading, restarting, moving, undoing,
    /// tracking move counts, and evaluating win conditions.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Level References")]
        [SerializeField] private List<LevelData> levels = new List<LevelData>();
        [SerializeField] private LevelLoader levelLoader;

        public int CurrentLevelIndex { get; private set; } = 0;
        public LevelData CurrentLevelData => (CurrentLevelIndex >= 0 && CurrentLevelIndex < levels.Count) ? levels[CurrentLevelIndex] : null;
        public int TotalLevels => levels.Count;
        public int MoveCount { get; private set; } = 0;

        // Events for UI and Audio decoupling
        public event Action<LevelData> OnLevelLoaded;
        public event Action OnLevelRestarted;
        public event Action<int> OnMoveCountChanged;
        public event Action<LevelCompletionData> OnLevelCompleted;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            if (levelLoader == null)
            {
                levelLoader = GetComponent<LevelLoader>();
                if (levelLoader == null)
                {
                    levelLoader = gameObject.AddComponent<LevelLoader>();
                }
            }
        }

        public void SetLevelsList(List<LevelData> newLevels)
        {
            levels = newLevels;
        }

        public void LoadLevel(int levelId)
        {
            int index = levels.FindIndex(l => l != null && l.levelId == levelId);
            if (index >= 0)
            {
                LoadLevelByIndex(index);
            }
            else
            {
                Debug.LogWarning($"[LevelManager] Level ID {levelId} not found, defaulting to index 0.");
                LoadLevelByIndex(0);
            }
        }

        public void LoadLevelByIndex(int index)
        {
            if (levels == null || levels.Count == 0)
            {
                Debug.LogError("[LevelManager] No levels configured in LevelManager!");
                return;
            }

            CurrentLevelIndex = Mathf.Clamp(index, 0, levels.Count - 1);
            LevelData data = CurrentLevelData;

            if (data == null)
            {
                Debug.LogError($"[LevelManager] LevelData at index {CurrentLevelIndex} is null!");
                return;
            }

            MoveCount = 0;
            if (UndoManager.Instance != null)
            {
                UndoManager.Instance.Clear();
            }
            if (DeadlockDetector.Instance != null)
            {
                DeadlockDetector.Instance.ResetDeadlock();
            }

            levelLoader.LoadLevel(data);

            if (levelLoader.CurrentPlayer != null)
            {
                levelLoader.CurrentPlayer.OnPlayerMoved += HandlePlayerMoved;
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SetState(GameState.Playing);
            }

            OnLevelLoaded?.Invoke(data);
            OnMoveCountChanged?.Invoke(MoveCount);
        }

        public void RestartLevel()
        {
            if (CurrentLevelData != null)
            {
                LoadLevelByIndex(CurrentLevelIndex);
                OnLevelRestarted?.Invoke();
            }
        }

        public void LoadNextLevel()
        {
            if (HasNextLevel())
            {
                LoadLevelByIndex(CurrentLevelIndex + 1);
            }
            else
            {
                Debug.Log("[LevelManager] All levels completed!");
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.SetState(GameState.LevelSelect);
                }
            }
        }

        public void LoadPreviousLevel()
        {
            if (CurrentLevelIndex > 0)
            {
                LoadLevelByIndex(CurrentLevelIndex - 1);
            }
        }

        public bool HasNextLevel()
        {
            return CurrentLevelIndex < levels.Count - 1;
        }

        /// <summary>
        /// Captures a snapshot before moving, to allow undoing this move later.
        /// </summary>
        public void PrepareForMove()
        {
            if (levelLoader.CurrentPlayer == null || UndoManager.Instance == null)
                return;

            Vector2Int playerPos = levelLoader.CurrentPlayer.GridPosition;
            var boxDict = new Dictionary<int, Vector2Int>();
            var boxes = GridManager.Instance.AllBoxes;
            for (int i = 0; i < boxes.Count; i++)
            {
                boxDict[boxes[i].BoxId] = boxes[i].GridPosition;
            }

            UndoManager.Instance.Record(new GameSnapshot(playerPos, boxDict, MoveCount));
        }

        private void HandlePlayerMoved(Vector2Int newPos, bool didPushBox)
        {
            MoveCount++;
            OnMoveCountChanged?.Invoke(MoveCount);

            if (didPushBox && DeadlockDetector.Instance != null)
            {
                DeadlockDetector.Instance.CheckDeadlocks();
            }

            CheckCompletion();
        }

        public void UndoMove()
        {
            if (GameStateManager.Instance != null && !GameStateManager.Instance.IsPlaying())
                return;

            if (UndoManager.Instance == null || !UndoManager.Instance.CanUndo)
                return;

            if (levelLoader.CurrentPlayer != null && levelLoader.CurrentPlayer.IsMoving)
                return;

            GameSnapshot snapshot = UndoManager.Instance.Pop();
            if (snapshot == null) return;

            // 1. Restore Player Position
            if (levelLoader.CurrentPlayer != null)
            {
                Vector3 playerWorldPos = GridManager.Instance.GridToWorld(snapshot.PlayerPosition);
                levelLoader.CurrentPlayer.SetGridPositionImmediate(snapshot.PlayerPosition, playerWorldPos);
            }

            // 2. Restore Boxes Position
            var boxes = GridManager.Instance.AllBoxes;
            for (int i = 0; i < boxes.Count; i++)
            {
                BoxController box = boxes[i];
                if (snapshot.BoxPositions.TryGetValue(box.BoxId, out Vector2Int restoredPos))
                {
                    Vector3 boxWorldPos = GridManager.Instance.GridToWorld(restoredPos);
                    box.RestorePosition(restoredPos, boxWorldPos);
                }
            }

            // 3. Restore Move Count
            MoveCount = snapshot.MoveCount;
            OnMoveCountChanged?.Invoke(MoveCount);

            // 4. Re-check deadlock
            if (DeadlockDetector.Instance != null)
            {
                DeadlockDetector.Instance.CheckDeadlocks();
            }
        }

        public void CheckCompletion()
        {
            if (GridManager.Instance != null && GridManager.Instance.AreAllBoxesOnTargets())
            {
                TriggerLevelCompleted();
            }
        }

        private void TriggerLevelCompleted()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.IsLevelCompleted())
                return;

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SetState(GameState.LevelCompleted);
            }

            LevelData data = CurrentLevelData;
            int stars = data != null ? data.CalculateStars(MoveCount) : 1;

            if (SaveManager.Instance != null && data != null)
            {
                SaveManager.Instance.SaveLevelResult(data.levelId, stars, MoveCount);
            }

            LevelCompletionData completionData = new LevelCompletionData
            {
                levelId = data != null ? data.levelId : CurrentLevelIndex + 1,
                levelName = data != null ? data.levelName : $"Level {CurrentLevelIndex + 1}",
                moves = MoveCount,
                stars = stars,
                hasNextLevel = HasNextLevel()
            };

            OnLevelCompleted?.Invoke(completionData);
        }
    }
}
