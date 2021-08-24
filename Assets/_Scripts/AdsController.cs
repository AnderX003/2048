using UnityEngine;
using GoogleMobileAds.Api;

namespace _Scripts
{
    public class AdsController : MonoBehaviour
    {
        #region Google AdMob

        [SerializeField] private string adBannerId;
        private BannerView bannerView;

        public void Initialize()
        {
            MobileAds.Initialize(initStatus => { });
            RequestBanner();
        }

        private void RequestBanner()
        {
            bannerView = new BannerView(adBannerId, AdSize.Banner, AdPosition.Top);
            AdRequest request = new AdRequest.Builder().Build();
            bannerView.LoadAd(request);
        }

        #endregion
    }
}