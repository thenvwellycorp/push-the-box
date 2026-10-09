using System;
using System.Reflection;
using UnityEngine;

namespace PushTheBox.GrowthIntegration
{
    /// <summary>
    /// Game-side workaround for a Growth SDK bug (v0.2.1 .. v0.3.0, AdMobAdPresenter + FullscreenShowWatchdog).
    ///
    /// On Android a fullscreen ad runs in its own Activity, so Unity is paused and AdMob's "Opened" event stays queued in
    /// MobileAdsEventExecutor until Unity resumes. The SDK watchdog counts real time, so on the first frame after the
    /// player closes the ad it sees "not opened after 7s" and completes the show as Failed: rewarded ads never grant
    /// their reward (and interstitials report Failed instead of Shown).
    ///
    /// Losing focus / pausing right after a show was requested means the ad took over the screen, so we mark the
    /// watchdog as opened. The SDK keeps its 1s grace after focus returns, which lets Opened / reward / Closed arrive.
    ///
    /// Remove this component once the SDK ships the fix (FullscreenShowWatchdog.MarkFocusLost marking the show opened).
    /// </summary>
    public class AdShowWatchdogPauseFix : MonoBehaviour
    {
        private const string PresenterObjectName = "[Growth] AdMob";
        private const string PresenterTypeName = "Ziba.Growth.Ads.AdMob.AdMobAdPresenter";
        private const string WatchdogFieldName = "_watchdog";
        private const string MarkOpenedMethodName = "MarkOpened";

        private object _watchdog;
        private MethodInfo _markOpened;
        private bool _unsupported;

        private void OnApplicationPause(bool paused)
        {
            if (paused) MarkShowOpened();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus) MarkShowOpened();
        }

        private void MarkShowOpened()
        {
            if (!TryResolveWatchdog()) return;

            try
            {
                // MarkOpened is a no-op when no fullscreen show is in progress.
                _markOpened.Invoke(_watchdog, null);
            }
            catch (Exception e)
            {
                _unsupported = true;
                Debug.LogWarning($"[AdShowWatchdogPauseFix] MarkOpened failed, workaround disabled: {e.Message}");
            }
        }

        private bool TryResolveWatchdog()
        {
            if (_unsupported) return false;
            if (_watchdog != null) return true;

            // The presenter is created by GrowthSDK.Init; it may not exist yet (or ever, e.g. SDK init failed).
            var host = GameObject.Find(PresenterObjectName);
            if (host == null) return false;

            Component presenter = null;
            foreach (var component in host.GetComponents<MonoBehaviour>())
            {
                if (component != null && component.GetType().FullName == PresenterTypeName)
                {
                    presenter = component;
                    break;
                }
            }
            if (presenter == null) return false;

            var field = presenter.GetType().GetField(WatchdogFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            var watchdog = field?.GetValue(presenter);
            var markOpened = watchdog?.GetType().GetMethod(MarkOpenedMethodName, BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);
            if (markOpened == null)
            {
                _unsupported = true;
                Debug.LogWarning("[AdShowWatchdogPauseFix] Growth SDK internals changed (AdMobAdPresenter._watchdog.MarkOpened not found); workaround disabled.");
                return false;
            }

            _watchdog = watchdog;
            _markOpened = markOpened;
            return true;
        }
    }
}
