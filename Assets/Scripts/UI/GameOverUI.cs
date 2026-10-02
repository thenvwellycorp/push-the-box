using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PushTheBox.Core;
using PushTheBox.Level;
using PushTheBox.Audio;
using PushTheBox.Gameplay;
using Growth;
using PushTheBox.GrowthIntegration;

namespace PushTheBox.UI
{
    /// <summary>
    /// Modal dialog displayed when a corner deadlock occurs (Game Over).
    /// Provides options to Revive (Undo last move) or Retry (Restart level).
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [Header("Modal Containers")]
        [SerializeField] private GameObject modalRoot;
        [SerializeField] private Transform dialogContent;

        [Header("Information Labels")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text reasonText;
        [SerializeField] private Text descriptionText;

        [Header("Action Buttons")]
        [SerializeField] private Button reviveButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button levelSelectButton;

        private void Awake()
        {
            EnsureUI();
        }

        public void EnsureUI()
        {
            if (dialogContent != null && reviveButton != null && retryButton != null)
                return;

            Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (defaultFont == null) defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // Modal Root Stretch
            RectTransform rootRt = GetComponent<RectTransform>();
            if (rootRt == null) rootRt = gameObject.AddComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            modalRoot = gameObject;

            // Dark Backdrop
            Image modalDarkBg = GetComponent<Image>();
            if (modalDarkBg == null) modalDarkBg = gameObject.AddComponent<Image>();
            modalDarkBg.color = new Color(0.04f, 0.05f, 0.08f, 0.88f);

            // Dialog Card Panel
            Transform existingCard = transform.Find("CardPanel");
            GameObject cardObj;
            if (existingCard == null)
            {
                cardObj = new GameObject("CardPanel", typeof(RectTransform));
                cardObj.transform.SetParent(transform, false);
            }
            else
            {
                cardObj = existingCard.gameObject;
            }

            RectTransform cardRt = cardObj.GetComponent<RectTransform>();
            cardRt.anchorMin = new Vector2(0.10f, 0.22f);
            cardRt.anchorMax = new Vector2(0.90f, 0.78f);
            cardRt.offsetMin = Vector2.zero;
            cardRt.offsetMax = Vector2.zero;

            Image cardBg = cardObj.GetComponent<Image>();
            if (cardBg == null) cardBg = cardObj.AddComponent<Image>();
            cardBg.color = new Color(0.12f, 0.15f, 0.22f, 1f);

            dialogContent = cardObj.transform;

            // 1. Icon (Warning / Skull)
            Transform existingIcon = cardObj.transform.Find("WarningIcon");
            if (existingIcon == null)
            {
                GameObject iconObj = new GameObject("WarningIcon", typeof(RectTransform));
                iconObj.transform.SetParent(cardObj.transform, false);
                RectTransform iconRt = iconObj.GetComponent<RectTransform>();
                iconRt.anchorMin = new Vector2(0.35f, 0.82f);
                iconRt.anchorMax = new Vector2(0.65f, 0.96f);
                iconRt.offsetMin = Vector2.zero;
                iconRt.offsetMax = Vector2.zero;

                Text iconTxt = iconObj.AddComponent<Text>();
                iconTxt.text = "⚠️";
                iconTxt.font = defaultFont;
                iconTxt.fontSize = 58;
                iconTxt.alignment = TextAnchor.MiddleCenter;
                iconTxt.color = new Color(1f, 0.35f, 0.35f, 1f);
            }

            // 2. Title Text "GAME OVER"
            if (titleText == null)
            {
                Transform existingTitle = cardObj.transform.Find("TitleText");
                GameObject titleObj = existingTitle != null ? existingTitle.gameObject : new GameObject("TitleText", typeof(RectTransform));
                titleObj.transform.SetParent(cardObj.transform, false);

                RectTransform titleRt = titleObj.GetComponent<RectTransform>();
                titleRt.anchorMin = new Vector2(0.05f, 0.70f);
                titleRt.anchorMax = new Vector2(0.95f, 0.83f);
                titleRt.offsetMin = Vector2.zero;
                titleRt.offsetMax = Vector2.zero;

                titleText = titleObj.GetComponent<Text>();
                if (titleText == null) titleText = titleObj.AddComponent<Text>();
                titleText.text = "GAME OVER";
                titleText.font = defaultFont;
                titleText.fontSize = 50;
                titleText.fontStyle = FontStyle.Bold;
                titleText.alignment = TextAnchor.MiddleCenter;
                titleText.color = new Color(1f, 0.28f, 0.28f, 1f);
            }

            // 3. Reason Text "CORNER DEADLOCK!"
            if (reasonText == null)
            {
                Transform existingReason = cardObj.transform.Find("ReasonText");
                GameObject reasonObj = existingReason != null ? existingReason.gameObject : new GameObject("ReasonText", typeof(RectTransform));
                reasonObj.transform.SetParent(cardObj.transform, false);

                RectTransform reasonRt = reasonObj.GetComponent<RectTransform>();
                reasonRt.anchorMin = new Vector2(0.05f, 0.60f);
                reasonRt.anchorMax = new Vector2(0.95f, 0.70f);
                reasonRt.offsetMin = Vector2.zero;
                reasonRt.offsetMax = Vector2.zero;

                reasonText = reasonObj.GetComponent<Text>();
                if (reasonText == null) reasonText = reasonObj.AddComponent<Text>();
                reasonText.text = "KẸT GÓC (CORNER DEADLOCK)";
                reasonText.font = defaultFont;
                reasonText.fontSize = 28;
                reasonText.fontStyle = FontStyle.Bold;
                reasonText.alignment = TextAnchor.MiddleCenter;
                reasonText.color = new Color(1f, 0.82f, 0.2f, 1f);
            }

            // 4. Description Text
            if (descriptionText == null)
            {
                Transform existingDesc = cardObj.transform.Find("DescriptionText");
                GameObject descObj = existingDesc != null ? existingDesc.gameObject : new GameObject("DescriptionText", typeof(RectTransform));
                descObj.transform.SetParent(cardObj.transform, false);

                RectTransform descRt = descObj.GetComponent<RectTransform>();
                descRt.anchorMin = new Vector2(0.08f, 0.44f);
                descRt.anchorMax = new Vector2(0.92f, 0.59f);
                descRt.offsetMin = Vector2.zero;
                descRt.offsetMax = Vector2.zero;

                descriptionText = descObj.GetComponent<Text>();
                if (descriptionText == null) descriptionText = descObj.AddComponent<Text>();
                descriptionText.text = "Hộp đã bị đẩy vào góc tường không thể di chuyển!\nHãy dùng Hồi Sinh để hoàn tác bước vừa rồi.";
                descriptionText.font = defaultFont;
                descriptionText.fontSize = 22;
                descriptionText.alignment = TextAnchor.MiddleCenter;
                descriptionText.color = new Color(0.85f, 0.88f, 0.95f, 0.85f);
            }

            // 5. Revive Button (Big Emerald Green)
            if (reviveButton == null)
            {
                Transform existingRevive = cardObj.transform.Find("ReviveButton");
                GameObject revObj = existingRevive != null ? existingRevive.gameObject : new GameObject("ReviveButton", typeof(RectTransform));
                revObj.transform.SetParent(cardObj.transform, false);

                RectTransform revRt = revObj.GetComponent<RectTransform>();
                revRt.anchorMin = new Vector2(0.10f, 0.27f);
                revRt.anchorMax = new Vector2(0.90f, 0.41f);
                revRt.offsetMin = Vector2.zero;
                revRt.offsetMax = Vector2.zero;

                Image revBg = revObj.GetComponent<Image>();
                if (revBg == null) revBg = revObj.AddComponent<Image>();
                revBg.color = new Color(0.13f, 0.75f, 0.42f, 1f);

                reviveButton = revObj.GetComponent<Button>();
                if (reviveButton == null) reviveButton = revObj.AddComponent<Button>();
                ColorBlock cbRev = reviveButton.colors;
                cbRev.highlightedColor = new Color(0.25f, 0.88f, 0.52f, 1f);
                cbRev.pressedColor = new Color(0.08f, 0.58f, 0.32f, 1f);
                reviveButton.colors = cbRev;

                Transform existingRevTxt = revObj.transform.Find("Text");
                GameObject revTxtObj = existingRevTxt != null ? existingRevTxt.gameObject : new GameObject("Text", typeof(RectTransform));
                revTxtObj.transform.SetParent(revObj.transform, false);
                RectTransform revTxtRt = revTxtObj.GetComponent<RectTransform>();
                revTxtRt.anchorMin = Vector2.zero;
                revTxtRt.anchorMax = Vector2.one;
                revTxtRt.offsetMin = Vector2.zero;
                revTxtRt.offsetMax = Vector2.zero;

                Text revTxt = revTxtObj.GetComponent<Text>();
                if (revTxt == null) revTxt = revTxtObj.AddComponent<Text>();
                revTxt.text = "↺ HỒI SINH (UNDO)";
                revTxt.font = defaultFont;
                revTxt.fontSize = 32;
                revTxt.fontStyle = FontStyle.Bold;
                revTxt.alignment = TextAnchor.MiddleCenter;
                revTxt.color = Color.white;
            }

            // 6. Retry Button (Orange)
            if (retryButton == null)
            {
                Transform existingRetry = cardObj.transform.Find("RetryButton");
                GameObject retObj = existingRetry != null ? existingRetry.gameObject : new GameObject("RetryButton", typeof(RectTransform));
                retObj.transform.SetParent(cardObj.transform, false);

                RectTransform retRt = retObj.GetComponent<RectTransform>();
                retRt.anchorMin = new Vector2(0.10f, 0.12f);
                retRt.anchorMax = new Vector2(0.52f, 0.24f);
                retRt.offsetMin = Vector2.zero;
                retRt.offsetMax = Vector2.zero;

                Image retBg = retObj.GetComponent<Image>();
                if (retBg == null) retBg = retObj.AddComponent<Image>();
                retBg.color = new Color(0.92f, 0.52f, 0.14f, 1f);

                retryButton = retObj.GetComponent<Button>();
                if (retryButton == null) retryButton = retObj.AddComponent<Button>();
                ColorBlock cbRet = retryButton.colors;
                cbRet.highlightedColor = new Color(1f, 0.65f, 0.25f, 1f);
                cbRet.pressedColor = new Color(0.75f, 0.38f, 0.08f, 1f);
                retryButton.colors = cbRet;

                Transform existingRetTxt = retObj.transform.Find("Text");
                GameObject retTxtObj = existingRetTxt != null ? existingRetTxt.gameObject : new GameObject("Text", typeof(RectTransform));
                retTxtObj.transform.SetParent(retObj.transform, false);
                RectTransform retTxtRt = retTxtObj.GetComponent<RectTransform>();
                retTxtRt.anchorMin = Vector2.zero;
                retTxtRt.anchorMax = Vector2.one;
                retTxtRt.offsetMin = Vector2.zero;
                retTxtRt.offsetMax = Vector2.zero;

                Text retTxt = retTxtObj.GetComponent<Text>();
                if (retTxt == null) retTxt = retTxtObj.AddComponent<Text>();
                retTxt.text = "↻ CHƠI LẠI";
                retTxt.font = defaultFont;
                retTxt.fontSize = 24;
                retTxt.fontStyle = FontStyle.Bold;
                retTxt.alignment = TextAnchor.MiddleCenter;
                retTxt.color = Color.white;
            }

            // 7. Level Select Button (Slate)
            if (levelSelectButton == null)
            {
                Transform existingLs = cardObj.transform.Find("LevelSelectButton");
                GameObject lsObj = existingLs != null ? existingLs.gameObject : new GameObject("LevelSelectButton", typeof(RectTransform));
                lsObj.transform.SetParent(cardObj.transform, false);

                RectTransform lsRt = lsObj.GetComponent<RectTransform>();
                lsRt.anchorMin = new Vector2(0.55f, 0.12f);
                lsRt.anchorMax = new Vector2(0.90f, 0.24f);
                lsRt.offsetMin = Vector2.zero;
                lsRt.offsetMax = Vector2.zero;

                Image lsBg = lsObj.GetComponent<Image>();
                if (lsBg == null) lsBg = lsObj.AddComponent<Image>();
                lsBg.color = new Color(0.25f, 0.28f, 0.38f, 1f);

                levelSelectButton = lsObj.GetComponent<Button>();
                if (levelSelectButton == null) levelSelectButton = lsObj.AddComponent<Button>();
                ColorBlock cbLs = levelSelectButton.colors;
                cbLs.highlightedColor = new Color(0.35f, 0.40f, 0.52f, 1f);
                cbLs.pressedColor = new Color(0.18f, 0.20f, 0.28f, 1f);
                levelSelectButton.colors = cbLs;

                Transform existingLsTxt = lsObj.transform.Find("Text");
                GameObject lsTxtObj = existingLsTxt != null ? existingLsTxt.gameObject : new GameObject("Text", typeof(RectTransform));
                lsTxtObj.transform.SetParent(lsObj.transform, false);
                RectTransform lsTxtRt = lsTxtObj.GetComponent<RectTransform>();
                lsTxtRt.anchorMin = Vector2.zero;
                lsTxtRt.anchorMax = Vector2.one;
                lsTxtRt.offsetMin = Vector2.zero;
                lsTxtRt.offsetMax = Vector2.zero;

                Text lsTxt = lsTxtObj.GetComponent<Text>();
                if (lsTxt == null) lsTxt = lsTxtObj.AddComponent<Text>();
                lsTxt.text = "☰ CHỌN MÀN";
                lsTxt.font = defaultFont;
                lsTxt.fontSize = 24;
                lsTxt.fontStyle = FontStyle.Bold;
                lsTxt.alignment = TextAnchor.MiddleCenter;
                lsTxt.color = new Color(0.85f, 0.88f, 0.95f, 0.9f);
            }
        }

        private void OnEnable()
        {
            EnsureUI();

            if (reviveButton != null)
            {
                reviveButton.onClick.RemoveListener(OnReviveClicked);
                reviveButton.onClick.AddListener(OnReviveClicked);
            }
            if (retryButton != null)
            {
                retryButton.onClick.RemoveListener(OnRetryClicked);
                retryButton.onClick.AddListener(OnRetryClicked);
            }
            if (levelSelectButton != null)
            {
                levelSelectButton.onClick.RemoveListener(OnLevelSelectClicked);
                levelSelectButton.onClick.AddListener(OnLevelSelectClicked);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(SoundType.GameOver);
            }

            StopAllCoroutines();
            StartCoroutine(AnimateEntrance());
        }

        private void OnDisable()
        {
            if (reviveButton != null) reviveButton.onClick.RemoveListener(OnReviveClicked);
            if (retryButton != null) retryButton.onClick.RemoveListener(OnRetryClicked);
            if (levelSelectButton != null) levelSelectButton.onClick.RemoveListener(OnLevelSelectClicked);
        }

        private IEnumerator AnimateEntrance()
        {
            if (dialogContent == null) yield break;

            float dur = 0.25f;
            float elapsed = 0f;
            Vector3 targetScale = Vector3.one;
            dialogContent.localScale = Vector3.one * 0.8f;

            while (elapsed < dur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / dur;
                // Pop scale with slight overshoot
                float scale = Mathf.Lerp(0.8f, 1.05f, Mathf.Sin(t * Mathf.PI * 0.5f));
                if (t >= 0.8f)
                {
                    scale = Mathf.Lerp(1.05f, 1.0f, (t - 0.8f) / 0.2f);
                }
                dialogContent.localScale = Vector3.one * scale;
                yield return null;
            }
            dialogContent.localScale = targetScale;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.U))
            {
                OnReviveClicked();
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                OnRetryClicked();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnLevelSelectClicked();
            }
        }

        private void OnReviveClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);

            if (GrowthManager.Instance != null)
            {
                GrowthManager.Instance.ShowRewarded(
                    Placement.ReviveOffer,
                    RewardType.Revive,
                    1,
                    onRewardGranted: () =>
                    {
                        Hide();
                        if (LevelManager.Instance != null)
                        {
                            LevelManager.Instance.ReviveFromGameOver();
                        }
                    });
            }
            else
            {
                Hide();
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.ReviveFromGameOver();
                }
            }
        }

        private void OnRetryClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            Hide();

            if (GrowthManager.Instance != null)
            {
                GrowthManager.Instance.ShowInterstitial(Placement.AfterLevelFail, () =>
                {
                    if (LevelManager.Instance != null)
                    {
                        LevelManager.Instance.RestartLevel();
                    }
                });
            }
            else
            {
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.RestartLevel();
                }
            }
        }

        private void OnLevelSelectClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            Hide();

            if (GrowthManager.Instance != null)
            {
                GrowthManager.Instance.ShowInterstitial(Placement.MapReturn, () =>
                {
                    if (GameStateManager.Instance != null)
                    {
                        GameStateManager.Instance.SetState(GameState.LevelSelect);
                    }
                });
            }
            else
            {
                if (GameStateManager.Instance != null)
                {
                    GameStateManager.Instance.SetState(GameState.LevelSelect);
                }
            }
        }

        private void Hide()
        {
            if (modalRoot != null) modalRoot.SetActive(false);
            else gameObject.SetActive(false);
        }
    }
}
