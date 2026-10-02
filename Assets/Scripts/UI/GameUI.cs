using UnityEngine;
using UnityEngine.UI;
using PushTheBox.Core;
using PushTheBox.Level;
using PushTheBox.Audio;
using PushTheBox.Gameplay;
using PushTheBox.InputSystem;
using PushTheBox.Save;

namespace PushTheBox.UI
{
    /// <summary>
    /// HUD during gameplay: shows current level, move counter, star goals,
    /// undo and restart buttons, virtual D-pad, and deadlock alert.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("HUD Labels")]
        [SerializeField] private Text levelTitleText;
        [SerializeField] private Text moveCountText;
        [SerializeField] private Text targetMovesText;
        [SerializeField] private Text coinCountText;

        [Header("Control Buttons")]
        [SerializeField] private Button undoButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        [Header("Virtual D-Pad Buttons")]
        [SerializeField] private Button dpadUpButton;
        [SerializeField] private Button dpadDownButton;
        [SerializeField] private Button dpadLeftButton;
        [SerializeField] private Button dpadRightButton;

        [Header("Deadlock Banner")]
        [SerializeField] private GameObject deadlockBanner;
        [SerializeField] private Text deadlockText;

        private void Awake()
        {
            EnsureCoinUI();
        }

        private void EnsureCoinUI()
        {
            if (coinCountText != null) return;

            Transform topBar = transform.Find("TopBar");
            Transform parent = topBar != null ? topBar : transform;
            Vector2 aMin = topBar != null ? new Vector2(0.65f, 0f) : new Vector2(0.70f, 0.90f);
            Vector2 aMax = topBar != null ? new Vector2(1f, 0.45f) : new Vector2(0.95f, 0.96f);
            Font font = levelTitleText != null ? levelTitleText.font : null;

            coinCountText = CoinUIHelper.CreateBadge(parent, font, aMin, aMax, "HUDCoinBadge");
        }

        private void OnEnable()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelLoaded += HandleLevelLoaded;
                LevelManager.Instance.OnMoveCountChanged += HandleMoveCountChanged;
            }

            if (UndoManager.Instance != null)
            {
                UndoManager.Instance.OnCanUndoChanged += HandleCanUndoChanged;
                UpdateUndoButtonState(UndoManager.Instance.CanUndo);
            }

            if (DeadlockDetector.Instance != null)
            {
                DeadlockDetector.Instance.OnDeadlockStatusChanged += HandleDeadlockStatusChanged;
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnCoinsChanged += HandleCoinsChanged;
                UpdateCoinsUI(SaveManager.Instance.Coins);
            }

            // Hook button listeners
            if (undoButton != null) undoButton.onClick.AddListener(OnUndoClicked);
            if (restartButton != null) restartButton.onClick.AddListener(OnRestartClicked);
            if (menuButton != null) menuButton.onClick.AddListener(OnMenuClicked);

            if (dpadUpButton != null) dpadUpButton.onClick.AddListener(() => SendDpad(Vector2Int.up));
            if (dpadDownButton != null) dpadDownButton.onClick.AddListener(() => SendDpad(Vector2Int.down));
            if (dpadLeftButton != null) dpadLeftButton.onClick.AddListener(() => SendDpad(Vector2Int.left));
            if (dpadRightButton != null) dpadRightButton.onClick.AddListener(() => SendDpad(Vector2Int.right));

            if (deadlockBanner != null)
                deadlockBanner.SetActive(false);
        }

        private void OnDisable()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelLoaded -= HandleLevelLoaded;
                LevelManager.Instance.OnMoveCountChanged -= HandleMoveCountChanged;
            }

            if (UndoManager.Instance != null)
            {
                UndoManager.Instance.OnCanUndoChanged -= HandleCanUndoChanged;
            }

            if (DeadlockDetector.Instance != null)
            {
                DeadlockDetector.Instance.OnDeadlockStatusChanged -= HandleDeadlockStatusChanged;
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnCoinsChanged -= HandleCoinsChanged;
            }

            if (undoButton != null) undoButton.onClick.RemoveListener(OnUndoClicked);
            if (restartButton != null) restartButton.onClick.RemoveListener(OnRestartClicked);
            if (menuButton != null) menuButton.onClick.RemoveListener(OnMenuClicked);
        }

        private void HandleCoinsChanged(int totalCoins)
        {
            UpdateCoinsUI(totalCoins);
        }

        private void UpdateCoinsUI(int coins)
        {
            if (coinCountText != null)
            {
                coinCountText.text = coins.ToString();
            }
        }

        private void HandleLevelLoaded(LevelData data)
        {
            if (levelTitleText != null && data != null)
            {
                levelTitleText.text = data.levelName;
            }

            if (targetMovesText != null && data != null)
            {
                targetMovesText.text = $"★★★ ≤ {data.threeStarMoves}   ★★ ≤ {data.twoStarMoves}";
            }

            HandleMoveCountChanged(0);
            if (SaveManager.Instance != null)
            {
                UpdateCoinsUI(SaveManager.Instance.Coins);
            }
            if (deadlockBanner != null) deadlockBanner.SetActive(false);
        }

        private void HandleMoveCountChanged(int moves)
        {
            if (moveCountText != null)
            {
                moveCountText.text = $"Moves: {moves}";
            }
        }

        private void HandleCanUndoChanged(bool canUndo)
        {
            UpdateUndoButtonState(canUndo);
        }

        private void UpdateUndoButtonState(bool canUndo)
        {
            if (undoButton != null)
            {
                undoButton.interactable = canUndo;
            }
        }

        private void HandleDeadlockStatusChanged(bool isDeadlocked)
        {
            if (deadlockBanner != null)
            {
                deadlockBanner.SetActive(isDeadlocked);
                if (isDeadlocked && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(SoundType.DeadlockWarning);
                }
            }
        }

        private void SendDpad(Vector2Int dir)
        {
            if (InputController.Instance != null)
            {
                InputController.Instance.SendMove(dir);
            }
        }

        private void OnUndoClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.Undo);
            if (InputController.Instance != null)
            {
                InputController.Instance.SendUndo();
            }
        }

        private void OnRestartClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            if (InputController.Instance != null)
            {
                InputController.Instance.SendRestart();
            }
        }

        private void OnMenuClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SetState(GameState.LevelSelect);
            }
        }
    }
}
