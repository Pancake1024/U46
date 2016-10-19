using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_ANDROID
using GetJar.Android.SDK.Unity;
using GetJar.Android.SDK.Unity.Response;
#endif

#if UNITY_ANDROID
public class GetJarAndroidManager : MonoBehaviour
{
    #region Singleton instance
    protected static GetJarAndroidManager instance = null;

    public static GetJarAndroidManager Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion

    #region Public events
    public event Action<string> licenseLoaded;
    public event Action<string> purchaseSucceededEvent;
    public event Action purchaseCancelledEvent;
    public event Action<string> purchaseFailedEvent;
    #endregion

    #region Public members
    public string applicationToken;
    public string encryptionKey;
    public string licenceProductId = string.Empty;
    #endregion

    #region Private members
    private GetJarContext _getJarContext = null;
    private UserAuth _userAuth = null;
    private GetJarPage _getJarPage = null;
    private Localization _localization = null;
    private Licensing _licensing = null;
    private RecommendedPrices _recommendedPrices = null;
    private Dictionary<string, Pricing> _pricings = null;
    private bool isLogged = false;
    #endregion

    #region Public properties
    public bool IsLogged
    {
        get
        {
            return isLogged;
        }
    }
    #endregion

    #region Public methods
    public void EnsureUserReciever(string userJson)
    {
        Debug.Log("GETJAR: EnsureUserReciever() recieved: " + userJson);
        if (String.IsNullOrEmpty(userJson))
        {
            Debug.Log("GETJAR: EnsureUserReciever() ensure user failed");

            // Try again
            //this._userAuth.EnsureUserAsync("User Auth From Unity!", name, "EnsureUserReciever");
        }
        else
        {
            Debug.Log("GETJAR: EnsureUserReciever() ensure user succeeded, starting additional SDK work...");

            isLogged = true;

            // We are now authed, so do other GetJar SDK work like loading licenses...
            if (licenceProductId.Length > 0)
                this._licensing.GetUnmanagedProductLicensesAsync(new String[] { licenceProductId }, name, "GetLicensesReceiver");

            // ...and loading recommended prices...
            //List<Pricing> pricingList = new List<Pricing>(2);
            //pricingList.Add(_PricingApp);
            //pricingList.Add(_PricingFish);
            this._localization.GetRecommendedPricesAsync(new List<Pricing>(_pricings.Values), name, "GetRecommendedPricesReceiver");
        }
    }

    public void GetLicensesReceiver(string licensesJson)
    {
        Debug.Log("GETJAR: GetLicensesReceiver() received: " + licensesJson);

        try
        {
            // Check the user's licenses for the one we care about
            /*JsonData licenses = JsonMapper.ToObject(licensesJson);
            for (int i = 0; i < licenses.Count; i++)
            {
                JsonData license = licenses[i];
                string licensedProductID = license["itemId"].ToString();
                if (_UpgradeProductID.Equals(licensedProductID))
                {
                    this._appLicensed = true;
                    return;
                }
            }
            this._appLicensed = false;*/

            if (licenseLoaded != null)
                licenseLoaded(licensesJson);
        }
        catch (Exception e)
        {
            Debug.LogError(
                "GETJAR: GetLicensesReceiver() failed" + Environment.NewLine +
                e.GetType().Name + Environment.NewLine +
                e.Message + Environment.NewLine +
                e.StackTrace);
        }
    }

    public void GetRecommendedPricesReceiver(string recommendedPricesJson)
    {
		Debug.Log("GETJAR: GetRecommendedPricesReceiver() received: " + recommendedPricesJson);
        _recommendedPrices = new RecommendedPrices(recommendedPricesJson);
		/*Debug.Log(
			"GETJAR: GetRecommendedPricesReceiver() lookup [base:" + 
			_PricingApp.GetBasePrice() + " recommended:" + 
			recommendedPrices.GetRecommendedPrice(_PricingApp) + "]");
		Debug.Log(
			"GETJAR: GetRecommendedPricesReceiver() lookup [base:" + 
			_PricingFish.GetBasePrice() + " recommended:" + 
			recommendedPrices.GetRecommendedPrice(_PricingFish) + "]");

		this._priceApp = recommendedPrices.GetRecommendedPrice(_PricingApp);
		this._priceFish = recommendedPrices.GetRecommendedPrice(_PricingFish);*/
	}

    public void StartUserAuth(Dictionary<string, Pricing> pricings)
    {
        _pricings = pricings;

        if (Application.platform == RuntimePlatform.Android)
        {
            this._userAuth.EnsureUserAsync("Select account for GetJar login", name, "EnsureUserReciever");
        }
    }

    public Nullable<int> GetProductPrice(string productId)
    {
        if (Application.platform == RuntimePlatform.Android && productId != null)
        {
            Pricing pricing = null;
            if (_pricings.TryGetValue(productId, out pricing))
            {
                if (_recommendedPrices == null)
                    return null;
                return _recommendedPrices.GetRecommendedPrice(pricing);
            }
        }
        return null;
    }

    public void BuyProduct(string productId, string productName, string productDesc)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            Pricing pricing = null;
            if (_pricings.TryGetValue(productId, out pricing))
            {
                Nullable<int> price = _recommendedPrices.GetRecommendedPrice(pricing);
                if (price.HasValue)
                {
                    this._getJarPage.SetConsumableProduct(productId, productName, productDesc, price.Value);
                    this._getJarPage.ShowPageAsync(name, "ShowPageReciever");
                }
            }
        }
    }

    public void ShowPageReciever(string responseJson)
    {
        //this._getJarPage.Close();

        Debug.Log("GETJAR: ShowPageReciever() recieved: " + responseJson);

        // Parse the response JSON
        Response response = Response.GetInstance(responseJson);
        if (response == null)
        {
            Debug.LogError("GETJAR: ShowPageReceiver() Unrecognized Response type!");
            return;
        }

        // React to various response types
        if (response is CloseResponse)
        {
            if (purchaseCancelledEvent != null)
                purchaseCancelledEvent();
        }
        else if (response is PurchaseSucceededResponse)
        {
            PurchaseSucceededResponse purchase = (PurchaseSucceededResponse)response;
            Debug.Log(
                "GETJAR: ShowPageReceiver() Purchased: " +
                " type:" + purchase.responseType +
                " amount:" + purchase.amount +
                " productId:" + purchase.productId +
                " productName:" + purchase.productName +
                " transactionId:" + purchase.transactionId);

            // The user has purchased something so credit them acordingly
            /*if (_FishProductID.Equals(purchase.productId))
            {
                if (PlayerPrefs.HasKey(KEY_FISH_COUNT))
                {
                    fish_count = PlayerPrefs.GetInt(KEY_FISH_COUNT);
                }
                PlayerPrefs.SetInt(KEY_FISH_COUNT, ++fish_count);
                PlayerPrefs.Save();
                Debug.Log("GETJAR: ShowPageReceiver()");
                // TODO: Maybe show some sort of "thank you for your purchase" UI
            }
            else if (_UpgradeProductID.Equals(purchase.productId))
            {
                this._appLicensed = true;  // In the future this is set by the license check
                // TODO: Maybe show some sort of "thank you for your purchase" UI
            }*/

            if (purchaseSucceededEvent != null)
                purchaseSucceededEvent(purchase.productId);
        }
        else
        {
            if (purchaseFailedEvent != null)
                purchaseFailedEvent(response.responseType);
        }
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        instance = this;

        Debug.Log("GETJAR: Awake()");

        this._getJarContext = GetJarManager.CreateContext(applicationToken);//, encryptionKey);
        //Debug.Log("GETJAR: Awake() new GetJarContext ID: " + this._getJarContext.GetGetJarContextId());

        this._getJarPage = new GetJarPage(this._getJarContext);
        this._localization = new Localization(this._getJarContext);
        this._licensing = new Licensing(this._getJarContext);
        this._userAuth = new UserAuth(this._getJarContext);
        /*
        if (PlayerPrefs.HasKey(KEY_FISH_COUNT))
        {
            fish_count = PlayerPrefs.GetInt(KEY_FISH_COUNT);
        }

        if (Application.platform == RuntimePlatform.Android)
        {
            this._userAuth.EnsureUserAsync("User Auth From Unity!", name, "EnsureUserReciever");
        }*/

        GameObject.DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        instance = null;
    }

    void OnApplicationQuit()
    {
        Debug.Log("GETJAR: OnApplicationQuit()");
        if (this._userAuth != null)
        {
            this._userAuth.Dispose();
        }
        if (this._getJarPage != null)
        {
            this._getJarPage.Dispose();
        }
        if (this._localization != null)
        {
            this._localization.Dispose();
        }
        if (this._licensing != null)
        {
            this._licensing.Dispose();
        }
    }
    #endregion
}
#endif