using UnityEngine;
using PushTheBox.Core;

namespace PushTheBox.UI
{
    /// <summary>
    /// Coordinates high-level UI screen transitions based on GameState changes.
    /// Follows the Observer pattern.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Views")]
        [SerializeField] private GameObject mainMenuView;
        [SerializeField] private GameObject levelSelectView;
        [SerializeField] private GameObject gameView;
        [SerializeField] private GameObject levelCompleteModal;
        [SerializeField] private GameObject gameOverModal;

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

            if (levelCompleteModal == null)
            {
                var completeUI = Object.FindFirstObjectByType<LevelCompleteUI>(FindObjectsInactive.Include);
                if (completeUI != null)
                {
                    levelCompleteModal = completeUI.gameObject;
                }
            }

            if (gameOverModal == null)
            {
                var goUI = Object.FindFirstObjectByType<GameOverUI>(FindObjectsInactive.Include);
                if (goUI != null)
                {
                    gameOverModal = goUI.gameObject;
                }
                else
                {
                    GameObject goObj = new GameObject("GameOver_Modal", typeof(RectTransform));
                    goObj.transform.SetParent(transform, false);
                    var ui = goObj.AddComponent<GameOverUI>();
                    ui.EnsureUI();
                    gameOverModal = goObj;
                    gameOverModal.SetActive(false);
                }
            }
        }

        private void Start()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnGameStateChanged += HandleGameStateChanged;
                // Initialize to current state
                HandleGameStateChanged(GameState.MainMenu, GameStateManager.Instance.CurrentState);
            }
        }

        private void OnDestroy()
        {
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void HandleGameStateChanged(GameState oldState, GameState newState)
        {
            if (mainMenuView != null) mainMenuView.SetActive(newState == GameState.MainMenu);
            if (levelSelectView != null) levelSelectView.SetActive(newState == GameState.LevelSelect);
            if (gameView != null) gameView.SetActive(newState == GameState.Playing || newState == GameState.LevelCompleted || newState == GameState.Paused || newState == GameState.GameOver);

            if (levelCompleteModal != null)
            {
                levelCompleteModal.SetActive(newState == GameState.LevelCompleted);
            }

            if (gameOverModal != null)
            {
                gameOverModal.SetActive(newState == GameState.GameOver);
            }
        }
    }
}
