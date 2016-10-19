using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_ANDROID && UNITY_KINDLE

public class AmazonIAP
{
	private static AndroidJavaObject _plugin;
    		
	static AmazonIAP()
	{
		if( Application.platform != RuntimePlatform.Android )
			return;

		// find the plugin instance
		using( var pluginClass = new AndroidJavaClass( "com.amazon.AmazonIAPPlugin" ) )
			_plugin = pluginClass.CallStatic<AndroidJavaObject>( "instance" );
	}

	// Sends off a request to fetch all the avaialble products. This MUST be called before any other methods
	public static void initiateItemDataRequest( string[] items )
	{		
        if( Application.platform != RuntimePlatform.Android)
			return;
		
		var initMethod = AndroidJNI.GetMethodID( _plugin.GetRawClass(), "initiateItemDataRequest", "([Ljava/lang/String;)V" );
		AndroidJNI.CallVoidMethod( _plugin.GetRawObject(), initMethod, AndroidJNIHelper.CreateJNIArgArray( new object[] { items } ) );
	}


	// Purchases the given sku
	public static void initiatePurchaseRequest( string sku )
	{
		if( Application.platform != RuntimePlatform.Android)
			return;

		_plugin.Call( "initiatePurchaseRequest", sku );
	}
	
	
	// Sends off a request to fetch the logged in user's id
	public static void initiateGetUserIdRequest()
	{
		if( Application.platform != RuntimePlatform.Android )
			return;
		
		_plugin.Call( "initiateGetUserIdRequest" );
	}

}
#endif