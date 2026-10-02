using System;
using System.Collections;
using UnityEngine;
using PushTheBox.Core;

namespace PushTheBox.Gameplay
{
    /// <summary>
    /// Analyzes the board for deadlock conditions (e.g. box pushed into a non-target corner).
    /// Provides early warning to the player and triggers Game Over when a corner deadlock occurs.
    /// </summary>
    public class DeadlockDetector : MonoBehaviour
    {
        public static DeadlockDetector Instance { get; private set; }

        public event Action<bool> OnDeadlockStatusChanged;
        public event Action OnCornerDeadlock;

        public bool IsDeadlocked { get; private set; }

        private Coroutine deadlockGameOverCoroutine;

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

        public void CheckDeadlocks()
        {
            if (GridManager.Instance == null) return;

            bool wasDeadlocked = IsDeadlocked;
            IsDeadlocked = false;

            var boxes = GridManager.Instance.AllBoxes;
            for (int i = 0; i < boxes.Count; i++)
            {
                BoxController box = boxes[i];
                if (box == null) continue;

                Vector2Int pos = box.GridPosition;

                // If already on target, it's not a deadlock for this box
                if (GridManager.Instance.IsTarget(pos))
                    continue;

                // Check 4 corner configurations
                bool wallUp = IsImpassable(pos + Vector2Int.up);
                bool wallDown = IsImpassable(pos + Vector2Int.down);
                bool wallLeft = IsImpassable(pos + Vector2Int.left);
                bool wallRight = IsImpassable(pos + Vector2Int.right);

                // Top-Left corner
                if (wallUp && wallLeft)
                {
                    IsDeadlocked = true;
                    break;
                }
                // Top-Right corner
                if (wallUp && wallRight)
                {
                    IsDeadlocked = true;
                    break;
                }
                // Bottom-Left corner
                if (wallDown && wallLeft)
                {
                    IsDeadlocked = true;
                    break;
                }
                // Bottom-Right corner
                if (wallDown && wallRight)
                {
                    IsDeadlocked = true;
                    break;
                }
            }

            if (wasDeadlocked != IsDeadlocked)
            {
                OnDeadlockStatusChanged?.Invoke(IsDeadlocked);
            }

            if (IsDeadlocked)
            {
                OnCornerDeadlock?.Invoke();
                TriggerDeadlockGameOver();
            }
            else
            {
                CancelDeadlockGameOver();
            }
        }

        private void TriggerDeadlockGameOver()
        {
            if (GameStateManager.Instance != null && GameStateManager.Instance.IsLevelCompleted())
                return;

            CancelDeadlockGameOver();
            deadlockGameOverCoroutine = StartCoroutine(DeadlockGameOverRoutine());
        }

        private void CancelDeadlockGameOver()
        {
            if (deadlockGameOverCoroutine != null)
            {
                StopCoroutine(deadlockGameOverCoroutine);
                deadlockGameOverCoroutine = null;
            }
        }

        private IEnumerator DeadlockGameOverRoutine()
        {
            // Give 0.35s for the box push animation to complete and settle
            yield return new WaitForSeconds(0.35f);

            if (IsDeadlocked && GameStateManager.Instance != null && GameStateManager.Instance.IsPlaying())
            {
                GameStateManager.Instance.SetState(GameState.GameOver);
            }
        }

        private bool IsImpassable(Vector2Int pos)
        {
            return GridManager.Instance.IsWall(pos);
        }

        public void ResetDeadlock()
        {
            CancelDeadlockGameOver();
            if (IsDeadlocked)
            {
                IsDeadlocked = false;
                OnDeadlockStatusChanged?.Invoke(false);
            }
        }
    }
}
