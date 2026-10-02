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

        private void OnReplayClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            HideModal();
            if (GrowthManager.Instance != null)
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
            if (GrowthManager.Instance != null)
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
            if (GrowthManager.Instance != null)
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
            if (modalRoot != null) modalRoot.SetActive(false);
            else gameObject.SetActive(false);
        }
    }
}
