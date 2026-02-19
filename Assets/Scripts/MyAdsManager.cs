/*using UnityEngine;

public class MyAdsManager : MonoBehaviour
{
    public GameManager gameManager;
    public bool activateInterstitial;
    public float interstitialTime = 60f;
    private float _elapsed;
    public int reward;
    public GameObject getCoinsUI;
    
    void Awake()
    {
        Advertisements.Instance.Initialize();
    }

    void Update()
    {
        if (activateInterstitial)
        {
            _elapsed += Time.deltaTime;
            if (_elapsed >= interstitialTime) {
                _elapsed = _elapsed % interstitialTime;
                ShowInterstitial();
            } 
        }

    }
    
    public void ShowInterstitial()
    {
        Advertisements.Instance.ShowInterstitial();
    }
    
    public void ShowRewardedVideo()
    {
        _elapsed = 0;
        Advertisements.Instance.ShowRewardedVideo(CompleteMethod);
    }
    
    private void CompleteMethod(bool completed)
    {
        if (completed)
        {
            print("compleed");
            gameManager.AddCoins(reward);
        }
        else
        {
            print("nooon");
        }
        getCoinsUI.SetActive(false);
    }
}
*/