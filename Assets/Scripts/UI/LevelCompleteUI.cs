using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PushTheBox.Core;
using PushTheBox.Level;
using PushTheBox.Audio;
using PushTheBox.Save;
using Growth;
using PushTheBox.GrowthIntegration;

namespace PushTheBox.UI
{
    /// <summary>
    /// Modal dialog displayed upon level completion.
    /// Shows moves, star rating animation, and replay/next buttons.
    /// </summary>
    public class LevelCompleteUI : MonoBehaviour
    {
        [Header("Modal Containers")]
        [SerializeField] private GameObject modalRoot;
        [SerializeField] private Transform dialogContent;

        [Header("Information Labels")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text movesText;

        [Header("Star Icons")]
        [SerializeField] private Image[] starImages;
        [SerializeField] private Color starActiveColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private Color starInactiveColor = new Color(0.35f, 0.35f, 0.4f, 0.5f);

        [Header("Coin Reward Display")]
        [SerializeField] private GameObject coinContainer;
        [SerializeField] private Text coinsEarnedText;
        [SerializeField] private Text totalCoinsText;
        [SerializeField] private Transform coinAnimTransform;

        [Header("Double Coins Action")]
        [SerializeField] private Button doubleCoinsButton;
        [SerializeField] private Text doubleCoinsButtonText;

        [Header("Action Buttons")]
        [SerializeField] private Button replayButton;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button levelSelectButton;

        private int currentCoinsEarned = 0;
        private bool isDoubleClaimed = false;
        private bool isShowing = false;
        private LevelCompletionData? currentShownData = null;

        // In-game offer (win_screen) drawn as an extra section under the card instead of a separate popup.
        private const float WinOfferCardLift = 0.08f;
        private RectTransform winOfferSection;
        private Text winOfferTitleText;
        private Text winOfferGrantText;
        private Text winOfferStatusText;
        private Button winOfferBuyButton;
        private Text winOfferBuyText;
        private OfferView winOffer;
        private long winOfferCoins;
        private bool winOfferPurchaseInFlight;
        private bool cardLifted;

        private void Awake()
        {
            EnsureCoinUI();
        }

        private void EnsureCoinUI()
        {
            Transform parent = dialogContent != null ? dialogContent : (modalRoot != null ? modalRoot.transform : transform);
            Font f = titleText != null ? titleText.font : null;

            // Align Title Text
            if (titleText != null)
            {
                RectTransform rt = titleText.rectTransform;
                rt.anchorMin = new Vector2(0.05f, 0.83f);
                rt.anchorMax = new Vector2(0.95f, 0.96f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            // Align Stars Row if parented under dialogContent
            if (starImages != null && starImages.Length > 0 && starImages[0] != null)
            {
                RectTransform starsRowRt = starImages[0].transform.parent as RectTransform;
                if (starsRowRt != null && starsRowRt != parent)
                {
                    starsRowRt.anchorMin = new Vector2(0.15f, 0.66f);
                    starsRowRt.anchorMax = new Vector2(0.85f, 0.81f);
                    starsRowRt.offsetMin = Vector2.zero;
                    starsRowRt.offsetMax = Vector2.zero;
                }
            }

            // Align Moves Text
            if (movesText != null)
            {
                RectTransform rt = movesText.rectTransform;
                rt.anchorMin = new Vector2(0.10f, 0.43f);
                rt.anchorMax = new Vector2(0.90f, 0.50f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }

            // Align or Create Coin Reward Row
            if (coinsEarnedText == null)
            {
                var res = CoinUIHelper.CreateRewardRow(parent, f);
                coinsEarnedText = res.earnedTxt;
                totalCoinsText = res.totalTxt;
                coinContainer = res.container;
                coinAnimTransform = res.container != null ? res.container.transform : null;
            }

            if (coinContainer != null)
            {
                RectTransform coinRt = coinContainer.GetComponent<RectTransform>();
                if (coinRt != null)
                {
                    coinRt.anchorMin = new Vector2(0.10f, 0.52f);
                    coinRt.anchorMax = new Vector2(0.90f, 0.63f);
                    coinRt.offsetMin = Vector2.zero;
                    coinRt.offsetMax = Vector2.zero;
                }
            }

            // Align or Create Double Coins Button
            if (doubleCoinsButton == null)
            {
                var btnRes = CoinUIHelper.CreateDoubleCoinsButton(parent, f, new Vector2(0.10f, 0.26f), new Vector2(0.90f, 0.39f));
                doubleCoinsButton = btnRes.button;
                doubleCoinsButtonText = btnRes.labelText;
            }
            else
            {
                RectTransform dBtnRt = doubleCoinsButton.GetComponent<RectTransform>();
                if (dBtnRt != null)
                {
                    dBtnRt.anchorMin = new Vector2(0.10f, 0.26f);
                    dBtnRt.anchorMax = new Vector2(0.90f, 0.39f);
                    dBtnRt.offsetMin = Vector2.zero;
                    dBtnRt.offsetMax = Vector2.zero;
                }
            }

            // Align Bottom Buttons Row
            if (replayButton != null)
            {
                RectTransform btnRowRt = replayButton.transform.parent as RectTransform;
                if (btnRowRt != null && btnRowRt != parent)
                {
                    btnRowRt.anchorMin = new Vector2(0.08f, 0.08f);
                    btnRowRt.anchorMax = new Vector2(0.92f, 0.22f);
                    btnRowRt.offsetMin = Vector2.zero;
                    btnRowRt.offsetMax = Vector2.zero;
                }
            }
        }

        private void OnEnable()
        {
            EnsureCoinUI();

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
                LevelManager.Instance.OnLevelCompleted += HandleLevelCompleted;
            }

            if (doubleCoinsButton != null)
            {
                doubleCoinsButton.onClick.RemoveListener(OnDoubleCoinsClicked);
                doubleCoinsButton.onClick.AddListener(OnDoubleCoinsClicked);
            }
            if (replayButton != null)
            {
                replayButton.onClick.RemoveListener(OnReplayClicked);
                replayButton.onClick.AddListener(OnReplayClicked);
            }
            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
                nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            }
            if (levelSelectButton != null)
            {
                levelSelectButton.onClick.RemoveListener(OnLevelSelectClicked);
                levelSelectButton.onClick.AddListener(OnLevelSelectClicked);
            }

            // If LevelManager already triggered completion before or during activation, display it!
            if (LevelManager.Instance != null && LevelManager.Instance.LastCompletionData.HasValue)
            {
                HandleLevelCompleted(LevelManager.Instance.LastCompletionData.Value);
            }
        }

        private void OnDisable()
        {
            isShowing = false;
            currentShownData = null;

            HideWinOffer();

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
            }

            if (doubleCoinsButton != null) doubleCoinsButton.onClick.RemoveListener(OnDoubleCoinsClicked);
            if (replayButton != null) replayButton.onClick.RemoveListener(OnReplayClicked);
            if (nextLevelButton != null) nextLevelButton.onClick.RemoveListener(OnNextLevelClicked);
            if (levelSelectButton != null) levelSelectButton.onClick.RemoveListener(OnLevelSelectClicked);
        }

        private void HandleLevelCompleted(LevelCompletionData data)
        {
            if (isShowing && currentShownData.HasValue && currentShownData.Value.levelId == data.levelId && currentShownData.Value.moves == data.moves)
                return;

            isShowing = true;
            currentShownData = data;

            if (modalRoot != null && !modalRoot.activeSelf)
                modalRoot.SetActive(true);
            else if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            currentCoinsEarned = data.coinsEarned;
            isDoubleClaimed = false;

            if (titleText != null)
                titleText.text = $"{data.levelName} Cleared!";

            if (movesText != null)
                movesText.text = $"Total Moves: {data.moves}";

            if (coinsEarnedText != null)
            {
                coinsEarnedText.text = $"+{data.coinsEarned}";
            }

            if (totalCoinsText != null)
            {
                totalCoinsText.text = $"{data.totalCoins}";
            }

            // Setup Double Coins Button
            if (doubleCoinsButton != null)
            {
                doubleCoinsButton.interactable = true;
                doubleCoinsButton.gameObject.SetActive(true);
            }
            if (doubleCoinsButtonText != null)
            {
                doubleCoinsButtonText.text = $"▶ CLAIM 2X (+{data.coinsEarned})";
            }

            // Setup Next Level Button
            if (nextLevelButton != null)
            {
                nextLevelButton.gameObject.SetActive(true);
                Text nextTxt = nextLevelButton.GetComponentInChildren<Text>();
                if (nextTxt != null)
                {
                    nextTxt.text = data.hasNextLevel ? "NEXT LEVEL ▶" : "FINISH ▶";
                }
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.LevelComplete);
            }

            StopAllCoroutines();
            HideWinOffer();
            StartCoroutine(AnimateCompletionRoutine(data));
        }

        private void OnDoubleCoinsClicked()
        {
            if (isDoubleClaimed || currentCoinsEarned <= 0) return;

            if (GrowthManager.Instance != null)
            {
                GrowthManager.Instance.ShowRewarded(
                    Placement.DoubleCoin,
                    RewardType.Coin,
                    currentCoinsEarned,
                    onRewardGranted: () =>
                    {
                        isDoubleClaimed = true;
                        if (AudioManager.Instance != null)
                        {
                            AudioManager.Instance.PlaySound(SoundType.CoinReward);
                        }
                        // When the SDK is running it already credits RewardType.Coin through the inventory adapter.
                        if (!GrowthManager.Instance.IsInitialized && SaveManager.Instance != null)
                        {
                            SaveManager.Instance.AddCoins(currentCoinsEarned);
                        }
                        StartCoroutine(AnimateDoubleCoinsRoutine());
                    });
            }
            else
            {
                isDoubleClaimed = true;
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(SoundType.CoinReward);
                }
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.AddCoins(currentCoinsEarned);
                }
                StartCoroutine(AnimateDoubleCoinsRoutine());
            }
        }

        private IEnumerator AnimateDoubleCoinsRoutine()
        {
            if (doubleCoinsButton != null)
            {
                doubleCoinsButton.interactable = false;
            }
            if (doubleCoinsButtonText != null)
            {
                doubleCoinsButtonText.text = "CLAIMED 2X! ✓";
            }

            int startCoins = currentCoinsEarned;
            int targetCoins = currentCoinsEarned * 2;
            float dur = 0.5f;
            float elapsed = 0f;

            Transform targetAnim = coinAnimTransform != null ? coinAnimTransform : (coinsEarnedText != null ? coinsEarnedText.transform : null);
            Vector3 origScale = targetAnim != null ? targetAnim.localScale : Vector3.one;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                if (coinsEarnedText != null)
                {
                    int curr = Mathf.RoundToInt(Mathf.Lerp(startCoins, targetCoins, t));
                    coinsEarnedText.text = $"+{curr} (2X!)";
                }
                if (targetAnim != null)
                {
                    float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.35f;
                    targetAnim.localScale = origScale * scale;
                }
                yield return null;
            }

            if (coinsEarnedText != null)
            {
                coinsEarnedText.text = $"+{targetCoins} (2X!)";
            }
            if (targetAnim != null)
            {
                targetAnim.localScale = origScale;
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.CoinReward);
            }

            yield return new WaitForSeconds(0.7f);

            OnNextLevelClicked();
        }

        private IEnumerator AnimateCompletionRoutine(LevelCompletionData data)
        {
            // One frame later so GrowthManager has reported Level.Complete (win count used by every_n_wins).
            yield return null;
            TryShowWinOffer();

            // Reset all stars to inactive
            if (starImages != null)
            {
                for (int i = 0; i < starImages.Length; i++)
                {
                    if (starImages[i] != null)
                    {
                        starImages[i].color = starInactiveColor;
                        starImages[i].transform.localScale = Vector3.one * 0.8f;
                    }
                }

                yield return new WaitForSeconds(0.2f);

                // Reveal stars sequentially with pop bounce
                for (int i = 0; i < starImages.Length && i < data.stars; i++)
                {
                    if (starImages[i] != null)
                    {
                        starImages[i].color = starActiveColor;

                        // Punch bounce
                        float dur = 0.2f;
                        float elapsed = 0f;
                        while (elapsed < dur)
                        {
                            elapsed += Time.deltaTime;
                            float t = elapsed / dur;
                            float scale = Mathf.Lerp(1.5f, 1f, t);
                            starImages[i].transform.localScale = Vector3.one * scale;
                            yield return null;
                        }
                        starImages[i].transform.localScale = Vector3.one;

                        if (AudioManager.Instance != null)
                        {
                            AudioManager.Instance.PlaySound(SoundType.BoxOnTarget);
                        }

                        yield return new WaitForSeconds(0.12f);
                    }
                }
            }

            // Animate Coin Reward
            if (data.coinsEarned > 0)
            {
                yield return new WaitForSeconds(0.1f);

                Transform targetAnim = coinAnimTransform != null ? coinAnimTransform : (coinsEarnedText != null ? coinsEarnedText.transform : null);

                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(SoundType.CoinReward);
                }

                if (targetAnim != null)
                {
                    float dur = 0.35f;
                    float elapsed = 0f;
                    Vector3 initialScale = targetAnim.localScale;

                    while (elapsed < dur)
                    {
                        elapsed += Time.deltaTime;
                        float t = elapsed / dur;
                        // Bounce curve: pop up to 1.4x then settle back
                        float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.4f;
                        targetAnim.localScale = initialScale * scale;

                        // Counting up effect for text
                        if (coinsEarnedText != null)
                        {
                            int displayCoins = Mathf.RoundToInt(Mathf.Lerp(0, data.coinsEarned, t));
                            coinsEarnedText.text = $"+{displayCoins}";
                        }

                        yield return null;
                    }
                    targetAnim.localScale = initialScale;
                    if (coinsEarnedText != null)
                    {
                        coinsEarnedText.text = $"+{data.coinsEarned}";
                    }
                }
            }

        }

        #region Win Screen Offer

        /// <summary>
        /// Adds the SDK's win_screen offer to this card when one is picked (frequency such as every_n_wins is remote
        /// config). Asked right before drawing, per SDK rule L6, and reported as shown once visible.
        /// </summary>
        private void TryShowWinOffer()
        {
            if (!isShowing || winOfferPurchaseInFlight || GrowthManager.Instance == null) return;

            OfferView offer = GrowthManager.Instance.GetOffer(OfferPlacementIds.WinScreen);
            if (offer == null || !EnsureWinOfferSection()) return;

            winOffer = offer;
            winOfferCoins = OfferPopupUI.CoinsGranted(offer);

            string title = offer.Title ?? string.Empty;
            if (!string.IsNullOrEmpty(offer.BadgeText)) title += $"  <color=#FF6B7A>{offer.BadgeText}</color>";
            winOfferTitleText.text = title;
            winOfferGrantText.text = winOfferCoins > 0 ? $"+{winOfferCoins}" : (offer.Body ?? string.Empty);
            winOfferBuyText.text = string.IsNullOrEmpty(offer.LocalizedPrice) ? "MUA" : offer.LocalizedPrice;
            winOfferStatusText.text = string.Empty;
            winOfferBuyButton.gameObject.SetActive(true);
            winOfferBuyButton.interactable = true;

            SetCardLifted(true);
            winOfferSection.gameObject.SetActive(true);
            offer.ReportShown();
        }

        private void HideWinOffer()
        {
            winOffer = null;
            if (winOfferSection != null) winOfferSection.gameObject.SetActive(false);
            SetCardLifted(false);
        }

        /// <summary>Moves the card up while the offer section hangs under it, so card + offer stay centred.</summary>
        private void SetCardLifted(bool lifted)
        {
            RectTransform cardRt = dialogContent as RectTransform;
            if (cardRt == null || cardLifted == lifted) return;
            Vector2 shift = new Vector2(0f, lifted ? WinOfferCardLift : -WinOfferCardLift);
            cardRt.anchorMin += shift;
            cardRt.anchorMax += shift;
            cardLifted = lifted;
        }

        private bool EnsureWinOfferSection()
        {
            if (winOfferSection != null) return true;
            if (dialogContent == null) return false;

            Font f = CoinUIHelper.GetDefaultFont(titleText != null ? titleText.font : null);

            // Hangs directly below the card (anchors below 0) with the card's own background, so it reads as one panel.
            GameObject section = OfferPopupUI.CreateChild("WinOfferSection", dialogContent, new Vector2(0f, -0.30f), new Vector2(1f, 0f));
            Image cardBg = dialogContent.GetComponent<Image>();
            section.AddComponent<Image>().color = cardBg != null ? cardBg.color : new Color(0.14f, 0.18f, 0.25f, 1f);
            winOfferSection = section.GetComponent<RectTransform>();

            GameObject divider = OfferPopupUI.CreateChild("Divider", section.transform, new Vector2(0.05f, 0.975f), new Vector2(0.95f, 1f));
            divider.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.12f);

            winOfferTitleText = OfferPopupUI.CreateText(section.transform, f, string.Empty, 38, FontStyle.Bold, Color.white,
                new Vector2(0.06f, 0.55f), new Vector2(0.62f, 0.90f));
            winOfferTitleText.alignment = TextAnchor.MiddleLeft;

            GameObject icon = OfferPopupUI.CreateChild("CoinIcon", section.transform, new Vector2(0.06f, 0.26f), new Vector2(0.14f, 0.54f));
            Image iconImg = icon.AddComponent<Image>();
            iconImg.sprite = CoinUIHelper.GetOrCreateCoinSprite();
            iconImg.preserveAspect = true;

            winOfferGrantText = OfferPopupUI.CreateText(section.transform, f, string.Empty, 36, FontStyle.Bold, new Color(1f, 0.86f, 0.2f, 1f),
                new Vector2(0.16f, 0.26f), new Vector2(0.62f, 0.54f));
            winOfferGrantText.alignment = TextAnchor.MiddleLeft;

            winOfferBuyButton = OfferPopupUI.CreateButton(section.transform, f, "WinOfferBuyButton", new Color(0.2f, 0.72f, 0.4f, 1f),
                new Vector2(0.64f, 0.28f), new Vector2(0.94f, 0.86f), 36, out winOfferBuyText);
            winOfferBuyButton.onClick.AddListener(OnWinOfferBuyClicked);

            winOfferStatusText = OfferPopupUI.CreateText(section.transform, f, string.Empty, 22, FontStyle.Italic, new Color(1f, 0.85f, 0.45f, 1f),
                new Vector2(0.05f, 0.03f), new Vector2(0.95f, 0.24f));

            section.SetActive(false);
            return true;
        }

        private void OnWinOfferBuyClicked()
        {
            if (winOffer == null || winOfferPurchaseInFlight) return;
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);

            OfferView offer = winOffer;
            winOfferPurchaseInFlight = true;
            winOfferBuyButton.interactable = false;
            winOfferStatusText.text = "Đang xử lý...";

            offer.Buy(result =>
            {
                winOfferPurchaseInFlight = false;
                Debug.Log($"[LevelCompleteUI] {offer.PlacementId}/{offer.OfferId} purchase result: {result}");
                if (this == null || winOffer != offer) return; // the card moved on while the store dialog was open

                winOfferBuyButton.interactable = true;
                string status = OfferPopupUI.StatusFor(result);
                switch (result.Outcome)
                {
                    case OfferPurchaseOutcome.Purchased:
                        // The SDK already credited the pack through the inventory adapter; only refresh what this card shows.
                        if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.CoinReward);
                        winOfferStatusText.text = winOfferCoins > 0 ? $"Đã nhận +{winOfferCoins} coin!" : "Mua thành công!";
                        winOfferBuyButton.gameObject.SetActive(false);
                        if (totalCoinsText != null && SaveManager.Instance != null)
                            totalCoinsText.text = $"{SaveManager.Instance.Coins}";
                        break;

                    case OfferPurchaseOutcome.Pending:
                        winOfferStatusText.text = status;
                        winOfferBuyButton.gameObject.SetActive(false);
                        break;

                    case OfferPurchaseOutcome.Failed:
                        winOfferStatusText.text = status;
                        break;

                    case OfferPurchaseOutcome.NotShown:
                        if (status != null) winOfferStatusText.text = status;
                        else HideWinOffer();
                        break;
                }
            });
        }

        #endregion

        // Player just watched a rewarded ad (2X coins): skip the interstitial on the next transition.
        private void OnReplayClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            HideModal();
            if (GrowthManager.Instance != null && !isDoubleClaimed)
            {
                GrowthManager.Instance.ShowInterstitial(Placement.AfterLevelComplete, () =>
                {
                    if (LevelManager.Instance != null) LevelManager.Instance.RestartLevel();
                });
            }
            else
            {
                if (LevelManager.Instance != null) LevelManager.Instance.RestartLevel();
            }
        }

        private void OnNextLevelClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            HideModal();
            if (GrowthManager.Instance != null && !isDoubleClaimed)
            {
                GrowthManager.Instance.ShowInterstitial(Placement.AfterLevelComplete, () =>
                {
                    if (LevelManager.Instance != null) LevelManager.Instance.LoadNextLevel();
                });
            }
            else
            {
                if (LevelManager.Instance != null) LevelManager.Instance.LoadNextLevel();
            }
        }

        private void OnLevelSelectClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            HideModal();
            if (GrowthManager.Instance != null && !isDoubleClaimed)
            {
                GrowthManager.Instance.ShowInterstitial(Placement.MapReturn, () =>
                {
                    if (GameStateManager.Instance != null) GameStateManager.Instance.SetState(GameState.LevelSelect);
                });
            }
            else
            {
                if (GameStateManager.Instance != null) GameStateManager.Instance.SetState(GameState.LevelSelect);
            }
        }

        private void HideModal()
        {
            isShowing = false;
            currentShownData = null;
            HideWinOffer();
            if (modalRoot != null) modalRoot.SetActive(false);
            else gameObject.SetActive(false);
        }
    }
}
