using System;
using UnityEngine;
using PushTheBox.Core;
using PushTheBox.Gameplay;
using PushTheBox.Level;

namespace PushTheBox.InputSystem
{
    /// <summary>
    /// Unifies keyboard, touch swipe gestures, and virtual D-pad buttons.
    /// Manages move execution and coordinates undo snapshotting.
    /// </summary>
    public class InputController : MonoBehaviour, IInputService
    {
        public static InputController Instance { get; private set; }

        [Header("Swipe Gesture Settings")]
        [SerializeField] private float minSwipeDistancePixels = 40f;

        public event Action<Vector2Int> OnMoveInput;
        public event Action OnUndoInput;
        public event Action OnRestartInput;

        private Vector2 touchStartPos;
        private bool isTrackingTouch = false;

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

        private void Update()
        {
            // Only accept input if currently playing
            if (GameStateManager.Instance != null && !GameStateManager.Instance.IsPlaying())
                return;

            HandleKeyboardInput();
            HandleTouchSwipeInput();
        }

        private void HandleKeyboardInput()
        {
            // Movement keys
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                SendMove(Vector2Int.up);
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                SendMove(Vector2Int.down);
            }
            else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SendMove(Vector2Int.left);
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                SendMove(Vector2Int.right);
            }

            // Shortcuts
            if (Input.GetKeyDown(KeyCode.R))
            {
                SendRestart();
            }
            else if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.U))
            {
                SendUndo();
            }
        }

        private void HandleTouchSwipeInput()
        {
            // Mobile Touch or Mouse drag simulation in Editor
            if (Input.GetMouseButtonDown(0))
            {
                touchStartPos = Input.mousePosition;
                isTrackingTouch = true;
            }
            else if (Input.GetMouseButtonUp(0) && isTrackingTouch)
            {
                isTrackingTouch = false;
                Vector2 touchEndPos = Input.mousePosition;
                Vector2 delta = touchEndPos - touchStartPos;

                if (delta.magnitude >= minSwipeDistancePixels)
                {
                    // Detect primary swipe axis
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    {
                        SendMove(delta.x > 0 ? Vector2Int.right : Vector2Int.left);
                    }
                    else
                    {
                        SendMove(delta.y > 0 ? Vector2Int.up : Vector2Int.down);
                    }
                }
            }
        }

        // Virtual D-pad UI callback methods
        public void OnDpadUp() => SendMove(Vector2Int.up);
        public void OnDpadDown() => SendMove(Vector2Int.down);
        public void OnDpadLeft() => SendMove(Vector2Int.left);
        public void OnDpadRight() => SendMove(Vector2Int.right);

        public void SendMove(Vector2Int direction)
        {
            if (direction == Vector2Int.zero) return;

            OnMoveInput?.Invoke(direction);

            if (LevelManager.Instance == null) return;

            // Capture snapshot candidate prior to attempting movement
            LevelManager.Instance.PrepareForMove();

            PlayerController player = FindAnyObjectByType<PlayerController>();
            if (player != null)
            {
                bool moved = player.TryMove(direction);
                if (!moved)
                {
                    // If movement failed (wall bump or blocked push), pop back the unused snapshot
                    if (UndoManager.Instance != null && UndoManager.Instance.CanUndo)
                    {
                        UndoManager.Instance.Pop();
                    }
                }
            }
        }

        public void SendUndo()
        {
            OnUndoInput?.Invoke();
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.UndoMove();
            }
        }

        public void SendRestart()
        {
            OnRestartInput?.Invoke();
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RestartLevel();
            }
        }
    }
}
