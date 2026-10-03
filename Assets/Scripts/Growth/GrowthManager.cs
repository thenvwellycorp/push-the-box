using System;
using System.Threading.Tasks;
using UnityEngine;
using Growth;
using PushTheBox.Core;
using PushTheBox.Level;
using PushTheBox.Save;

namespace PushTheBox.GrowthIntegration
{
    /// <summary>
    /// Bridges Push The Box game currency (Coins) with the Growth SDK economy and wallet ledger.
    /// </summary>
    public class PushTheBoxInventoryAdapter : IInventoryAdapter
    {
        public long GetBalance(string resourceId)
        {
            if (resourceId == "coin")
            {
                if (SaveManager.Instance != null)
                    return SaveManager.Instance.Coins;
                return PlayerPrefs.GetInt("PushTheBox_Coins_Fallback", 0);
            }
            return 0;
        }

        public void Add(string resourceId, long amount)
        {
            if (resourceId == "coin")
            {
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.AddCoins((int)amount);
                }
                else
                {
                    int curr = PlayerPrefs.GetInt("PushTheBox_Coins_Fallback", 0);
                    PlayerPrefs.SetInt("PushTheBox_Coins_Fallback", curr + (int)amount);
                    PlayerPrefs.Save();
                }
            }
        }

        public bool TryRemove(string resourceId, long amount)
        {
            if (resourceId == "coin")
            {
                if (SaveManager.Instance != null)
                {
                    return SaveManager.Instance.SpendCoins((int)amount);
                }
                else
                {
                    int curr = PlayerPrefs.GetInt("PushTheBox_Coins_Fallback", 0);
                    if (curr >= amount)
                    {
                        PlayerPrefs.SetInt("PushTheBox_Coins_Fallback", curr - (int)amount);
                        PlayerPrefs.Save();
                        return true;
                    }
                    return false;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Offer placement ids for in-game offers drawn outside the Shop. Must match GameCatalog.OfferPlacements
    /// and "offer_placements" in paywall_shop_config.json — a typo silently never shows anything.
    /// </summary>
    public static class OfferPlacementIds
    {
        public const string WinScreen = "win_screen";
        public const string HomeRemoveAds = "home_remove_ads";
    }

    /// <summary>
    /// Central manager for Growth SDK integration in Push The Box.
    /// Configures GameCatalog, Ads (AdMob + UMP), IAP, Economy ledger, and Shop.
    /// Handles telemetry hooks for Level progression and Ad placements.
    /// </summary>
    public class GrowthManager : MonoBehaviour
    {
        public static GrowthManager Instance { get; private set; }

        private bool _isInitialized = false;

        public bool IsInitialized => _isInitialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoInitialize()
        {
            if (Instance == null)
            {
                GameObject host = new GameObject("GrowthManager");
                DontDestroyOnLoad(host);
                Instance = host.AddComponent<GrowthManager>();
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitSDK();
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            SubscribeToGameEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromGameEvents();
        }

        public void InitSDK()
        {
            if (_isInitialized) return;

            try
            {
                var shopJsonAsset = Resources.Load<TextAsset>("paywall_shop_config");
                string shopJson = shopJsonAsset != null ? shopJsonAsset.text : string.Empty;

                var adsJsonAsset = Resources.Load<TextAsset>("ads_config");
                string adsJson = adsJsonAsset != null ? adsJsonAsset.text : string.Empty;

                var adUnits = Resources.Load<GrowthAdUnits>("GrowthAdUnits");
                var shopTheme = Resources.Load<ShopTheme>("ArrowRushShopTheme");

                var catalog = new GameCatalog
                {
                    GameId = "push_the_box",
                    ConceptId = "push_the_box_v1",
                    ConceptShort = "ptbox",
                    Resources = new[] { "coin" },
                    Entitlements = new[] { "no_banner", "no_interstitial" },
                    CardTemplates = new[] { "hero_card", "bundle_card", "rewarded_card", "grid_tile", "item_row" },
                    EmbeddedShopConfigJson = shopJson,
                    EmbeddedAdsConfigJson = adsJson,
                    LevelVersionsJson = "{\"levels\":{}}",
                    TutorialId = "tutorial_v1",
                    OfferPlacements = new[] { OfferPlacementIds.WinScreen, OfferPlacementIds.HomeRemoveAds }
                };

                var config = new GrowthConfig
                {
                    Catalog = catalog,
                    Inventory = new PushTheBoxInventoryAdapter(),
                    Ads = new AdsSetup
                    {
                        Units = adUnits != null ? adUnits : ScriptableObject.CreateInstance<GrowthAdUnits>(),
                        ConsentDebug = new ConsentDebug()
                    },
                    Purchases = new PurchaseSetup(),
                    ShopTheme = shopTheme
                };

                GrowthSDK.Init(config);
                _isInitialized = true;
                Debug.Log("[GrowthManager] Growth SDK initialized successfully!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GrowthManager] Failed to initialize Growth SDK: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void SubscribeToGameEvents()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelLoaded += HandleLevelLoaded;
                LevelManager.Instance.OnLevelRestarted += HandleLevelRestarted;
                LevelManager.Instance.OnLevelCompleted += HandleLevelCompleted;
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnGameStateChanged += HandleGameStateChanged;

                // The initial MainMenu state never raises OnGameStateChanged, so show its banner explicitly.
                ApplyBannerForState(GameStateManager.Instance.CurrentState);
            }
        }

        private void UnsubscribeFromGameEvents()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelLoaded -= HandleLevelLoaded;
                LevelManager.Instance.OnLevelRestarted -= HandleLevelRestarted;
                LevelManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
            }

            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        private void HandleLevelLoaded(LevelData data)
        {
            if (!_isInitialized) return;
            try
            {
                int levelNum = data != null ? data.levelId : 1;
                Growth.Level.Begin(levelNum, EntryPoint.Map);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GrowthManager] Level.Begin exception: {e.Message}");
            }
        }

        private void HandleLevelRestarted()
        {
            if (!_isInitialized) return;
            try
            {
                int moves = LevelManager.Instance != null ? LevelManager.Instance.MoveCount : 0;
                Growth.Level.Restart(moves, 0);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GrowthManager] Level.Restart exception: {e.Message}");
            }
        }

        private void HandleLevelCompleted(LevelCompletionData data)
        {
            if (!_isInitialized) return;
            try
            {
                Growth.Level.Complete(data.moves, data.stars);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GrowthManager] Level.Complete exception: {e.Message}");
            }
        }

        private void ApplyBannerForState(GameState state)
        {
            if (!_isInitialized) return;

            try
            {
                switch (state)
                {
                    case GameState.MainMenu:
                    case GameState.LevelSelect:
                        Growth.Ads.SetBannerScreen(Placement.BannerMap);
                        break;
                    case GameState.Playing:
                        Growth.Ads.SetBannerScreen(Placement.BannerGameplay);
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GrowthManager] SetBannerScreen exception: {e.Message}");
            }
        }

        private void HandleGameStateChanged(GameState oldState, GameState newState)
        {
            if (!_isInitialized) return;

            try
            {
                switch (newState)
                {
                    case GameState.MainMenu:
                    case GameState.LevelSelect:
                    case GameState.Playing:
                        ApplyBannerForState(newState);
                        break;
                    case GameState.GameOver:
                        int moves = LevelManager.Instance != null ? LevelManager.Instance.MoveCount : 0;
                        Growth.Level.Fail(LevelFailReason.NoValidMove, moves, 0);
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[GrowthManager] HandleGameStateChanged exception: {e.Message}");
            }
        }

        #region Public Ads & Monetization API

        /// <summary>
        /// Attempts to display an interstitial ad at the specified placement.
        /// Automatically respects caps, cooldowns, and Remove Ads entitlements.
        /// </summary>
        public void ShowInterstitial(Placement placement, Action onComplete = null)
        {
            if (!_isInitialized)
            {
                onComplete?.Invoke();
                return;
            }

            Growth.Ads.TryShowInterstitial(placement, result =>
            {
                Debug.Log($"[GrowthManager] Interstitial {placement} result: {result}");
                onComplete?.Invoke();
            });
        }

        // SDK requires resourceId for RewardType.Coin; this game has a single currency.
        private static string ResolveRewardResourceId(RewardType rewardType)
        {
            return rewardType == RewardType.Coin ? "coin" : null;
        }

        /// <summary>
        /// Checks if a rewarded offer is available for the given placement and reward.
        /// </summary>
        public bool IsRewardedAvailable(Placement placement, RewardType rewardType, int amount)
        {
            if (!_isInitialized) return false;
            try
            {
                var offer = Growth.Ads.EvaluateRewarded(placement, rewardType, amount, ResolveRewardResourceId(rewardType));
                return offer != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Evaluates and shows a rewarded ad. Invokes onRewardGranted only if the ad is fully watched.
        /// </summary>
        public void ShowRewarded(Placement placement, RewardType rewardType, int amount, Action onRewardGranted, Action onDone = null)
        {
            if (!_isInitialized)
            {
                onRewardGranted?.Invoke();
                onDone?.Invoke();
                return;
            }

            try
            {
                var offer = Growth.Ads.EvaluateRewarded(placement, rewardType, amount, ResolveRewardResourceId(rewardType));
                if (offer != null)
                {
                    offer.ReportShown();
                    offer.Watch(() =>
                    {
                        onRewardGranted?.Invoke();
                    }, result =>
                    {
                        Debug.Log($"[GrowthManager] Rewarded {placement} finished: {result}");
                        onDone?.Invoke();
                    });
                }
                else
                {
                    Debug.LogWarning($"[GrowthManager] Rewarded ad not available for {placement}");
                    onDone?.Invoke();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[GrowthManager] Error showing rewarded ad: {e.Message}");
                onDone?.Invoke();
            }
        }

        /// <summary>
        /// Opens the standardized SDK Paywall / Shop screen.
        /// </summary>
        public async Task<ShopResult> OpenShopAsync()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[GrowthManager] SDK not initialized yet.");
                return new ShopResult();
            }

            return await Growth.Shop.OpenAsync(ShopEntry.Shop);
        }

        /// <summary>
        /// Checks if the player has purchased Remove Ads (no_ads / no_interstitial).
        /// </summary>
        public bool HasNoAds()
        {
            if (!_isInitialized) return false;
            return Growth.Purchase.IsOwned("no_ads") || Growth.Shop.HasEntitlement("no_interstitial");
        }

        /// <summary>
        /// Returns the offer to draw at an in-game placement right now, or null when nothing should be drawn.
        /// Call it just before drawing (never cache it), then call ReportShown() once it is on screen.
        /// </summary>
        public OfferView GetOffer(string placementId)
        {
            if (!_isInitialized) return null;
            return Growth.Offers.Get(placementId);
        }

        /// <summary>
        /// Restores non-consumable purchases (Remove Ads). onDone is always invoked exactly once on the main thread.
        /// </summary>
        public void RestorePurchases(Action<RestoreResult> onDone)
        {
            if (!_isInitialized)
            {
                onDone?.Invoke(new RestoreResult { Failed = true, Reason = RestoreResult.NotConfigured });
                return;
            }

            Growth.Purchase.Restore(result =>
            {
                Debug.Log($"[GrowthManager] Restore finished: restored={result.RestoredCount} failed={result.Failed} reason={result.Reason}");
                onDone?.Invoke(result);
            });
        }

        /// <summary>
        /// Shows the Google UMP Privacy Options form (for GDPR / EEA players to change consent).
        /// </summary>
        public void ShowPrivacyOptions(Action onDone = null)
        {
            if (!_isInitialized)
            {
                onDone?.Invoke();
                return;
            }

            Growth.Privacy.ShowOptions(onDone ?? (() => { }));
        }

        #endregion
    }
}
