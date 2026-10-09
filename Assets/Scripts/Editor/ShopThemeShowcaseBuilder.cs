using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Growth;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PushTheBox.EditorTools
{
    /// <summary>
    /// Builds the "Neon Warehouse" Shop theme (Assets/Resources/PushTheBoxShopTheme.asset) purely from game-side data:
    /// procedurally drawn sprites, TMP material presets and every ShopTheme field the SDK reads. No SDK code is touched.
    /// Also renders a default-vs-themed preview of the Shop screen so the SDK's theming range can be reviewed.
    /// </summary>
    public static class ShopThemeShowcaseBuilder
    {
        private const string ArtDir = "Assets/Art/ShopTheme";
        private const string ThemePath = "Assets/Resources/PushTheBoxShopTheme.asset";
        private const string FontDir = "Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/";

        // Palette — deliberately far from the SDK default (navy / orange cartoon).
        private static readonly Color Ink = Hex("120A26");
        private static readonly Color Violet = Hex("2A1458");
        private static readonly Color DeepViolet = Hex("1A0D3A");
        private static readonly Color Cyan = Hex("22E6FF");
        private static readonly Color Magenta = Hex("FF2BD6");
        private static readonly Color Lime = Hex("B6FF3B");
        private static readonly Color Amber = Hex("FFB61E");
        private static readonly Color Teal = Hex("0E6B73");
        private static readonly Color Pink = Hex("FF5C9A");
        private static readonly Color Cream = Hex("FFF4D6");

        [MenuItem("Push The Box/Shop Theme/Build Neon Warehouse Theme")]
        public static void BuildFromMenu()
        {
            Build();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<ShopTheme>(ThemePath);
        }

        /// <summary>Batch / menu entry point: builds the theme then renders the preview (Temp/ShopPreview by default).</summary>
        [MenuItem("Push The Box/Shop Theme/Build Theme + Render Preview")]
        public static void BuildAndPreview()
        {
            Build();
            var output = Environment.GetEnvironmentVariable("SHOP_PREVIEW_DIR");
            if (string.IsNullOrEmpty(output)) output = Path.GetFullPath("Temp/ShopPreview");
            Preview(output);
        }

        // ------------------------------------------------------------------ theme

        public static void Build()
        {
            Directory.CreateDirectory(ArtDir);
            Directory.CreateDirectory(ArtDir + "/Fonts");

            // --- sprites
            var screenBg = MakeSprite("screen_bg", 540, 1170, p => p.Gradient(Hex("2B0F57"), Hex("07142E")).Grid(new Color(0.35f, 0.9f, 1f, 0.06f), 45).Vignette(0.55f));
            var topBar = MakeSprite("top_bar", 543, 363, p => p.RoundRect(0, 0, 543, 363, 40, Hex("3A1A78"), Hex("150A33"), Magenta, 6).Stripes(new Color(1, 1, 1, 0.05f), 28));
            var close = MakeSprite("button_close", 108, 108, p => p.Circle(Magenta, Hex("8A0F7A"), Cream, 6).Cross(Cream, 7, 30));
            var secondaryButton = MakeSprite("button_secondary", 246, 50, p => p.RoundRect(0, 0, 246, 50, 25, Cyan, Hex("0B8FA6"), Ink, 3));
            var walletBar = MakeSprite("wallet_pill", 222, 96, p => p.RoundRect(0, 0, 222, 96, 48, Hex("241047"), Hex("120A26"), Cyan, 5));
            var retryLike = secondaryButton;

            var shelfPrimary = MakeSprite("shelf_primary", 402, 76, p => p.Ribbon(Magenta, Hex("FF7A1E"), Cream, 3));
            var shelfSecondary = MakeSprite("shelf_secondary", 402, 76, p => p.Ribbon(Cyan, Hex("1E5BFF"), Cream, 3));

            var badgeHighlight = MakeSprite("badge_highlight", 140, 140, p => p.Burst(Amber, Hex("FF7A00"), Ink, 4));
            var badgeValue = MakeSprite("badge_value", 140, 140, p => p.Burst(Lime, Hex("2FBF4A"), Ink, 4));
            var badgePopular = MakeSprite("badge_popular", 140, 140, p => p.Burst(Pink, Magenta, Ink, 4));
            var badgeGift = MakeSprite("badge_gift", 140, 140, p => p.Burst(Cyan, Hex("1E5BFF"), Ink, 4));

            var buyButton = MakeSprite("button_buy", 198, 66, p => p.RoundRect(0, 0, 198, 66, 33, Lime, Hex("4FB80F"), Ink, 4).Gloss(0.35f));
            var buyButtonGold = MakeSprite("button_buy_gold", 198, 66, p => p.RoundRect(0, 0, 198, 66, 33, Amber, Hex("E06A00"), Ink, 4).Gloss(0.35f));
            var grantBg = MakeSprite("grant_bg", 96, 96, p => p.RoundRect(0, 0, 96, 96, 24, new Color(1, 1, 1, 0.16f), new Color(1, 1, 1, 0.06f), new Color(1, 1, 1, 0.55f), 3), new Vector4(30, 30, 30, 30));

            var heroBg = MakeSprite("hero_bg", 512, 362, p => p.RoundRect(0, 0, 512, 362, 34, Hex("4B1C8F"), Hex("1A0B3D"), Cyan, 6).Stripes(new Color(1, 1, 1, 0.04f), 22));
            var heroFrame = MakeSprite("hero_frame", 8, 8, p => { });   // trong suot: bo dai khung tieu de cua prefab
            var heroPanel = MakeSprite("hero_panel", 483, 125, p => p.RoundRect(0, 0, 483, 125, 26, new Color(0.13f, 0.9f, 1f, 0.18f), new Color(0.13f, 0.9f, 1f, 0.05f), Cyan, 3));

            var bundleBg = MakeSprite("bundle_bg", 514, 274, p => p.RoundRect(0, 0, 514, 274, 30, Hex("11807F"), Hex("06323F"), Lime, 5).Grid(new Color(1, 1, 1, 0.05f), 26));
            var bundleFrame = MakeSprite("bundle_frame", 536, 274, p => p.RoundRect(4, 4, 528, 266, 32, Color.clear, Color.clear, new Color(0.71f, 1f, 0.23f, 0.85f), 4));
            var bundlePanel = MakeSprite("bundle_panel", 192, 123, p => p.RoundRect(0, 0, 192, 123, 22, new Color(0, 0, 0, 0.35f), new Color(0, 0, 0, 0.2f), Lime, 3));
            var petBg = MakeSprite("pet_bg", 514, 274, p => p.RoundRect(0, 0, 514, 274, 30, Hex("C2185B"), Hex("4A0E3F"), Amber, 5).Dots(new Color(1, 1, 1, 0.08f), 24));
            var petFrame = MakeSprite("pet_frame", 536, 274, p => p.RoundRect(4, 4, 528, 266, 32, Color.clear, Color.clear, new Color(1f, 0.71f, 0.12f, 0.9f), 4));

            var rewardedBg = MakeSprite("rewarded_bg", 540, 248, p => p.RoundRect(0, 0, 540, 248, 30, Hex("1F3FA8"), Hex("0B1440"), Amber, 5).Stripes(new Color(1, 0.8f, 0.2f, 0.06f), 18));
            var gridBg = MakeSprite("grid_bg", 321, 312, p => p.RoundRect(0, 0, 321, 312, 30, Hex("3A2A10"), Hex("1A1206"), Amber, 5).Planks(new Color(1, 0.75f, 0.3f, 0.08f), 52));
            var itemBg = MakeSprite("item_row_bg", 510, 126, p => p.RoundRect(0, 0, 510, 126, 28, Hex("6A1FB8"), Hex("26104F"), Magenta, 5));
            var itemFrame = MakeSprite("item_row_frame", 497, 111, p => p.RoundRect(3, 3, 491, 105, 24, Color.clear, Color.clear, new Color(1, 1, 1, 0.25f), 2));
            var discountBadge = MakeSprite("discount_badge", 123, 120, p => p.Burst(Hex("FF3B3B"), Hex("B00020"), Cream, 4));
            var itemPanel = MakeSprite("item_row_panel", 278, 87, p => p.RoundRect(0, 0, 278, 87, 20, new Color(0, 0, 0, 0.3f), new Color(0, 0, 0, 0.15f), Magenta, 2));

            AssetDatabase.Refresh();

            // --- game art reused as icons
            var coin = Load<Sprite>("Assets/Art/Coin.png");
            var star = Load<Sprite>("Assets/Art/Star.png");
            var box = Load<Sprite>("Assets/Art/Box.png");
            var boxOnTarget = Load<Sprite>("Assets/Art/BoxOnTarget.png");
            var player = Load<Sprite>("Assets/Art/Player.png");
            var target = Load<Sprite>("Assets/Art/Target.png");
            var ticket = Load<Sprite>("Assets/Art/Shop/icon_ticket.png");
            var undo = Load<Sprite>("Assets/Art/Shop/icon_undo.png");

            // --- fonts + material presets
            var bangers = Load<TMP_FontAsset>(FontDir + "Bangers SDF.asset");
            var anton = Load<TMP_FontAsset>(FontDir + "Anton SDF.asset");
            var oswald = Load<TMP_FontAsset>(FontDir + "Oswald Bold SDF.asset");
            var roboto = Load<TMP_FontAsset>(FontDir + "Roboto-Bold SDF.asset");
            var bangersNeon = FontPreset(bangers, "Bangers Neon", Magenta, 0.28f, new Color(0, 0, 0, 0.75f), 1.2f, -1.2f);
            var bangersInk = FontPreset(bangers, "Bangers Ink", Ink, 0.22f, Color.clear, 0, 0);
            var antonInk = FontPreset(anton, "Anton Ink", new Color(1, 1, 1, 0.9f), 0.18f, new Color(0, 0, 0, 0.35f), 0.6f, -0.6f);
            var oswaldGlow = FontPreset(oswald, "Oswald Glow", Ink, 0.25f, new Color(0.13f, 0.9f, 1f, 0.6f), 0, 0, 0.6f);

            // --- theme asset
            var theme = AssetDatabase.LoadAssetAtPath<ShopTheme>(ThemePath);
            if (theme == null)
            {
                theme = ScriptableObject.CreateInstance<ShopTheme>();
                AssetDatabase.CreateAsset(theme, ThemePath);
            }

            theme.TitleFont = bangers;
            theme.BodyFont = roboto;
            theme.PriceFont = anton;
            theme.GrantFont = oswald;

            theme.Primary = Swatch(shelfPrimary, Color.white, Cream, Style(bangers, bangersInk, Cream, 60, FontStyles.UpperCase));
            theme.Secondary = Swatch(shelfSecondary, Color.white, Cream, Style(bangers, bangersInk, Cream, 60, FontStyles.UpperCase | FontStyles.Italic));
            theme.Highlight = Swatch(badgeHighlight, Color.white, Ink, Style(bangers, null, Ink, 0, FontStyles.UpperCase));
            theme.Value = Swatch(badgeValue, Color.white, Ink, Style(bangers, null, Ink, 0, FontStyles.UpperCase));
            theme.Popular = Swatch(badgePopular, Color.white, Cream, Style(bangers, bangersInk, Cream, 0, FontStyles.UpperCase));
            theme.Gift = Swatch(badgeGift, Color.white, Ink, Style(bangers, null, Ink, 0, FontStyles.UpperCase));

            theme.ResourceIcons = new[]
            {
                new ResourceIcon { ResourceId = "coin", Icon = coin, DisplayName = "Coins" },
                new ResourceIcon { ResourceId = "ticket", Icon = ticket, DisplayName = "Tickets" },
                new ResourceIcon { ResourceId = "undo", Icon = undo, DisplayName = "Undo" },
            };
            theme.OfferIcons = new[]
            {
                new OfferIcon { IconKey = "remove_ads_hero", Icon = player },
                new OfferIcon { IconKey = "chest_adventure", Icon = box },
                new OfferIcon { IconKey = "chest_pet_lover", Icon = boxOnTarget },
                new OfferIcon { IconKey = "free_coins_ad", Icon = star },
                new OfferIcon { IconKey = "coin_pile_1", Icon = coin },
                new OfferIcon { IconKey = "coin_pile_2", Icon = coin },
                new OfferIcon { IconKey = "coin_pile_3", Icon = coin },
                new OfferIcon { IconKey = "coin_pile_4", Icon = coin },
            };

            theme.HeroCard = Card(heroBg, heroFrame, buyButtonGold, star, grantBg, heroPanel, coin,
                title: Style(bangers, bangersNeon, Cream, 0, FontStyles.UpperCase),
                body: Style(roboto, null, Hex("BDF6FF"), 0, FontStyles.Italic),
                bullets: Lines(Style(roboto, null, Hex("BDF6FF"), 0, FontStyles.Normal), 0f),
                badge: null,
                price: Style(anton, null, Ink, 0, FontStyles.UpperCase),
                grant: Style(oswald, oswaldGlow, Lime, 0, FontStyles.Bold),
                notice: Style(roboto, null, Amber, 0, FontStyles.Italic));
            theme.BundleCard = Card(bundleBg, bundleFrame, buyButton, box, grantBg, bundlePanel, coin,
                title: Style(bangers, bangersNeon, Cream, 0, FontStyles.UpperCase),
                body: Style(roboto, null, Cream, 0, FontStyles.Normal),
                bullets: Lines(Style(roboto, null, Cream, 0, FontStyles.Normal), 0f),
                badge: null,
                price: Style(anton, null, Ink, 0, FontStyles.UpperCase),
                grant: Style(oswald, oswaldGlow, Lime, 0, FontStyles.Bold),
                notice: Style(roboto, null, Amber, 0, FontStyles.Italic));
            theme.RewardedCard = Card(rewardedBg, null, buyButtonGold, star, grantBg, null, coin,
                title: Style(bangers, bangersNeon, Amber, 0, FontStyles.UpperCase),
                body: Style(roboto, null, Cream, 0, FontStyles.Normal),
                bullets: null,
                badge: null,
                price: Style(anton, null, Ink, 0, FontStyles.UpperCase),
                grant: Style(oswald, oswaldGlow, Amber, 0, FontStyles.Bold),
                notice: null);
            theme.GridTile = Card(gridBg, null, buyButton, coin, grantBg, null, coin,
                title: null,
                body: null,
                bullets: null,
                badge: null,
                price: Style(anton, null, Ink, 0, FontStyles.UpperCase),
                grant: Style(oswald, oswaldGlow, Amber, 0, FontStyles.Bold),
                notice: null);
            theme.ItemRow = Card(itemBg, itemFrame, buyButtonGold, coin, grantBg, itemPanel, coin,
                title: Style(bangers, bangersNeon, Cream, 0, FontStyles.UpperCase),
                body: null,
                bullets: null,
                badge: null,
                price: Style(anton, null, Ink, 0, FontStyles.UpperCase),
                grant: Style(oswald, oswaldGlow, Cyan, 0, FontStyles.Bold),
                notice: null);

            var petSkin = Card(petBg, petFrame, buyButtonGold, boxOnTarget, grantBg, bundlePanel, coin,
                title: Style(bangers, bangersNeon, Cream, 0, FontStyles.UpperCase),
                body: Style(roboto, null, Cream, 0, FontStyles.Normal),
                bullets: null,
                badge: null,
                price: Style(anton, null, Ink, 0, FontStyles.UpperCase),
                grant: Style(oswald, oswaldGlow, Amber, 0, FontStyles.Bold),
                notice: null);
            Discount(theme.BundleCard, discountBadge, "-40%", bangers);
            Discount(petSkin, discountBadge, "-25%", bangers);
            theme.OfferSkins = new[] { new OfferCardSkin { OfferId = "bundle_pet_lover", Skin = petSkin } };

            theme.Screen = new ScreenSkin
            {
                Background = screenBg,
                HeaderBackground = topBar,
                WalletBar = walletBar,
                CloseButton = close,
                SecondaryButton = retryLike,
                BackgroundColor = Ink,
                TextColor = Cream,
                ToastBackgroundColor = new Color(0.52f, 0.06f, 0.48f, 0.92f),
                TitleText = Style(bangers, bangersNeon, Cream, 0, FontStyles.UpperCase),
                WalletText = Style(oswald, null, Lime, 0, FontStyles.Bold),
                ButtonText = Style(bangers, null, Ink, 0, FontStyles.UpperCase),
                MessageText = Style(roboto, null, Cyan, 0, FontStyles.Italic),
                ToastText = Style(bangers, null, Cream, 0, FontStyles.UpperCase),
            };

            theme.Labels = new ShopLabels
            {
                WatchAd = "WATCH & GRAB",
                Restore = "RESTORE MY CRATES",
                MoreOffers = "+ MORE CRATES",
                ShowLess = "- FEWER CRATES",
                StoreUnavailable = "The warehouse is closed. Try again!",
                Retry = "KNOCK AGAIN",
                PendingPayment = "PENDING...",
                DailyCapReached = "Back tomorrow!",
                AdLoading = "LOADING...",
                CooldownFormat = "m\\:ss",
                PurchaseSuccess = "Delivered!",
                PurchaseFailed = "Oops, nothing was charged.",
                RestoreDone = "Crates restored!",
                RestoreNothing = "No crates to restore",
                RestoreFailed = "Restore failed, try again",
                Owned = "YOURS",
                DevPlaceholderPrice = "$?",
                DevPlaceholderNotice = "COMING SOON",
                Title = "CRATE SHOP",
                Loading = "Unpacking crates...",
                GrantCoinFormat = "{0:N0}",
                GrantItemFormat = "+{0}",
                WalletFormat = "{0:N0}",
            };

            theme.Layout = new ShopLayout
            {
                ShelfSpacing = 48f,
                CardSpacing = 44f,
                ShelfHeaderSize = new Vector2(900f, 130f),
                ShelfHeaderTextPadding = new Vector4(120f, 120f, 22f, 30f),
                GridSpacing = new Vector2(21f, 40f),
                WalletPillSize = new Vector2(300f, 96f),
                WalletIconSize = 80f,
                ToastSize = new Vector2(900f, 120f),
                ToastOffsetY = 1650f,      // toast len tren (vi da xuong day man)
                ToastSeconds = 3f,
            };
            theme.ScreenPrefab = BuildScreenVariant();
            theme.CardPrefabs = new[]
            {
                new CardPrefabOverride { CardTemplate = "grid_tile", Prefab = BuildGridTileVariant() },
            };

            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();
            Debug.Log("[ShopThemeShowcase] Theme written to " + ThemePath);
        }

        private static CardSkin Card(Sprite background, Sprite frame, Sprite buy, Sprite defaultIcon, Sprite grantBackground,
                                     Sprite grantPanel, Sprite coinGrantIcon, TextStyle title, TextStyle body, TextStyle bullets,
                                     TextStyle badge, TextStyle price, TextStyle grant, TextStyle notice)
        {
            return new CardSkin
            {
                Background = background, Frame = frame, BuyButton = buy, DefaultIcon = defaultIcon,
                GrantBackground = grantBackground, GrantPanel = grantPanel, CoinGrantIcon = coinGrantIcon,
                TitleText = title ?? new TextStyle(), BodyText = body ?? new TextStyle(), BulletsText = bullets ?? new TextStyle(),
                BadgeText = badge ?? new TextStyle(), PriceText = price ?? new TextStyle(), GrantText = grant ?? new TextStyle(),
                NoticeText = notice ?? new TextStyle(),
            };
        }

        private static TextStyle Lines(TextStyle style, float lineSpacing)
        {
            style.OverrideLineSpacing = true;
            style.LineSpacing = lineSpacing;
            return style;
        }

        private static void Discount(CardSkin skin, Sprite badge, string text, TMP_FontAsset font)
        {
            skin.DiscountBadge = badge;
            skin.DiscountText = text;
            skin.DiscountTextStyle = new TextStyle { Font = font, Color = Cream, OverrideFontStyle = true, FontStyle = FontStyles.Bold, FontSizeMin = 18f, FontSizeMax = 40f };
        }

        // ------------------------------------------------------------------ layout variants (Prefab Variant cua prefab SDK)

        private const string PrefabDir = "Assets/Prefabs/Shop";
        private const string SdkScreenPrefab = "Packages/com.ziba.growth/Runtime/Paywall.Unity/Screen/Resources/Growth/ShopScreen.prefab";
        private const string SdkGridTilePrefab = "Packages/com.ziba.growth/Runtime/Paywall.Unity/Prefabs/ShopCard_grid_tile.prefab";

        /// <summary>
        /// Man Shop: thanh tren thap lai, nut dong sang trai, vi xuong day man hinh. Prefab Variant khong cho doi cha cua object
        /// con, nen TopBar duoc keo phu ca vung an toan (no chi co RectTransform, khong chan bam) roi vi neo vao day TopBar.
        /// </summary>
        private static GameObject BuildScreenVariant()
        {
            return SaveVariant(SdkScreenPrefab, "PushTheBoxShopScreen", root =>
            {
                var topBar = Find(root, "SafeArea/Ready/TopBar");
                topBar.anchorMin = Vector2.zero;
                topBar.anchorMax = Vector2.one;
                topBar.offsetMin = Vector2.zero;
                topBar.offsetMax = Vector2.zero;

                var artwork = Find(root, "SafeArea/Ready/TopBar/Artwork");
                artwork.sizeDelta = new Vector2(artwork.sizeDelta.x, 495f);   // 195 tran len vung tai tho + 300

                var title = Find(root, "SafeArea/Ready/TopBar/Title");
                title.anchorMin = title.anchorMax = new Vector2(0.5f, 1f);
                title.pivot = new Vector2(0.5f, 1f);
                title.anchoredPosition = new Vector2(0f, -70f);
                title.sizeDelta = new Vector2(700f, 140f);

                var close = Find(root, "SafeArea/Ready/TopBar/Close");
                close.anchoredPosition = new Vector2(36f, close.anchoredPosition.y);

                var wallet = Find(root, "SafeArea/Ready/TopBar/Wallet");
                wallet.anchorMin = wallet.anchorMax = new Vector2(0.5f, 0f);
                wallet.pivot = new Vector2(0.5f, 0f);
                wallet.anchoredPosition = new Vector2(0f, 36f);
                wallet.sizeDelta = new Vector2(1000f, 96f);

                var scroll = Find(root, "SafeArea/Ready/ScrollView");
                scroll.offsetMax = new Vector2(scroll.offsetMax.x, -300f);
                scroll.offsetMin = new Vector2(scroll.offsetMin.x, 168f);
            });
        }

        /// <summary>The grid_tile: nut gia len dau, icon giua, so coin duoi, badge nho o mep phai.</summary>
        private static ShopCardView BuildGridTileVariant()
        {
            var go = SaveVariant(SdkGridTilePrefab, "PushTheBoxGridTile", root =>
            {
                Find(root, "Background").sizeDelta = new Vector2(321f, 360f);
                Find(root, "PriceButton").anchoredPosition = new Vector2(12f, -6f);
                Find(root, "OfferIcon").anchoredPosition = new Vector2(82.25f, -112f);
                Find(root, "Grants").anchoredPosition = new Vector2(72f, -285f);
                // Badge nam trong the (mep phai, duoi nut gia) — ra ngoai the thi bi o ben canh / nut gia ve de len.
                var badge = Find(root, "Badge");
                badge.anchoredPosition = new Vector2(228f, -108f);
                badge.localScale = new Vector3(0.72f, 0.72f, 1f);
            });
            return go.GetComponent<ShopCardView>();
        }

        private static GameObject SaveVariant(string sourcePath, string name, Action<Transform> edit)
        {
            Directory.CreateDirectory(PrefabDir);
            var source = Load<GameObject>(sourcePath);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            try
            {
                edit(instance.transform);
                var path = PrefabDir + "/" + name + ".prefab";
                return PrefabUtility.SaveAsPrefabAsset(instance, path);   // instance cua prefab -> luu thanh Prefab Variant
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        private static RectTransform Find(Transform root, string path)
        {
            var found = root.Find(path) as RectTransform;
            if (found == null) throw new InvalidOperationException("[ShopThemeShowcase] Prefab SDK khong con '" + path + "'.");
            return found;
        }

        private static StyleSwatch Swatch(Sprite background, Color tint, Color textColor, TextStyle text) =>
            new StyleSwatch { Background = background, Tint = tint, TextColor = textColor, Text = text };

        private static TextStyle Style(TMP_FontAsset font, Material material, Color color, float size, FontStyles style) =>
            new TextStyle
            {
                Font = font, FontMaterial = material, Color = color, FontSize = size,
                OverrideFontStyle = true, FontStyle = style,
            };

        private static Material FontPreset(TMP_FontAsset font, string name, Color outline, float outlineWidth, Color underlay,
                                           float underlayX, float underlayY, float underlaySoftness = 0f)
        {
            var path = ArtDir + "/Fonts/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(font.material);
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = font.material.shader;
                material.CopyPropertiesFromMaterial(font.material);
            }

            material.EnableKeyword(ShaderUtilities.Keyword_Outline);
            material.SetColor(ShaderUtilities.ID_OutlineColor, outline);
            material.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineWidth);
            if (underlay.a > 0f)
            {
                material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
                material.SetColor(ShaderUtilities.ID_UnderlayColor, underlay);
                material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, underlayX);
                material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, underlayY);
                material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, underlaySoftness);
            }
            else
            {
                material.DisableKeyword(ShaderUtilities.Keyword_Underlay);
            }
            EditorUtility.SetDirty(material);
            return material;
        }

        // ------------------------------------------------------------------ sprite drawing

        private static Sprite MakeSprite(string name, int width, int height, Action<Painter> draw, Vector4 border = default)
        {
            var painter = new Painter(width, height);
            draw(painter);
            var path = ArtDir + "/" + name + ".png";
            File.WriteAllBytes(path, painter.Encode());
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.spriteBorder = border;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static T Load<T>(string path) where T : UnityEngine.Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) throw new InvalidOperationException("[ShopThemeShowcase] Missing asset: " + path);
            return asset;
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var color);
            return color;
        }

        /// <summary>Tiny anti-aliased software rasterizer — enough for flat game-UI shapes.</summary>
        private sealed class Painter
        {
            private readonly int _w, _h;
            private readonly Color[] _px;

            public Painter(int w, int h)
            {
                _w = w; _h = h;
                _px = new Color[w * h];
            }

            public byte[] Encode()
            {
                var texture = new Texture2D(_w, _h, TextureFormat.RGBA32, false);
                texture.SetPixels(_px);
                texture.Apply();
                var bytes = texture.EncodeToPNG();
                UnityEngine.Object.DestroyImmediate(texture);
                return bytes;
            }

            private static Color Over(Color dst, Color src)
            {
                var a = src.a + dst.a * (1f - src.a);
                if (a <= 0f) return Color.clear;
                var rgb = ((Vector4)src * src.a + (Vector4)dst * dst.a * (1f - src.a)) / a;
                return new Color(rgb.x, rgb.y, rgb.z, a);
            }

            private void Blend(int x, int y, Color c)
            {
                if (x < 0 || y < 0 || x >= _w || y >= _h || c.a <= 0f) return;
                _px[y * _w + x] = Over(_px[y * _w + x], c);
            }

            // Overlays only keep the existing alpha (patterns drawn on top of a filled shape).
            private void Tint(int x, int y, Color c)
            {
                var i = y * _w + x;
                var dst = _px[i];
                if (dst.a <= 0f) return;
                var mixed = Color.Lerp(dst, new Color(c.r, c.g, c.b, dst.a), c.a);
                mixed.a = dst.a;
                _px[i] = mixed;
            }

            private static float RoundRectDistance(float px, float py, float cx, float cy, float hw, float hh, float r)
            {
                var qx = Mathf.Abs(px - cx) - hw + r;
                var qy = Mathf.Abs(py - cy) - hh + r;
                var outside = new Vector2(Mathf.Max(qx, 0f), Mathf.Max(qy, 0f)).magnitude;
                return outside + Mathf.Min(Mathf.Max(qx, qy), 0f) - r;
            }

            public Painter RoundRect(float x0, float y0, float w, float h, float r, Color top, Color bottom, Color border, float borderWidth)
            {
                var cx = x0 + w / 2f; var cy = y0 + h / 2f;
                for (var y = 0; y < _h; y++)
                for (var x = 0; x < _w; x++)
                {
                    var d = RoundRectDistance(x + 0.5f, y + 0.5f, cx, cy, w / 2f, h / 2f, r);
                    if (d > 1f) continue;
                    var coverage = Mathf.Clamp01(0.5f - d);
                    var t = Mathf.Clamp01((y - y0) / Mathf.Max(1f, h));
                    var fill = Color.Lerp(bottom, top, t);
                    var edge = Mathf.Clamp01(d + borderWidth + 0.5f);
                    var c = Color.Lerp(fill, border, border.a > 0f ? edge : 0f);
                    if (border.a > 0f && edge > 0f) c.a = Mathf.Lerp(fill.a, border.a, edge);
                    c.a *= coverage;
                    Blend(x, y, c);
                }
                return this;
            }

            public Painter Gradient(Color top, Color bottom)
            {
                for (var y = 0; y < _h; y++)
                {
                    var c = Color.Lerp(bottom, top, y / (float)(_h - 1));
                    for (var x = 0; x < _w; x++) _px[y * _w + x] = c;
                }
                return this;
            }

            public Painter Grid(Color line, int step)
            {
                for (var y = 0; y < _h; y++)
                for (var x = 0; x < _w; x++)
                    if (x % step == 0 || y % step == 0) Tint(x, y, line);
                return this;
            }

            public Painter Planks(Color line, int step)
            {
                for (var y = 0; y < _h; y++)
                for (var x = 0; x < _w; x++)
                    if (y % step < 2) Tint(x, y, line);
                return this;
            }

            public Painter Stripes(Color stripe, int step)
            {
                for (var y = 0; y < _h; y++)
                for (var x = 0; x < _w; x++)
                    if ((x + y) % (step * 2) < step) Tint(x, y, stripe);
                return this;
            }

            public Painter Dots(Color dot, int step)
            {
                for (var y = 0; y < _h; y++)
                for (var x = 0; x < _w; x++)
                {
                    var dx = x % step - step / 2f; var dy = y % step - step / 2f;
                    if (dx * dx + dy * dy < step * step / 16f) Tint(x, y, dot);
                }
                return this;
            }

            public Painter Gloss(float strength)
            {
                for (var y = _h / 2; y < _h; y++)
                for (var x = 0; x < _w; x++)
                    Tint(x, y, new Color(1, 1, 1, strength * (y - _h / 2f) / _h));
                return this;
            }

            public Painter Vignette(float strength)
            {
                for (var y = 0; y < _h; y++)
                for (var x = 0; x < _w; x++)
                {
                    var dx = (x / (float)_w - 0.5f) * 2f; var dy = (y / (float)_h - 0.5f) * 2f;
                    Tint(x, y, new Color(0, 0, 0, strength * Mathf.Clamp01((dx * dx + dy * dy) - 0.35f)));
                }
                return this;
            }

            public Painter Circle(Color top, Color bottom, Color border, float borderWidth)
            {
                var r = Mathf.Min(_w, _h) / 2f - 1f;
                return RoundRect(_w / 2f - r, _h / 2f - r, r * 2f, r * 2f, r, top, bottom, border, borderWidth);
            }

            public Painter Cross(Color color, float thickness, float halfLength)
            {
                var cx = _w / 2f; var cy = _h / 2f;
                for (var y = 0; y < _h; y++)
                for (var x = 0; x < _w; x++)
                {
                    var px = x + 0.5f - cx; var py = y + 0.5f - cy;
                    var a = Mathf.Abs(px - py) / 1.4142f; var b = Mathf.Abs(px + py) / 1.4142f;
                    var along1 = Mathf.Abs(px + py) / 1.4142f; var along2 = Mathf.Abs(px - py) / 1.4142f;
                    var d = Mathf.Min(along1 <= halfLength ? a : 999f, along2 <= halfLength ? b : 999f) - thickness / 2f;
                    if (d < 1f) Blend(x, y, new Color(color.r, color.g, color.b, color.a * Mathf.Clamp01(0.5f - d)));
                }
                return this;
            }

            public Painter Ribbon(Color left, Color right, Color border, float borderWidth)
            {
                // Banner with V-notched ends.
                var notch = _h * 0.45f;
                for (var y = 0; y < _h; y++)
                for (var x = 0; x < _w; x++)
                {
                    var fy = Mathf.Abs((y + 0.5f) - _h / 2f) / (_h / 2f);
                    var inset = notch * (1f - fy);
                    var dLeft = inset - (x + 0.5f);
                    var dRight = (x + 0.5f) - (_w - inset);
                    var dTop = Mathf.Abs((y + 0.5f) - _h / 2f) - (_h / 2f - 1f);
                    var d = Mathf.Max(Mathf.Max(dLeft, dRight), dTop);
                    if (d > 1f) continue;
                    var fill = Color.Lerp(left, right, x / (float)_w);
                    var edge = Mathf.Clamp01(d + borderWidth + 0.5f);
                    var c = Color.Lerp(fill, border, edge);
                    c.a *= Mathf.Clamp01(0.5f - d);
                    Blend(x, y, c);
                }
                return this;
            }

            public Painter Burst(Color inner, Color outer, Color border, float borderWidth)
            {
                // 12-point starburst seal.
                var cx = _w / 2f; var cy = _h / 2f; var rMax = Mathf.Min(_w, _h) / 2f - 2f;
                for (var y = 0; y < _h; y++)
                for (var x = 0; x < _w; x++)
                {
                    var dx = x + 0.5f - cx; var dy = y + 0.5f - cy;
                    var dist = Mathf.Sqrt(dx * dx + dy * dy);
                    var angle = Mathf.Atan2(dy, dx);
                    var radius = rMax * (0.86f + 0.14f * Mathf.Cos(angle * 12f));
                    var d = dist - radius;
                    if (d > 1f) continue;
                    var fill = Color.Lerp(inner, outer, dist / rMax);
                    var edge = Mathf.Clamp01(d + borderWidth + 0.5f);
                    var c = Color.Lerp(fill, border, edge);
                    c.a *= Mathf.Clamp01(0.5f - d);
                    Blend(x, y, c);
                }
                return this;
            }
        }

        // ------------------------------------------------------------------ preview

        /// <summary>
        /// Renders the real SDK ShopScreen (default theme vs Neon Warehouse) into PNGs. Uses reflection into SDK internals
        /// only for this editor preview — the game build never does this.
        /// </summary>
        public static void Preview(string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            var themed = AssetDatabase.LoadAssetAtPath<ShopTheme>(ThemePath);
            var empty = ScriptableObject.CreateInstance<ShopTheme>();
            var json = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/paywall_shop_config.json").text;

            var sheets = new List<Texture2D>();
            foreach (var (name, theme) in new[] { ("default", empty), ("neon_warehouse", themed) })
            {
                var pages = RenderShop(theme, json, 3);
                for (var i = 0; i < pages.Count; i++)
                    File.WriteAllBytes(Path.Combine(outputDir, name + "_page" + (i + 1) + ".png"), pages[i].EncodeToPNG());
                sheets.Add(Strip(pages));
            }
            File.WriteAllBytes(Path.Combine(outputDir, "compare.png"), Stack(sheets).EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(empty);
            Debug.Log("[ShopThemeShowcase] Preview written to " + outputDir);
        }

        private static List<Texture2D> RenderShop(ShopTheme theme, string json, int pages)
        {
            const int width = 1080, height = 2340;
            // ShopScreen is internal to the SDK — reach it by name (editor preview only).
            var screenType = typeof(ShopCardView).Assembly.GetType("Growth.ShopScreen", true);
            var prefab = theme.ScreenPrefab != null ? theme.ScreenPrefab.GetComponent(screenType)
                                                    : (Component)Resources.Load("Growth/ShopScreen", screenType);
            var screen = (Component)UnityEngine.Object.Instantiate(prefab);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            screenType.GetField("_theme", flags).SetValue(screen, theme);
            screenType.GetField("_getBalance", flags).SetValue(screen, (Func<string, long>)(id => id == "coin" ? 12450 : id == "ticket" ? 7 : 3));

            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var cameraGo = new GameObject("PreviewCamera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            camera.targetTexture = rt;
            var canvas = screen.GetComponentInChildren<Canvas>(true);
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 10f;

            screenType.GetMethod("ApplyTheme", flags).Invoke(screen, null);
            screenType.GetMethod("BuildReadyView", flags).Invoke(screen, new[] { BuildModel(json) });

            var scroll = screen.GetComponentInChildren<ScrollRect>(true);
            var result = new List<Texture2D>();
            for (var i = 0; i < pages; i++)
            {
                for (var pass = 0; pass < 3; pass++)
                {
                    Canvas.ForceUpdateCanvases();
                    foreach (var layout in screen.GetComponentsInChildren<RectTransform>(true))
                        LayoutRebuilder.MarkLayoutForRebuild(layout);
                    Canvas.ForceUpdateCanvases();
                }
                if (scroll != null) scroll.verticalNormalizedPosition = pages == 1 ? 1f : 1f - i / (float)(pages - 1);
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = rt;
                var page = new Texture2D(width, height, TextureFormat.RGBA32, false);
                page.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                page.Apply();
                RenderTexture.active = null;
                result.Add(page);
            }

            UnityEngine.Object.DestroyImmediate(screen.gameObject);
            UnityEngine.Object.DestroyImmediate(cameraGo);
            rt.Release();
            return result;
        }

        private static object BuildModel(string json)
        {
            var core = AppDomain.CurrentDomain.GetAssemblies();
            Type Find(string name) => core.Select(a => a.GetType(name)).First(t => t != null);
            var shopT = Find("Ziba.Growth.Paywall.ShopViewModel");
            var shelfT = Find("Ziba.Growth.Paywall.ShelfViewModel");
            var cardT = Find("Ziba.Growth.Paywall.CardViewModel");
            var grantT = Find("Ziba.Growth.Paywall.GrantDisplay");
            var stateT = Find("Ziba.Growth.Paywall.ShopLoadState");
            var freeT = Find("Ziba.Growth.Paywall.FreeCoinsState");

            object NewList(Type t, IEnumerable<object> items)
            {
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(t));
                foreach (var item in items) list.Add(item);
                return list;
            }
            void Set(object o, string field, object value) => o.GetType().GetField(field).SetValue(o, value);

            var root = JObject.Parse(json);
            var prices = new Dictionary<string, string>
            {
                ["no_ads"] = "$4.99", ["adventure_bundle"] = "$9.99", ["pet_lover_pack"] = "$4.99", ["coin_500"] = "$0.99",
                ["coin_1200"] = "$1.99", ["coin_2500"] = "$3.99", ["coin_6000"] = "$7.99", ["coin_12000"] = "$14.99",
            };
            var shelves = new List<object>();
            foreach (var shelf in root["shelves"])
            {
                var cards = new List<object>();
                var slot = 0;
                foreach (var c in shelf["cards"])
                {
                    var card = Activator.CreateInstance(cardT);
                    Set(card, "OfferId", (string)c["offer_id"]);
                    Set(card, "CardTemplate", (string)c["card_template"]);
                    Set(card, "IconKey", (string)c["icon_key"]);
                    Set(card, "SlotIndex", slot++);
                    Set(card, "Title", (string)c["title"]);
                    Set(card, "Body", (string)c["body"]);
                    Set(card, "Bullets", c["bullets"]?.Select(b => (string)b).ToList());
                    Set(card, "BadgeText", (string)c["badge"]?["text"]);
                    Set(card, "BadgeStyle", (string)c["badge"]?["style"]);
                    var productId = (string)c["product_id"];
                    Set(card, "PriceText", productId != null && prices.TryGetValue(productId, out var price) ? price : null);
                    Set(card, "Grants", NewList(grantT, (c["grants"] ?? new JArray()).Select(g =>
                    {
                        var grant = Activator.CreateInstance(grantT);
                        Set(grant, "ResourceId", (string)g["resource_id"]);
                        Set(grant, "Amount", (long)g["amount"]);
                        return grant;
                    })));
                    Set(card, "Purchasable", true);
                    Set(card, "FreeCoins", Enum.Parse(freeT, "Ready"));
                    Set(card, "FreeCoinsRemainingToday", 20);
                    cards.Add(card);
                }
                var vm = Activator.CreateInstance(shelfT);
                Set(vm, "ShelfId", (string)shelf["shelf_id"]);
                Set(vm, "HeaderText", (string)shelf["header"]?["text"]);
                Set(vm, "HeaderStyle", (string)shelf["header"]?["style"]);
                Set(vm, "CollapsedByDefault", (bool?)shelf["collapsed_by_default"] ?? false);
                Set(vm, "Cards", NewList(cardT, cards));
                shelves.Add(vm);
            }
            var shop = Activator.CreateInstance(shopT);
            Set(shop, "ShopVisitId", "preview");
            Set(shop, "State", Enum.Parse(stateT, "Ready"));
            Set(shop, "WalletDisplay", root["wallet_display"].Select(w => (string)w).ToList());
            Set(shop, "Shelves", NewList(shelfT, shelves));
            return shop;
        }

        private static Texture2D Strip(List<Texture2D> pages)
        {
            // Pages side by side at half resolution.
            var w = pages[0].width / 2; var h = pages[0].height / 2;
            var sheet = new Texture2D(w * pages.Count + 20 * (pages.Count - 1), h, TextureFormat.RGBA32, false);
            var fill = new Color[sheet.width * sheet.height];
            for (var i = 0; i < fill.Length; i++) fill[i] = Color.white;
            sheet.SetPixels(fill);
            for (var p = 0; p < pages.Count; p++)
                for (var y = 0; y < h; y++)
                for (var x = 0; x < w; x++)
                    sheet.SetPixel(p * (w + 20) + x, y, pages[p].GetPixel(x * 2, y * 2));
            sheet.Apply();
            return sheet;
        }

        private static Texture2D Stack(List<Texture2D> sheets)
        {
            var w = sheets.Max(s => s.width); var h = sheets.Sum(s => s.height) + 40 * (sheets.Count - 1);
            var result = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var fill = new Color[w * h];
            for (var i = 0; i < fill.Length; i++) fill[i] = Color.white;
            result.SetPixels(fill);
            var y0 = h;
            foreach (var sheet in sheets)
            {
                y0 -= sheet.height;
                result.SetPixels(0, y0, sheet.width, sheet.height, sheet.GetPixels());
                y0 -= 40;
            }
            result.Apply();
            return result;
        }
    }
}
