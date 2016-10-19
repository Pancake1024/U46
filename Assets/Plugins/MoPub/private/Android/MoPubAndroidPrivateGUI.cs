using UnityEngine;
using System.Collections.Generic;


public class MoPubAndroidPrivateGUI : MonoBehaviour
{
#if UNITY_ANDROID
	private int _selectedToggleIndex;
	private string _bannerAdUnit = "23b49916add211e281c11231392559e4";
	private string _interstitialAdUnit = "3aba0056add211e281c11231392559e4";
	private string[] _adVendorList = new string[] { "MoPub", "Millennial", "AdMob", "Chartboost", "Vungle", "Facebook", "AdColony" };


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
			if( _bannerAdUnit == "" )
			{
				Debug.LogWarning( "No banner ad unit ID is available for the currently selected platform" );
				return;
			}
			MoPub.createBanner( _bannerAdUnit, MoPubAdPosition.BottomRight );
			//MoPubAndroid.createBanner( _bannerAdUnit, MoPubAdPosition.BottomRight );
		}


		if( GUILayout.Button( "Destroy Banner" ) )
		{
			MoPub.destroyBanner();
			//MoPubAndroid.destroyBanner();
		}


		if( GUILayout.Button( "Show Banner" ) )
		{
			MoPub.showBanner( true );
			//MoPubAndroid.showBanner( true );
		}


		if( GUILayout.Button( "Hide Banner" ) )
		{
			MoPub.showBanner( false );
			//MoPubAndroid.showBanner( false );
		}


		GUILayout.EndVertical();
		GUILayout.EndArea();

		GUILayout.BeginArea( new Rect( Screen.width - halfWidth, 0, halfWidth, Screen.height ) );
		GUILayout.BeginVertical();


		if( GUILayout.Button( "Request Interstitial" ) )
		{
			MoPub.requestInterstitialAd( _interstitialAdUnit );
			//MoPubAndroid.requestInterstitialAd( _interstitialAdUnit );
		}


		if( GUILayout.Button( "Show Interstitial" ) )
		{
			MoPub.showInterstitialAd( _interstitialAdUnit );
			//MoPubAndroid.showInterstitialAd( _interstitialAdUnit );
		}


		GUILayout.Space( 40 );

		if( GUILayout.Button( "Report App Open" ) )
		{
			MoPub.reportApplicationOpen();
			//MoPubAndroid.reportApplicationOpen();
		}


		if( GUILayout.Button( "Enable Location Support" ) )
		{
			MoPub.enableLocationSupport( true );
			//MoPubAndroid.setLocationAwareness( MoPubLocationAwareness.NORMAL );
		}


		if( GUILayout.Button( "Add Test Devices" ) )
		{
			MoPubAndroid.addFacebookTestDeviceId( "ca4edcdcc30803fe789e6905e867de11" );
			MoPubAndroid.addAdMobTestDeviceId( "E2236E5E84CD318D4AD96B62B6E0EE2B" );
			MoPubAndroid.addAdMobTestDeviceId( "F151E58787624F5DBC9E8C7B863F3E51" );
		}

		GUILayout.EndVertical();
		GUILayout.EndArea();


		GUI.changed = false;
		_selectedToggleIndex = GUI.Toolbar( new Rect( 0, Screen.height - GUI.skin.button.fixedHeight, Screen.width, GUI.skin.button.fixedHeight ), _selectedToggleIndex, _adVendorList );
		if( GUI.changed )
		{
			switch( _selectedToggleIndex )
			{
				case 0: // MoPub
					_bannerAdUnit = "23b49916add211e281c11231392559e4";
					_interstitialAdUnit = "3aba0056add211e281c11231392559e4";
					break;
				case 1: // Millenial
					_bannerAdUnit = "8e3cf35f5192475c913a156886eaa19b";
					_interstitialAdUnit = "c6566f7bd85c40afb7afc4232a1cd463";
					break;
				case 2: // AdMob
					_bannerAdUnit = "173f4589c04a43b1b2e2e49d05f58e80";
					_interstitialAdUnit = "554e8baff8d84137941b5a55354105fc";
					break;
				case 3: // Chartboost
					_bannerAdUnit = "";
					_interstitialAdUnit = "376366b49d324dedae3d5edb360c27b4";
					break;
				case 4: // Vungle
					_bannerAdUnit = "";
					_interstitialAdUnit = "4f5e1e97f87c406cb7878b9eff1d2a77";
					break;
				case 5: // Facebook
					// Native: f97733db27f44defbeb39ce495047779
					_bannerAdUnit = "b40a96dd275e4ce5be2cdf5faa92007d";
					_interstitialAdUnit = "9792d876011f4359887d2d26380e8a84";
					break;
				case 6: // AdColony
					_bannerAdUnit = "";
					_interstitialAdUnit = "3aa79f11389540db8e250a80e4d16a46";
					break;
			}
		}
	}
#endif
}
