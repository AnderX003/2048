using System;
using UnityEngine;
using GoogleMobileAds.Api;

namespace _Scripts
{
    public class AdsController : MonoBehaviour
    {
        #region Google AdMob

        [SerializeField] private string adBannerId;
        [SerializeField] private string adGameOverId;
        private BannerView bannerView;
        private InterstitialAd interstitial;

        public void Initialize()
        {
            MobileAds.Initialize(initStatus => { });
            RequestBanner();
            RequestGameOverAd();
        }

        private void RequestBanner()
        {
            bannerView = new BannerView(adBannerId, AdSize.Banner, AdPosition.Bottom);
            AdRequest request = new AdRequest.Builder().Build();
            bannerView.LoadAd(request);
        }


        private void RequestGameOverAd()
        {
            interstitial = new InterstitialAd(adGameOverId);
            interstitial.OnAdClosed += HandleOnAdClosed;
            AdRequest request = new AdRequest.Builder().Build();
            interstitial.LoadAd(request);
        }

        public void ShowGameOverAd()
        {
            if (interstitial.IsLoaded())
            {
                interstitial.Show();
            }
        }

        private void HandleOnAdClosed(object sender, EventArgs args)
        {
            DestroyInterstitialAd();
            RequestGameOverAd();
        }

        private void DestroyInterstitialAd()
        {
            interstitial?.Destroy();
        }

        #endregion
    }
}