using System;
using UnityEngine;

namespace PushTheBox.Core
{
    /// <summary>
    /// Centralized game state manager. Controls state transitions and broadcasts events
    /// to decouple UI, gameplay, and audio systems.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        /// <summary>
        /// Invoked when game state changes. Parameters: (oldState, newState)
        /// </summary>
        public event Action<GameState, GameState> OnGameStateChanged;

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

        public void SetState(GameState newState)
        {
            if (CurrentState == newState)
                return;

            GameState oldState = CurrentState;
            CurrentState = newState;

            #if UNITY_EDITOR
            Debug.Log($"[GameStateManager] State transition: {oldState} -> {newState}");
            #endif

            OnGameStateChanged?.Invoke(oldState, newState);
        }

        public bool IsPlaying() => CurrentState == GameState.Playing;
        public bool IsPaused() => CurrentState == GameState.Paused;
        public bool IsLevelCompleted() => CurrentState == GameState.LevelCompleted;
    }
}
