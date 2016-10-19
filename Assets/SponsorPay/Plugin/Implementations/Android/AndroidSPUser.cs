using UnityEngine;
using System;

namespace SponsorPay
{
#if UNITY_ANDROID
	public class AndroidSPUser : SPUser
	{

		private AndroidJavaObject spUser;
		
		private AndroidJavaObject SPUserObject
		{
			get
			{
				if (null == spUser)
				{
					spUser = new AndroidJavaObject("com.sponsorpay.unity.SPUserWrapper");
				}
				return spUser;
			}
		}


		protected override void NativePut(string json)
		{
			SPUserObject.Call("put", json);
		}

		protected override void NativeReset()
		{
			SPUtils.printWarningMessage();
		}

		protected override string GetJsonMessage(string key)
		{
			return SPUserObject.Call<string>("get", key);
		}

	}
#endif
}

