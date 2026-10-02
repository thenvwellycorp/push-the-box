using UnityEngine;
using PushTheBox.Level;
using PushTheBox.Save;

namespace PushTheBox.Core
{
    /// <summary>
    /// Game orchestrator and entry point.
    /// Bootstraps systems and manages high-level lifecycle without violating SRP.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Startup Configuration")]
        [SerializeField] private bool autoStartFirstLevelInEditor = false;
        [SerializeField] private int targetFrameRate = 60;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // Optional persistence across scenes if multi-scene setup is used
                DontDestroyOnLoad(gameObject);
                Application.targetFrameRate = targetFrameRate;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            if (autoStartFirstLevelInEditor && Application.isEditor)
            {
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.LoadLevel(1);
                }
            }
            else
            {
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.SetState(GameState.MainMenu);
                }
            }
        }

        public void QuitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
