using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Growth;
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
        [SerializeField] private Button removeAdsButton;
        [SerializeField] private Text removeAdsButtonText;
        [SerializeField] private Button restorePurchasesButton;
        [SerializeField] private Text restorePurchasesButtonText;

        // The store loads asynchronously after launch, so the first Offers.Get may come back empty.
        private const int RemoveAdsOfferRetries = 5;
        private const float RemoveAdsOfferRetryDelay = 2f;

        private OfferPopupUI offerPopup;
        private bool restoreInFlight;

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

            // 3. Remove Ads offer button below the main buttons (hidden until the SDK has a sellable offer)
            if (removeAdsButton == null)
            {
                removeAdsButton = CreateMenuButton("MainMenuRemoveAdsButton", new Vector2(0.16f, 0.09f), new Vector2(0.84f, 0.17f),
                    new Color(0.85f, 0.25f, 0.32f, 1f), 34, f, out removeAdsButtonText);
                removeAdsButtonText.text = "XOÁ QUẢNG CÁO";
                removeAdsButtonText.fontStyle = FontStyle.Bold;
                removeAdsButton.gameObject.SetActive(false);
            }

            // 4. Restore Purchases button at bottom right (mirrors the privacy button)
            if (restorePurchasesButton == null)
            {
                restorePurchasesButton = CreateMenuButton("MainMenuRestoreButton", new Vector2(0.68f, 0.03f), new Vector2(0.94f, 0.08f),
                    new Color(0.2f, 0.25f, 0.35f, 0.75f), 18, f, out restorePurchasesButtonText);
                restorePurchasesButtonText.text = "Khôi phục mua hàng";
                restorePurchasesButtonText.color = new Color(0.8f, 0.85f, 0.95f, 0.9f);
            }
        }

        private Button CreateMenuButton(string objName, Vector2 anchorMin, Vector2 anchorMax, Color color, int fontSize, Font font, out Text label)
        {
            Transform existing = transform.Find(objName);
            GameObject btnObj = existing != null ? existing.gameObject : new GameObject(objName, typeof(RectTransform));
            btnObj.transform.SetParent(transform, false);

            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image img = btnObj.GetComponent<Image>();
            if (img == null) img = btnObj.AddComponent<Image>();
            img.color = color;

            Button btn = btnObj.GetComponent<Button>();
            if (btn == null) btn = btnObj.AddComponent<Button>();

            Transform txtTransform = btnObj.transform.Find("Text");
            GameObject txtObj = txtTransform != null ? txtTransform.gameObject : new GameObject("Text", typeof(RectTransform));
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;

            label = txtObj.GetComponent<Text>();
            if (label == null) label = txtObj.AddComponent<Text>();
            label.font = font;
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = Mathf.Max(12, fontSize / 2);
            label.resizeTextMaxSize = fontSize;
            return btn;
        }

        private void OnEnable()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (levelSelectButton != null) levelSelectButton.onClick.AddListener(OnLevelSelectClicked);
            if (soundToggleButton != null) soundToggleButton.onClick.AddListener(OnSoundToggleClicked);
            if (shopButton != null) shopButton.onClick.AddListener(OnShopClicked);
            if (privacyButton != null) privacyButton.onClick.AddListener(OnPrivacyClicked);
            if (removeAdsButton != null) removeAdsButton.onClick.AddListener(OnRemoveAdsClicked);
            if (restorePurchasesButton != null) restorePurchasesButton.onClick.AddListener(OnRestorePurchasesClicked);

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnCoinsChanged += UpdateCoinsUI;
                UpdateCoinsUI(SaveManager.Instance.Coins);
            }

            Growth.Purchase.OwnershipChanged += HandleOwnershipChanged;

            UpdateSoundUI();
            StartCoroutine(RefreshRemoveAdsOfferRoutine());
        }

        private void OnDisable()
        {
            if (playButton != null) playButton.onClick.RemoveListener(OnPlayClicked);
            if (levelSelectButton != null) levelSelectButton.onClick.RemoveListener(OnLevelSelectClicked);
            if (soundToggleButton != null) soundToggleButton.onClick.RemoveListener(OnSoundToggleClicked);
            if (shopButton != null) shopButton.onClick.RemoveListener(OnShopClicked);
            if (privacyButton != null) privacyButton.onClick.RemoveListener(OnPrivacyClicked);
            if (removeAdsButton != null) removeAdsButton.onClick.RemoveListener(OnRemoveAdsClicked);
            if (restorePurchasesButton != null) restorePurchasesButton.onClick.RemoveListener(OnRestorePurchasesClicked);

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnCoinsChanged -= UpdateCoinsUI;
            }

            Growth.Purchase.OwnershipChanged -= HandleOwnershipChanged;

            if (offerPopup != null) offerPopup.Dismiss();
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

        private void HandleOwnershipChanged(string productId)
        {
            // Remove Ads bought in the Shop, restored, or a Pending payment completed: redraw the offer button.
            if (isActiveAndEnabled) StartCoroutine(RefreshRemoveAdsOfferRoutine());
        }

        /// <summary>
        /// Shows the Remove Ads button only while the SDK has a sellable offer for home_remove_ads
        /// (it returns null once no_ads is owned, the store has no product, or the placement is disabled remotely).
        /// </summary>
        private IEnumerator RefreshRemoveAdsOfferRoutine()
        {
            if (removeAdsButton == null) yield break;

            for (int attempt = 0; attempt < RemoveAdsOfferRetries; attempt++)
            {
                OfferView offer = GrowthManager.Instance != null ? GrowthManager.Instance.GetOffer(OfferPlacementIds.HomeRemoveAds) : null;
                bool visible = offer != null;
                removeAdsButton.gameObject.SetActive(visible);
                if (visible)
                {
                    if (removeAdsButtonText != null)
                    {
                        removeAdsButtonText.text = string.IsNullOrEmpty(offer.LocalizedPrice)
                            ? "XOÁ QUẢNG CÁO"
                            : $"XOÁ QUẢNG CÁO  ·  {offer.LocalizedPrice}";
                    }
                    yield break;
                }

                if (GrowthManager.Instance == null || !GrowthManager.Instance.IsInitialized) yield break;
                yield return new WaitForSecondsRealtime(RemoveAdsOfferRetryDelay);
            }
        }

        private void OnRemoveAdsClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            if (offerPopup != null || GrowthManager.Instance == null) return;

            Font f = removeAdsButtonText != null ? removeAdsButtonText.font : null;

            // Fetch the offer at the moment it is drawn (SDK rule L6); the popup reports it as shown.
            OfferView offer = GrowthManager.Instance.GetOffer(OfferPlacementIds.HomeRemoveAds);
            if (offer == null)
            {
                removeAdsButton.gameObject.SetActive(false);
                ToastUI.Show(transform, "Gói này hiện không khả dụng.", f);
                return;
            }

            offerPopup = OfferPopupUI.Show(transform, offer, f, outcome =>
            {
                offerPopup = null;
                if (outcome == OfferPurchaseOutcome.Purchased)
                {
                    ToastUI.Show(transform, "Đã xoá quảng cáo. Cảm ơn bạn!", f);
                }
                if (isActiveAndEnabled) StartCoroutine(RefreshRemoveAdsOfferRoutine());
            });
        }

        private void OnRestorePurchasesClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            if (restoreInFlight || GrowthManager.Instance == null) return;

            restoreInFlight = true;
            restorePurchasesButton.interactable = false;
            Font f = restorePurchasesButtonText != null ? restorePurchasesButtonText.font : null;

            GrowthManager.Instance.RestorePurchases(result =>
            {
                restoreInFlight = false;
                if (this == null) return;
                restorePurchasesButton.interactable = true;

                string message;
                if (result.Failed)
                {
                    message = result.Reason == RestoreResult.StoreUnavailable
                        ? "Không kết nối được cửa hàng, vui lòng thử lại."
                        : "Khôi phục không thành công, vui lòng thử lại.";
                }
                else
                {
                    message = result.RestoredCount > 0
                        ? $"Đã khôi phục {result.RestoredCount} giao dịch."
                        : "Không có giao dịch nào để khôi phục.";
                }
                ToastUI.Show(transform, message, f);

                if (isActiveAndEnabled) StartCoroutine(RefreshRemoveAdsOfferRoutine());
            });
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
