using UnityEngine;
using System.Collections.Generic;


public class MoPubDemoGUI : MonoBehaviour
{
#if UNITY_IPHONE || UNITY_ANDROID
	private string _bannerAdUnit = "23b49916add211e281c11231392559e4";
	private string _interstitialAdUnit = "3aba0056add211e281c11231392559e4";


	void OnGUI()
	{
		GUI.skin.button.margin = new RectOffset( 0, 0, 10, 0 );
		GUI.skin.button.stretchWidth = true;
		GUI.skin.button.fixedHeight = ( Screen.width >= 960 || Screen.height >= 960 ) ? 70 : 30;

		var halfWidth = Screen.width / 2;
		GUILayout.BeginArea( new Rect( 0, 0, halfWidth, Screen.height ) );
		GUILayout.BeginVertical();

		if( GUILayout.Button( "Create Banner (bottom center)" ) )
		{
			MoPub.createBanner( _bannerAdUnit, MoPubAdPosition.BottomCenter );
		}


		if( GUILayout.Button( "Create Banner (top center)" ) )
		{
			MoPub.createBanner( _bannerAdUnit, MoPubAdPosition.TopCenter );
		}


		if( GUILayout.Button( "Destroy Banner" ) )
		{
			MoPub.destroyBanner();
		}


		if( GUILayout.Button( "Show Banner" ) )
		{
			MoPub.showBanner( true );
		}


		if( GUILayout.Button( "Hide Banner" ) )
		{
			MoPub.showBanner( false );
		}


		GUILayout.EndVertical();
		GUILayout.EndArea();

		GUILayout.BeginArea( new Rect( Screen.width - halfWidth, 0, halfWidth, Screen.height ) );
		GUILayout.BeginVertical();


		if( GUILayout.Button( "Request Interstitial" ) )
		{
			MoPub.requestInterstitialAd( _interstitialAdUnit );
		}


		if( GUILayout.Button( "Show Interstitial" ) )
		{
			MoPub.showInterstitialAd( _interstitialAdUnit );
		}


		if( GUILayout.Button( "Report App Open" ) )
		{
			MoPub.reportApplicationOpen();
		}


		if( GUILayout.Button( "Enable Location Support" ) )
		{
			MoPub.enableLocationSupport( true );
		}

		GUILayout.EndVertical();
		GUILayout.EndArea();
	}
#endif
}
