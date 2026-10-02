using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PushTheBox.Core;
using PushTheBox.Level;
using PushTheBox.Save;
using PushTheBox.Audio;

namespace PushTheBox.UI
{
    /// <summary>
    /// Displays grid of levels with their stars and unlocked status.
    /// </summary>
    public class LevelSelectUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform buttonsContainer;
        [SerializeField] private LevelButtonUI levelButtonPrefab;
        [SerializeField] private Button backButton;
        [SerializeField] private Text coinsText;

        private readonly List<LevelButtonUI> spawnedButtons = new List<LevelButtonUI>();

        private void Awake()
        {
            EnsureCoinUI();
        }

        private void EnsureCoinUI()
        {
            if (coinsText != null) return;
            Text anyText = GetComponentInChildren<Text>();
            Font f = anyText != null ? anyText.font : null;
            coinsText = CoinUIHelper.CreateBadge(transform, f, new Vector2(0.68f, 0.86f), new Vector2(0.94f, 0.92f), "LevelSelectCoinBadge");
            if (coinsText != null)
            {
                Button badgeBtn = coinsText.transform.parent.gameObject.GetComponent<Button>();
                if (badgeBtn == null) badgeBtn = coinsText.transform.parent.gameObject.AddComponent<Button>();
                badgeBtn.onClick.AddListener(() =>
                {
                    if (PushTheBox.GrowthIntegration.GrowthManager.Instance != null)
                    {
                        _ = PushTheBox.GrowthIntegration.GrowthManager.Instance.OpenShopAsync();
                    }
                });
            }
        }

        private void OnEnable()
        {
            if (backButton != null) backButton.onClick.AddListener(OnBackClicked);

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnCoinsChanged += UpdateCoinsUI;
                UpdateCoinsUI(SaveManager.Instance.Coins);
            }

            PopulateLevels();
        }

        private void OnDisable()
        {
            if (backButton != null) backButton.onClick.RemoveListener(OnBackClicked);

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnCoinsChanged -= UpdateCoinsUI;
            }
        }

        private void UpdateCoinsUI(int coins)
        {
            if (coinsText != null)
            {
                coinsText.text = coins.ToString();
            }
        }

        public void PopulateLevels()
        {
            if (LevelManager.Instance == null || buttonsContainer == null) return;

            // Clear previous buttons
            for (int i = 0; i < spawnedButtons.Count; i++)
            {
                if (spawnedButtons[i] != null)
                {
                    Destroy(spawnedButtons[i].gameObject);
                }
            }
            spawnedButtons.Clear();

            int totalLevels = LevelManager.Instance.TotalLevels;
            for (int i = 0; i < totalLevels; i++)
            {
                int levelId = i + 1;
                bool isUnlocked = SaveManager.Instance == null || SaveManager.Instance.IsLevelUnlocked(levelId);
                var progress = SaveManager.Instance != null ? SaveManager.Instance.GetLevelProgress(levelId) : null;
                int stars = progress != null ? progress.stars : 0;

                LevelButtonUI btnObj;
                if (levelButtonPrefab != null)
                {
                    btnObj = Instantiate(levelButtonPrefab, buttonsContainer);
                }
                else
                {
                    // Procedural button fallback
                    GameObject go = CreateProceduralLevelButton(levelId);
                    go.transform.SetParent(buttonsContainer, false);
                    btnObj = go.GetComponent<LevelButtonUI>();
                }

                btnObj.Setup(levelId, isUnlocked, stars, HandleLevelSelected);
                spawnedButtons.Add(btnObj);
            }
        }

        private void HandleLevelSelected(int levelId)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadLevel(levelId);
            }
        }

        private void OnBackClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SetState(GameState.MainMenu);
            }
        }

        private GameObject CreateProceduralLevelButton(int levelId)
        {
            GameObject go = new GameObject($"LevelButton_{levelId}");
            Image img = go.AddComponent<Image>();
            img.color = new Color(0.24f, 0.28f, 0.36f, 1f);
            Button btn = go.AddComponent<Button>();

            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(0.35f, 0.45f, 0.6f, 1f);
            cb.pressedColor = new Color(0.18f, 0.22f, 0.3f, 1f);
            cb.disabledColor = new Color(0.15f, 0.17f, 0.2f, 0.5f);
            btn.colors = cb;

            // Number text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(go.transform, false);
            Text txt = textObj.AddComponent<Text>();
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null) txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = 26;

            // Stars text
            GameObject starsObj = new GameObject("Stars");
            starsObj.transform.SetParent(go.transform, false);
            Text starsTxt = starsObj.AddComponent<Text>();
            starsTxt.alignment = TextAnchor.LowerCenter;
            starsTxt.color = new Color(1f, 0.85f, 0.2f, 1f);
            starsTxt.font = txt.font;
            starsTxt.fontSize = 18;

            LevelButtonUI buttonUI = go.AddComponent<LevelButtonUI>();
            return go;
        }
    }
}
