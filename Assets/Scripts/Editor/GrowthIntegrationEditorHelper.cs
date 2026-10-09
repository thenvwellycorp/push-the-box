using UnityEditor;
using UnityEngine;

namespace PushTheBox.EditorTools
{
    [InitializeOnLoad]
    public static class GrowthIntegrationEditorHelper
    {
        static GrowthIntegrationEditorHelper()
        {
            EditorApplication.delayCall += OnEditorReady;
        }

        private static void OnEditorReady()
        {
            Debug.Log("[GrowthIntegrationEditorHelper] Growth SDK integrated in Push The Box.");
        }

        [MenuItem("Growth SDK/Validate Integration")]
        public static void ValidateIntegration()
        {
            var shopConfig = Resources.Load<TextAsset>("paywall_shop_config");
            var adsConfig = Resources.Load<TextAsset>("ads_config");
            var adUnits = Resources.Load<Growth.GrowthAdUnits>("GrowthAdUnits");
            var theme = Resources.Load<Growth.ShopTheme>("PushTheBoxShopTheme");

            Debug.Log($"[Growth SDK Validation]\n" +
                      $"- paywall_shop_config.json: {(shopConfig != null ? "FOUND (" + shopConfig.text.Length + " bytes)" : "MISSING")}\n" +
                      $"- ads_config.json: {(adsConfig != null ? "FOUND (" + adsConfig.text.Length + " bytes)" : "MISSING")}\n" +
                      $"- GrowthAdUnits: {(adUnits != null ? "FOUND" : "MISSING")}\n" +
                      $"- ShopTheme: {(theme != null ? "FOUND" : "MISSING")}\n" +
                      $"- Firebase: {(System.IO.Directory.Exists("Assets/Firebase") ? "INSTALLED" : "MISSING")}\n" +
                      $"- GoogleMobileAds: {(System.IO.Directory.Exists("Assets/GoogleMobileAds") ? "INSTALLED" : "MISSING")}\n" +
                      $"- EDM4U: {(System.IO.Directory.Exists("Assets/ExternalDependencyManager") ? "INSTALLED" : "MISSING")}\n" +
                      $"- Android Plugins: {(System.IO.Directory.Exists("Assets/Plugins/Android") ? "INSTALLED" : "MISSING")}");
        }
    }
}
