using UnityEngine;
using UnityEngine.UI;
using PushTheBox.Core;
using PushTheBox.Level;
using PushTheBox.Audio;
using PushTheBox.Save;
using PushTheBox.GrowthIntegration;

namespace PushTheBox.UI
{
    /// <summary>
    /// Controls the title / main menu screen.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Menu Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private Button soundToggleButton;
        [SerializeField] private Text soundToggleText;

        [Header("Coins Display")]
        [SerializeField] private Text totalCoinsText;

        [Header("Monetization")]
        [SerializeField] private Button shopButton;
        [SerializeField] private Button privacyButton;

        private void Awake()
        {
            EnsureCoinUI();
            EnsureShopUI();
        }

        private void EnsureCoinUI()
        {
            if (totalCoinsText != null) return;
            Font f = soundToggleText != null ? soundToggleText.font : null;
            totalCoinsText = CoinUIHelper.CreateBadge(transform, f, new Vector2(0.68f, 0.91f), new Vector2(0.94f, 0.97f), "MainMenuCoinBadge");
            if (totalCoinsText != null)
            {
                Button badgeBtn = totalCoinsText.transform.parent.gameObject.GetComponent<Button>();
                if (badgeBtn == null) badgeBtn = totalCoinsText.transform.parent.gameObject.AddComponent<Button>();
                badgeBtn.onClick.AddListener(OnShopClicked);
            }
        }

        private void EnsureShopUI()
        {
            Font f = soundToggleText != null ? soundToggleText.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // 1. Shop Button at top left
            if (shopButton == null)
            {
                Transform existing = transform.Find("MainMenuShopButton");
                GameObject shopBtnObj = existing != null ? existing.gameObject : new GameObject("MainMenuShopButton", typeof(RectTransform));
                shopBtnObj.transform.SetParent(transform, false);

                RectTransform rt = shopBtnObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.06f, 0.91f);
                rt.anchorMax = new Vector2(0.32f, 0.97f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                Image img = shopBtnObj.GetComponent<Image>();
                if (img == null) img = shopBtnObj.AddComponent<Image>();
                img.color = new Color(0.92f, 0.58f, 0.12f, 1f);

                shopButton = shopBtnObj.GetComponent<Button>();
                if (shopButton == null) shopButton = shopBtnObj.AddComponent<Button>();

                Transform txtTransform = shopBtnObj.transform.Find("Text");
                GameObject txtObj = txtTransform != null ? txtTransform.gameObject : new GameObject("Text", typeof(RectTransform));
                txtObj.transform.SetParent(shopBtnObj.transform, false);
                RectTransform txtRt = txtObj.GetComponent<RectTransform>();
                txtRt.anchorMin = Vector2.zero;
                txtRt.anchorMax = Vector2.one;
                txtRt.offsetMin = Vector2.zero;
                txtRt.offsetMax = Vector2.zero;

                Text txt = txtObj.GetComponent<Text>();
                if (txt == null) txt = txtObj.AddComponent<Text>();
                txt.text = "🛒 CỬA HÀNG";
                txt.font = f;
                txt.fontSize = 24;
                txt.fontStyle = FontStyle.Bold;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = Color.white;
            }

            // 2. Privacy Button at bottom left
            if (privacyButton == null)
            {
                Transform existing = transform.Find("MainMenuPrivacyButton");
                GameObject privBtnObj = existing != null ? existing.gameObject : new GameObject("MainMenuPrivacyButton", typeof(RectTransform));
                privBtnObj.transform.SetParent(transform, false);

                RectTransform rt = privBtnObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.06f, 0.03f);
                rt.anchorMax = new Vector2(0.32f, 0.08f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                Image img = privBtnObj.GetComponent<Image>();
                if (img == null) img = privBtnObj.AddComponent<Image>();
                img.color = new Color(0.2f, 0.25f, 0.35f, 0.75f);

                privacyButton = privBtnObj.GetComponent<Button>();
                if (privacyButton == null) privacyButton = privBtnObj.AddComponent<Button>();

                Transform txtTransform = privBtnObj.transform.Find("Text");
                GameObject txtObj = txtTransform != null ? txtTransform.gameObject : new GameObject("Text", typeof(RectTransform));
                txtObj.transform.SetParent(privBtnObj.transform, false);
                RectTransform txtRt = txtObj.GetComponent<RectTransform>();
                txtRt.anchorMin = Vector2.zero;
                txtRt.anchorMax = Vector2.one;
                txtRt.offsetMin = Vector2.zero;
                txtRt.offsetMax = Vector2.zero;

                Text txt = txtObj.GetComponent<Text>();
                if (txt == null) txt = txtObj.AddComponent<Text>();
                txt.text = "Quyền riêng tư";
                txt.font = f;
                txt.fontSize = 18;
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = new Color(0.8f, 0.85f, 0.95f, 0.9f);
            }
        }

        private void OnEnable()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (levelSelectButton != null) levelSelectButton.onClick.AddListener(OnLevelSelectClicked);
            if (soundToggleButton != null) soundToggleButton.onClick.AddListener(OnSoundToggleClicked);
            if (shopButton != null) shopButton.onClick.AddListener(OnShopClicked);
            if (privacyButton != null) privacyButton.onClick.AddListener(OnPrivacyClicked);

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnCoinsChanged += UpdateCoinsUI;
                UpdateCoinsUI(SaveManager.Instance.Coins);
            }

            UpdateSoundUI();
        }

        private void OnDisable()
        {
            if (playButton != null) playButton.onClick.RemoveListener(OnPlayClicked);
            if (levelSelectButton != null) levelSelectButton.onClick.RemoveListener(OnLevelSelectClicked);
            if (soundToggleButton != null) soundToggleButton.onClick.RemoveListener(OnSoundToggleClicked);
            if (shopButton != null) shopButton.onClick.RemoveListener(OnShopClicked);
            if (privacyButton != null) privacyButton.onClick.RemoveListener(OnPrivacyClicked);

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnCoinsChanged -= UpdateCoinsUI;
            }
        }

        private void UpdateCoinsUI(int coins)
        {
            if (totalCoinsText != null)
            {
                totalCoinsText.text = coins.ToString();
            }
        }

        private void OnPlayClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);

            int targetLevel = 1;
            if (SaveManager.Instance != null)
            {
                targetLevel = SaveManager.Instance.GetHighestUnlockedLevel();
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.LoadLevel(targetLevel);
            }
        }

        private void OnLevelSelectClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SetState(GameState.LevelSelect);
            }
        }

        private void OnSoundToggleClicked()
        {
            if (AudioManager.Instance != null)
            {
                bool newMuted = !AudioManager.Instance.IsMuted;
                AudioManager.Instance.SetMuted(newMuted);
                AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            }
            UpdateSoundUI();
        }

        private void UpdateSoundUI()
        {
            if (soundToggleText != null && AudioManager.Instance != null)
            {
                soundToggleText.text = AudioManager.Instance.IsMuted ? "Sound: OFF" : "Sound: ON";
            }
        }

        private void OnShopClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            if (GrowthManager.Instance != null)
            {
                _ = GrowthManager.Instance.OpenShopAsync();
            }
        }

        private void OnPrivacyClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            if (GrowthManager.Instance != null)
            {
                GrowthManager.Instance.ShowPrivacyOptions();
            }
        }
    }
}
