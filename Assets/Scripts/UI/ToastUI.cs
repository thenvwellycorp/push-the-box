using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PushTheBox.UI
{
    /// <summary>
    /// Short self-destroying message shown near the bottom of the root canvas (purchase / restore feedback).
    /// </summary>
    public class ToastUI : MonoBehaviour
    {
        private const float VisibleSeconds = 2.2f;
        private const float FadeSeconds = 0.4f;

        private static ToastUI current;

        public static void Show(Transform anchor, string message, Font font = null)
        {
            if (anchor == null || string.IsNullOrEmpty(message)) return;
            if (current != null) Destroy(current.gameObject);

            Canvas canvas = anchor.GetComponentInParent<Canvas>();
            Transform root = canvas != null ? canvas.rootCanvas.transform : anchor;

            GameObject obj = new GameObject("Toast", typeof(RectTransform), typeof(CanvasGroup));
            obj.transform.SetParent(root, false);
            obj.transform.SetAsLastSibling();
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.10f, 0.12f);
            rt.anchorMax = new Vector2(0.90f, 0.18f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Canvas overlay = obj.AddComponent<Canvas>();
            overlay.overrideSorting = true;
            overlay.sortingOrder = 110;

            Image bg = obj.AddComponent<Image>();
            bg.color = new Color(0.06f, 0.08f, 0.12f, 0.92f);
            bg.raycastTarget = false;

            GameObject txtObj = new GameObject("Text", typeof(RectTransform));
            txtObj.transform.SetParent(obj.transform, false);
            RectTransform txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(16f, 0f);
            txtRt.offsetMax = new Vector2(-16f, 0f);
            Text txt = txtObj.AddComponent<Text>();
            txt.text = message;
            txt.font = CoinUIHelper.GetDefaultFont(font);
            txt.fontSize = 30;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.raycastTarget = false;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = 16;
            txt.resizeTextMaxSize = 30;

            current = obj.AddComponent<ToastUI>();
        }

        private IEnumerator Start()
        {
            CanvasGroup group = GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            yield return new WaitForSecondsRealtime(VisibleSeconds);

            float elapsed = 0f;
            while (elapsed < FadeSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = 1f - elapsed / FadeSeconds;
                yield return null;
            }
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (current == this) current = null;
        }
    }
}
