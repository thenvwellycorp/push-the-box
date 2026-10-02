using UnityEngine;
using UnityEngine.UI;

namespace PushTheBox.UI
{
    /// <summary>
    /// Utility class providing procedural runtime Coin UI elements and sprites.
    /// Ensures coin badges and reward rows display immediately even if not configured in the scene.
    /// </summary>
    public static class CoinUIHelper
    {
        private static Sprite cachedCoinSprite;

        public static Sprite GetOrCreateCoinSprite()
        {
            if (cachedCoinSprite != null)
                return cachedCoinSprite;

            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color transparent = new Color(0, 0, 0, 0);
            Color border = new Color(0.78f, 0.48f, 0.05f, 1f);
            Color rim = new Color(1f, 0.84f, 0.15f, 1f);
            Color body = new Color(0.98f, 0.68f, 0.08f, 1f);
            Color star = new Color(1f, 0.96f, 0.55f, 1f);
            Color highlight = new Color(1f, 1f, 1f, 0.85f);

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float outerR = size * 0.45f;
            float innerR = size * 0.36f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pt = new Vector2(x, y) - center;
                    float dist = pt.magnitude;

                    if (dist > outerR)
                    {
                        tex.SetPixel(x, y, transparent);
                    }
                    else if (dist > outerR - 2.5f)
                    {
                        tex.SetPixel(x, y, border);
                    }
                    else if (dist > innerR)
                    {
                        float t = (pt.y / outerR + 1f) * 0.5f;
                        Color c = Color.Lerp(rim * 0.88f, rim, t);
                        tex.SetPixel(x, y, c);
                    }
                    else if (dist > innerR - 2f)
                    {
                        tex.SetPixel(x, y, border);
                    }
                    else
                    {
                        float angle = Mathf.Atan2(pt.y, pt.x);
                        float starR = (innerR * 0.45f) + (innerR * 0.35f) * Mathf.Cos(5f * angle);
                        if (dist <= starR)
                        {
                            tex.SetPixel(x, y, star);
                        }
                        else
                        {
                            float t = (pt.y / innerR + 1f) * 0.5f;
                            Color c = Color.Lerp(body * 0.85f, body * 1.08f, t);
                            tex.SetPixel(x, y, c);
                        }
                    }

                    // Specular highlight arc
                    if (dist < outerR - 3f && dist > innerR + 1f && pt.x < 0 && pt.y > 0)
                    {
                        float angle = Mathf.Atan2(pt.y, -pt.x);
                        if (angle > 0.5f && angle < 1.15f)
                        {
                            tex.SetPixel(x, y, highlight);
                        }
                    }
                }
            }

            tex.Apply();
            cachedCoinSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return cachedCoinSprite;
        }

        public static Font GetDefaultFont(Font fallback)
        {
            if (fallback != null) return fallback;
            Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return f;
        }

        /// <summary>
        /// Creates a sleek, modern coin counter badge at runtime and attaches it to parent.
        /// </summary>
        public static Text CreateBadge(Transform parent, Font font, Vector2 anchorMin, Vector2 anchorMax, string name = "CoinBadge")
        {
            if (parent == null) return null;

            GameObject badge = new GameObject(name, typeof(RectTransform));
            badge.transform.SetParent(parent, false);

            RectTransform rt = badge.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = badge.AddComponent<Image>();
            bg.color = new Color(0.10f, 0.14f, 0.22f, 0.88f);

            // Coin Icon
            GameObject iconObj = new GameObject("CoinIcon", typeof(RectTransform));
            iconObj.transform.SetParent(badge.transform, false);
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.06f, 0.12f);
            iconRt.anchorMax = new Vector2(0.36f, 0.88f);
            iconRt.offsetMin = Vector2.zero;
            iconRt.offsetMax = Vector2.zero;
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = GetOrCreateCoinSprite();
            iconImg.preserveAspect = true;

            // Coin Text
            GameObject txtObj = new GameObject("CoinText", typeof(RectTransform));
            txtObj.transform.SetParent(badge.transform, false);
            RectTransform txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = new Vector2(0.38f, 0.05f);
            txtRt.anchorMax = new Vector2(0.96f, 0.95f);
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;
            Text txt = txtObj.AddComponent<Text>();
            txt.text = "0";
            txt.font = GetDefaultFont(font);
            txt.fontSize = 32;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.color = new Color(1f, 0.86f, 0.2f, 1f);

            return txt;
        }

        /// <summary>
        /// Creates a reward display row inside the LevelComplete modal.
        /// </summary>
        public static (Text earnedTxt, Text totalTxt, GameObject container) CreateRewardRow(Transform parent, Font font)
        {
            if (parent == null) return (null, null, null);

            GameObject row = new GameObject("CoinRewardRow", typeof(RectTransform));
            row.transform.SetParent(parent, false);

            RectTransform rowRt = row.GetComponent<RectTransform>();
            rowRt.anchorMin = new Vector2(0.12f, 0.46f);
            rowRt.anchorMax = new Vector2(0.88f, 0.60f);
            rowRt.offsetMin = Vector2.zero;
            rowRt.offsetMax = Vector2.zero;

            Image bg = row.AddComponent<Image>();
            bg.color = new Color(0.18f, 0.24f, 0.35f, 0.85f);

            HorizontalLayoutGroup hlg = row.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 16f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            // Coin Icon
            GameObject iconObj = new GameObject("RewardCoinIcon", typeof(RectTransform));
            iconObj.transform.SetParent(row.transform, false);
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.sizeDelta = new Vector2(54, 54);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = GetOrCreateCoinSprite();
            iconImg.preserveAspect = true;

            // Earned Text
            GameObject earnedObj = new GameObject("CoinsEarnedText", typeof(RectTransform));
            earnedObj.transform.SetParent(row.transform, false);
            RectTransform earnedRt = earnedObj.GetComponent<RectTransform>();
            earnedRt.sizeDelta = new Vector2(160, 54);
            Text earnedTxt = earnedObj.AddComponent<Text>();
            earnedTxt.text = "+50";
            earnedTxt.font = GetDefaultFont(font);
            earnedTxt.fontSize = 42;
            earnedTxt.fontStyle = FontStyle.Bold;
            earnedTxt.alignment = TextAnchor.MiddleLeft;
            earnedTxt.color = new Color(1f, 0.86f, 0.2f, 1f);

            // Total Text
            GameObject totalObj = new GameObject("TotalCoinsText", typeof(RectTransform));
            totalObj.transform.SetParent(row.transform, false);
            RectTransform totalRt = totalObj.GetComponent<RectTransform>();
            totalRt.sizeDelta = new Vector2(160, 54);
            Text totalTxt = totalObj.AddComponent<Text>();
            totalTxt.text = "Coins";
            totalTxt.font = GetDefaultFont(font);
            totalTxt.fontSize = 26;
            totalTxt.alignment = TextAnchor.MiddleLeft;
            totalTxt.color = new Color(0.85f, 0.90f, 0.98f, 0.85f);

            return (earnedTxt, totalTxt, row);
        }

        /// <summary>
        /// Creates a high-impact Double Coins button.
        /// </summary>
        public static (Button button, Text labelText) CreateDoubleCoinsButton(Transform parent, Font font, Vector2 anchorMin, Vector2 anchorMax)
        {
            if (parent == null) return (null, null);

            GameObject btnObj = new GameObject("DoubleCoinsButton", typeof(RectTransform));
            btnObj.transform.SetParent(parent, false);

            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = btnObj.AddComponent<Image>();
            bg.color = new Color(0.96f, 0.58f, 0.12f, 1f); // Vibrant Gold-Orange

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(1f, 0.70f, 0.25f, 1f);
            cb.pressedColor = new Color(0.80f, 0.45f, 0.08f, 1f);
            cb.disabledColor = new Color(0.4f, 0.4f, 0.45f, 0.6f);
            btn.colors = cb;

            GameObject txtObj = new GameObject("Label", typeof(RectTransform));
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;

            Text txt = txtObj.AddComponent<Text>();
            txt.text = "▶ CLAIM 2X COIN";
            txt.font = GetDefaultFont(font);
            txt.fontSize = 38;
            txt.fontStyle = FontStyle.Bold;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.resizeTextForBestFit = true;
            txt.resizeTextMinSize = 22;
            txt.resizeTextMaxSize = 42;

            return (btn, txt);
        }
    }
}
