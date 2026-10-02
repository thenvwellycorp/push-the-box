using System;
using System.Collections.Generic;
using UnityEngine;

namespace PushTheBox.Core
{
    /// <summary>
    /// Manages undo history using a snapshot stack (Memento pattern).
    /// </summary>
    public class UndoManager : MonoBehaviour
    {
        public static UndoManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private int maxHistoryCount = 100;

        private readonly Stack<GameSnapshot> history = new Stack<GameSnapshot>();

        public bool CanUndo => history.Count > 0;
        public int HistoryCount => history.Count;

        public event Action<bool> OnCanUndoChanged;

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

        public void Record(GameSnapshot snapshot)
        {
            if (snapshot == null) return;

            // Maintain max capacity by discarding oldest if needed
            if (history.Count >= maxHistoryCount)
            {
                // Convert to array or list to trim oldest
                var list = new List<GameSnapshot>(history);
                list.RemoveAt(list.Count - 1); // remove oldest at bottom
                history.Clear();
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    history.Push(list[i]);
                }
            }

            bool wasCanUndo = CanUndo;
            history.Push(snapshot);

            if (!wasCanUndo)
            {
                OnCanUndoChanged?.Invoke(true);
            }
        }

        public GameSnapshot Pop()
        {
            if (history.Count == 0) return null;

            GameSnapshot snapshot = history.Pop();
            OnCanUndoChanged?.Invoke(CanUndo);
            return snapshot;
        }

        public void Clear()
        {
            bool wasCanUndo = CanUndo;
            history.Clear();

            if (wasCanUndo)
            {
                OnCanUndoChanged?.Invoke(false);
            }
        }
    }
}
