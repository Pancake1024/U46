using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using System.Globalization;

[AddComponentMenu("OnTheRun/OnTheRunShop")]
public class OnTheRunShop : MonoBehaviour
{
    #region Singleton instance
    protected static OnTheRunShop instance = null;

    public static OnTheRunShop Instance
    {
        get
        {
            return instance;
        }
    }
    #endregion
    
    #region inAppPurchase
	public class inAppPurchProduct
	{
		public string description;
		public string title;
		public string price;
		public string id;
		
		public override string ToString()
		{
			return id + ", " +  title + " (" + description + "), price: " + price;
		}
	}
	
	inAppPurchProduct[] products = null;
	//int productsCounter = 0;
	
	public inAppPurchProduct GetProductFromName(string name)
	{
		if (null == products)
			return null;
		foreach (inAppPurchProduct product in products)
		{
			string productName = product.id.Substring(product.id.LastIndexOf('.') + 1);
			if (productName == name)
				return product;
		}
		return null;
	}
	
	void onRequestStart(string productsCount)
	{
        int c = int.Parse(productsCount, CultureInfo.InvariantCulture);
		products = new inAppPurchProduct[c];
		//productsCounter = 0;
		for (int i = 0; i < c; ++i)
			products[i] = new inAppPurchProduct();
	}

	void onInvalidProductId(string identifier)
	{
		Debug.Log("Invalid product identifier: " + identifier);
	}

	void onProductData(string data, Action<inAppPurchProduct, string> setter)
	{/*
		Debug.Log(jsonData);
		CodeTitans.JSon.JSonReader jsonReader = new CodeTitans.JSon.JSonReader();
		object rawData = jsonReader.Read(jsonData);
		Dictionary<string, string> data = rawData as Dictionary<string, string>;
		if (null == data)
		{
			Debug.Log("Invalid product data " + rawData);
			return;
		}
		
		inAppPurchProduct product = products[productsCounter++] = new inAppPurchProduct();

		product.description = data["description"];
		product.title = data["title"];
		product.price = data["price"];
		product.id = data["id"];*/
		string[] indexAndData = data.Split('|');
        int index = int.Parse(indexAndData[0], CultureInfo.InvariantCulture);
		setter.Invoke(products[index], indexAndData[1]);
	}
    /*
	void onRequestEnd(string param)
	{}
	
	void onRequestError(string error)
	{
		Debug.Log("InAppPurchase error: " + error);
	}

	void provideContent(string productIdentifier)
	{
		Debug.Log("Provide Content: " + productIdentifier);
	}
	
	void onFinishTransaction(string param)
	{
		bool wasSuccessful = ('1' == param[0]);
		
		Debug.Log("Transaction finish: " + wasSuccessful);
		
		if (wasSuccessful)
            OnTheRunInterface.Instance.ShowPurchaseSuccessfulPopupNuggets();
		else
            OnTheRunInterface.Instance.ShowPurchaseFailedPopup();
	}
	
	void onProcessing(string productId)
	{
        OnTheRunInterface.Instance.OnProcessing(productId);
	}
	
	void onEndProcessing(string productId)
	{
        OnTheRunInterface.Instance.OnEndProcessing(productId);
	}*/
    #endregion
}