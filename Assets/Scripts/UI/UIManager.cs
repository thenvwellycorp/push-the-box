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
            if (gameView != null) gameView.SetActive(newState == GameState.Playing || newState == GameState.LevelCompleted || newState == GameState.Paused);

            // Modal is handled directly by LevelCompleteUI, but hide if leaving completed state
            if (newState != GameState.LevelCompleted && levelCompleteModal != null)
            {
                levelCompleteModal.SetActive(false);
            }
        }
    }
}
