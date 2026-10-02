using System;
using System.Collections.Generic;
using UnityEngine;

namespace PushTheBox.Save
{
    [Serializable]
    public class LevelSaveEntry
    {
        public int levelId;
        public int stars;
        public int bestMoves;
        public bool isCompleted;
    }

    [Serializable]
    public class SaveData
    {
        public int highestUnlockedLevel = 1;
        public bool soundEnabled = true;
        public int totalCoins = 0;
        public List<LevelSaveEntry> levels = new List<LevelSaveEntry>();
    }

    /// <summary>
    /// Handles persistent progress (unlocked levels, stars, best moves, coins, settings)
    /// using JSON serialization and PlayerPrefs. Isolated from gameplay logic.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string SAVE_KEY = "PushTheBox_SaveData_v1";

        private SaveData data;

        public bool SoundEnabled => data.soundEnabled;
        public int Coins => data != null ? data.totalCoins : 0;

        public event Action OnSaveDataChanged;
        public event Action<int> OnCoinsChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                Load();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        public void Load()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                try
                {
                    string json = PlayerPrefs.GetString(SAVE_KEY);
                    data = JsonUtility.FromJson<SaveData>(json);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[SaveManager] Failed to parse save data: {e.Message}. Resetting to default.");
                    data = new SaveData();
                }
            }
            else
            {
                data = new SaveData();
            }

            if (data.highestUnlockedLevel < 1)
                data.highestUnlockedLevel = 1;

            if (data.totalCoins < 0)
                data.totalCoins = 0;
        }

        public void Save()
        {
            try
            {
                string json = JsonUtility.ToJson(data, false);
                PlayerPrefs.SetString(SAVE_KEY, json);
                PlayerPrefs.Save();
                OnSaveDataChanged?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to write save data: {e.Message}");
            }
        }

        public int GetHighestUnlockedLevel()
        {
            return data != null ? data.highestUnlockedLevel : 1;
        }

        public bool IsLevelUnlocked(int levelId)
        {
            if (levelId <= 1) return true;
            return levelId <= data.highestUnlockedLevel;
        }

        public LevelSaveEntry GetLevelProgress(int levelId)
        {
            return data.levels.Find(l => l.levelId == levelId);
        }

        public int GetCoins()
        {
            return data != null ? data.totalCoins : 0;
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0 || data == null) return;
            data.totalCoins += amount;
            Save();
            OnCoinsChanged?.Invoke(data.totalCoins);
        }

        public bool SpendCoins(int amount)
        {
            if (amount <= 0 || data == null || data.totalCoins < amount) return false;
            data.totalCoins -= amount;
            Save();
            OnCoinsChanged?.Invoke(data.totalCoins);
            return true;
        }

        public void SaveLevelResult(int levelId, int stars, int moves, int coinsEarned = 0)
        {
            LevelSaveEntry entry = data.levels.Find(l => l.levelId == levelId);
            if (entry == null)
            {
                entry = new LevelSaveEntry
                {
                    levelId = levelId,
                    stars = stars,
                    bestMoves = moves,
                    isCompleted = true
                };
                data.levels.Add(entry);
            }
            else
            {
                entry.isCompleted = true;
                if (stars > entry.stars)
                    entry.stars = stars;
                if (entry.bestMoves == 0 || moves < entry.bestMoves)
                    entry.bestMoves = moves;
            }

            // Unlock next level
            int nextLevel = levelId + 1;
            if (nextLevel > data.highestUnlockedLevel)
            {
                data.highestUnlockedLevel = nextLevel;
            }

            if (coinsEarned > 0)
            {
                data.totalCoins += coinsEarned;
                OnCoinsChanged?.Invoke(data.totalCoins);
            }

            Save();
        }

        public void SetSoundEnabled(bool enabled)
        {
            data.soundEnabled = enabled;
            Save();
        }

        public void ResetAllProgress()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            data = new SaveData();
            Save();
            OnCoinsChanged?.Invoke(0);
        }
    }
}
