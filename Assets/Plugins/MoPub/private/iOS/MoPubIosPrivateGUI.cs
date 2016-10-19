using UnityEngine;
using System.Collections.Generic;


public class MoPubIosPrivateGUI : MonoBehaviour
{
#if UNITY_IPHONE
	private int _selectedToggleIndex;
	private string _bannerAdUnit = "60f3aa089e234ce6ad64046174d065a0";
	private string _interstitialAdUnit = "6995f8b34ce6453197c1af577a9c283d";
	private string[] _adVendorList = new string[] { "iAd", "Millennial", "AdMob", "Chartboost", "Vungle", "Facebook", "AdColony", "MoPub" };


	void OnGUI()
	{
		GUI.skin.button.margin = new RectOffset( 0, 0, 10, 0 );
		GUI.skin.button.stretchWidth = true;
		GUI.skin.button.fixedHeight = ( Screen.width >= 960 || Screen.height >= 960 ) ? 70 : 30;

		var halfWidth = Screen.width / 2;
		GUILayout.BeginArea( new Rect( 0, 0, halfWidth, Screen.height ) );
		GUILayout.BeginVertical();

		if( GUILayout.Button( "Create Banner (bottom right)" ) )
		{
			MoPub.createBanner( _bannerAdUnit, MoPubAdPosition.BottomRight, MoPubBannerType.Size320x50 );
		}


		if( GUILayout.Button( "Create Banner (top center)" ) )
		{
			MoPub.createBanner( _bannerAdUnit, MoPubAdPosition.TopCenter, MoPubBannerType.Size320x50 );
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
			MoPub.reportApplicationOpen( "ITUNES_APP_ID" );
		}


		if( GUILayout.Button( "Enable Location Support" ) )
		{
			MoPub.enableLocationSupport( true );
		}

		GUILayout.EndVertical();
		GUILayout.EndArea();


		GUI.changed = false;
		_selectedToggleIndex = GUI.Toolbar( new Rect( 0, Screen.height - GUI.skin.button.fixedHeight, Screen.width, GUI.skin.button.fixedHeight ), _selectedToggleIndex, _adVendorList );
		if( GUI.changed )
		{
			switch( _selectedToggleIndex )
			{
				case 0:
					_bannerAdUnit = "60f3aa089e234ce6ad64046174d065a0";
					_interstitialAdUnit = "6995f8b34ce6453197c1af577a9c283d";
					break;

				case 1:
					_bannerAdUnit = "1b282680106246aa83036892b32ec7cc";
					_interstitialAdUnit = "0da9e2762f1a48bab695887fb7798b66";
					break;
				case 2:
					_bannerAdUnit = "d2b8a9fcd92440e79c7437d2b51f25a6";
					_interstitialAdUnit = "4f9d8fb8521f4420b2429184f720f42b";
					break;
				case 3:
					_bannerAdUnit = "";
					_interstitialAdUnit = "a97fa010d9c24d06ae267be2a1487af1";
					break;
				case 4:
					Debug.Log( "NO VUNGLE AD UNIT" );
					_bannerAdUnit = "";
					_interstitialAdUnit = "";
					break;
				case 5:
					_bannerAdUnit = "fb759131fd7a40e6b9d324e637a4b299";
					_interstitialAdUnit = "27614fde27df488493327f2b952f9d21";
					break;
				case 6:
					_bannerAdUnit = "fb759131fd7a40e6b9d324e637a4b299";
					_interstitialAdUnit = "27614fde27df488493327f2b952f9d21";
					break;
				case 7:
					_bannerAdUnit = "23b49916add211e281c11231392559e4";
					_interstitialAdUnit = "3aba0056add211e281c11231392559e4";
					break;
			}
		}
	}
#endif
}
