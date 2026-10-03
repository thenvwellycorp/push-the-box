using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Growth;
using PushTheBox.Audio;

namespace PushTheBox.UI
{
    /// <summary>
    /// Popup that draws a Growth SDK in-game offer (outside the Shop) and runs its purchase flow.
    /// The SDK credits coins / entitlements itself — this popup only renders the offer and reacts to the outcome.
    /// </summary>
    public class OfferPopupUI : MonoBehaviour
    {
        private OfferView view;
        private Action<OfferPurchaseOutcome?> onClosed;
        private Button buyButton;
        private Text buyButtonText;
        private Button closeButton;
        private Text statusText;
        private bool purchaseInFlight;

        /// <summary>
        /// Draws the offer on top of the canvas that owns <paramref name="anchor"/> and reports it as shown.
        /// onClosed receives the final purchase outcome, or null if the player dismissed it without buying.
        /// </summary>
        public static OfferPopupUI Show(Transform anchor, OfferView offer, Font font, Action<OfferPurchaseOutcome?> onClosed = null)
        {
            if (anchor == null || offer == null) return null;

            Canvas canvas = anchor.GetComponentInParent<Canvas>();
            Transform root = canvas != null ? canvas.rootCanvas.transform : anchor;

            GameObject obj = new GameObject("OfferPopup_" + offer.PlacementId, typeof(RectTransform));
            obj.transform.SetParent(root, false);
            obj.transform.SetAsLastSibling();

            var popup = obj.AddComponent<OfferPopupUI>();
            popup.view = offer;
            popup.onClosed = onClosed;
            popup.Build(CoinUIHelper.GetDefaultFont(font));

            offer.ReportShown();
            return popup;
        }

        private void Build(Font font)
        {
            RectTransform rootRt = (RectTransform)transform;
            Stretch(rootRt, Vector2.zero, Vector2.one);

            // Draw above every other view of the root canvas.
            Canvas overlay = gameObject.AddComponent<Canvas>();
            overlay.overrideSorting = true;
            overlay.sortingOrder = 100;
            gameObject.AddComponent<GraphicRaycaster>();

            Image blocker = gameObject.AddComponent<Image>();
            blocker.color = new Color(0.02f, 0.03f, 0.06f, 0.78f);

            GameObject panel = CreateChild("Panel", transform, new Vector2(0.08f, 0.27f), new Vector2(0.92f, 0.73f));
            panel.AddComponent<Image>().color = new Color(0.14f, 0.18f, 0.25f, 1f);

            if (!string.IsNullOrEmpty(view.BadgeText))
            {
                GameObject badge = CreateChild("Badge", panel.transform, new Vector2(0.70f, 0.90f), new Vector2(0.96f, 0.98f));
                badge.AddComponent<Image>().color = new Color(0.90f, 0.22f, 0.28f, 1f);
                CreateText(badge.transform, font, view.BadgeText, 28, FontStyle.Bold, Color.white, Vector2.zero, Vector2.one);
            }

            CreateText(panel.transform, font, view.Title ?? string.Empty, 56, FontStyle.Bold, Color.white,
                new Vector2(0.06f, 0.78f), new Vector2(0.94f, 0.90f));

            string description = BuildDescription();
            if (description.Length > 0)
            {
                Text desc = CreateText(panel.transform, font, description, 32, FontStyle.Normal, new Color(0.85f, 0.90f, 0.98f, 1f),
                    new Vector2(0.08f, 0.50f), new Vector2(0.92f, 0.77f));
                desc.alignment = view.Bullets.Count > 0 ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter;
            }

            long coins = CoinsGranted(view);
            if (coins > 0)
            {
                GameObject row = CreateChild("GrantRow", panel.transform, new Vector2(0.20f, 0.38f), new Vector2(0.80f, 0.49f));
                row.AddComponent<Image>().color = new Color(0.18f, 0.24f, 0.35f, 0.85f);

                GameObject icon = CreateChild("CoinIcon", row.transform, new Vector2(0.08f, 0.10f), new Vector2(0.30f, 0.90f));
                Image iconImg = icon.AddComponent<Image>();
                iconImg.sprite = CoinUIHelper.GetOrCreateCoinSprite();
                iconImg.preserveAspect = true;

                Text amount = CreateText(row.transform, font, $"+{coins}", 44, FontStyle.Bold, new Color(1f, 0.86f, 0.2f, 1f),
                    new Vector2(0.32f, 0f), new Vector2(0.95f, 1f));
                amount.alignment = TextAnchor.MiddleLeft;
            }

            statusText = CreateText(panel.transform, font, string.Empty, 26, FontStyle.Italic, new Color(1f, 0.85f, 0.45f, 1f),
                new Vector2(0.06f, 0.29f), new Vector2(0.94f, 0.37f));

            buyButton = CreateButton(panel.transform, font, "BuyButton", new Color(0.2f, 0.72f, 0.4f, 1f),
                new Vector2(0.12f, 0.14f), new Vector2(0.88f, 0.28f), 44, out buyButtonText);
            buyButtonText.text = string.IsNullOrEmpty(view.LocalizedPrice) ? "MUA" : $"MUA  {view.LocalizedPrice}";
            buyButton.onClick.AddListener(OnBuyClicked);

            closeButton = CreateButton(panel.transform, font, "CloseButton", new Color(0.32f, 0.38f, 0.48f, 1f),
                new Vector2(0.30f, 0.03f), new Vector2(0.70f, 0.115f), 30, out Text closeText);
            closeText.text = "Để sau";
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        private string BuildDescription()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(view.Body)) sb.Append(view.Body);
            foreach (string bullet in view.Bullets)
            {
                if (string.IsNullOrEmpty(bullet)) continue;
                if (sb.Length > 0) sb.Append('\n');
                sb.Append("• ").Append(bullet);
            }
            return sb.ToString();
        }

        private void OnBuyClicked()
        {
            if (purchaseInFlight) return;
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);

            SetBusy(true);
            statusText.text = "Đang xử lý...";

            view.Buy(result =>
            {
                if (this == null) return; // popup destroyed while the store dialog was open
                SetBusy(false);
                Debug.Log($"[OfferPopupUI] {view.PlacementId}/{view.OfferId} purchase result: {result}");

                string status = StatusFor(result);
                switch (result.Outcome)
                {
                    case OfferPurchaseOutcome.Purchased:
                        if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.CoinReward);
                        Close(OfferPurchaseOutcome.Purchased);
                        break;

                    case OfferPurchaseOutcome.Pending:
                        statusText.text = status;
                        buyButton.gameObject.SetActive(false);
                        break;

                    case OfferPurchaseOutcome.Failed:
                        statusText.text = status;
                        break;

                    case OfferPurchaseOutcome.NotShown:
                        if (status != null) statusText.text = status;
                        else Close(OfferPurchaseOutcome.NotShown);
                        break;
                }
            });
        }

        /// <summary>
        /// Player-facing status for a purchase result, shared by every offer view. Null means there is nothing to say:
        /// Purchased, or a NotShown the offer cannot recover from (expired / already owned / not configured).
        /// </summary>
        public static string StatusFor(OfferPurchaseResult result)
        {
            switch (result.Outcome)
            {
                case OfferPurchaseOutcome.Pending:
                    // Rewards arrive later through the SDK (BalanceChanged / OwnershipChanged) — never credit here.
                    return "Đang chờ thanh toán. Quà sẽ tự về khi giao dịch hoàn tất.";
                case OfferPurchaseOutcome.Failed:
                    return result.Reason == "user_cancelled" ? string.Empty : "Mua không thành công, vui lòng thử lại.";
                case OfferPurchaseOutcome.NotShown:
                    if (result.Reason == OfferPurchaseReasons.PurchaseInProgress) return "Đang có giao dịch khác, vui lòng đợi.";
                    if (result.Reason == OfferPurchaseReasons.StoreUnavailable) return "Cửa hàng chưa sẵn sàng, vui lòng thử lại sau.";
                    return null;
                default:
                    return null;
            }
        }

        /// <summary>Total coins granted by an offer (the only resource of this game).</summary>
        public static long CoinsGranted(OfferView offer)
        {
            long coins = 0;
            foreach (var grant in offer.Grants)
            {
                if (grant.ResourceId == "coin") coins += grant.Amount;
            }
            return coins;
        }

        private void OnCloseClicked()
        {
            if (purchaseInFlight) return;
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySound(SoundType.ButtonClick);
            Close(null);
        }

        /// <summary>Closes the popup when the owning screen goes away. A popup with a purchase in flight stays until the store answers.</summary>
        public void Dismiss()
        {
            if (purchaseInFlight || this == null) return;
            Close(null);
        }

        private void SetBusy(bool busy)
        {
            purchaseInFlight = busy;
            buyButton.interactable = !busy;
            closeButton.interactable = !busy;
        }

        private void Close(OfferPurchaseOutcome? outcome)
        {
            var callback = onClosed;
            onClosed = null;
            Destroy(gameObject);
            callback?.Invoke(outcome);
        }

        #region UI Builders

        private static void Stretch(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        internal static GameObject CreateChild(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            Stretch(obj.GetComponent<RectTransform>(), anchorMin, anchorMax);
            return obj;
        }

        internal static Text CreateText(Transform parent, Font font, string content, int size, FontStyle style, Color color,
                                       Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject obj = CreateChild("Text", parent, anchorMin, anchorMax);
            Text txt = obj.AddComponent<Text>();
            txt.text = content;
            txt.font = font;
            txt.fontSize = size;
            txt.fontStyle = style;
            txt.color = color;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.raycastTarget = false;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = Mathf.Max(14, size / 2);
            txt.resizeTextMaxSize = size;
            return txt;
        }

        internal static Button CreateButton(Transform parent, Font font, string name, Color color,
                                           Vector2 anchorMin, Vector2 anchorMax, int fontSize, out Text label)
        {
            GameObject obj = CreateChild(name, parent, anchorMin, anchorMax);
            obj.AddComponent<Image>().color = color;
            Button btn = obj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.disabledColor = new Color(0.55f, 0.55f, 0.6f, 0.7f);
            btn.colors = cb;
            label = CreateText(obj.transform, font, string.Empty, fontSize, FontStyle.Bold, Color.white, Vector2.zero, Vector2.one);
            return btn;
        }

        #endregion
    }
}
