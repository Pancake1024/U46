using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SBS.Miniclip
{
	public class InAppPurchaseBindings
	{
		#if UNITY_IPHONE && !UNITY_EDITOR
		[DllImport("__Internal")]
		private static extern void loadStore();
		[DllImport("__Internal")]
		private static extern void loadStore_WithReceiptValidation();
		[DllImport("__Internal")]
		private static extern bool canMakePurchases();
		[DllImport("__Internal")]
		private static extern void purchaseProduct(string identifier);
        [DllImport("__Internal")]
        private static extern void restorePurchases();
#endif
        private static bool storeLoaded = false;
		
		public static void LoadStore()
		{
			if (!storeLoaded)
			{
#if UNITY_IPHONE && !UNITY_EDITOR
				loadStore();
#endif
				storeLoaded = true;
			}
		}

		public static void LoadStore_WithReceiptValidation()
		{
			if (!storeLoaded)
			{
#if UNITY_IPHONE && !UNITY_EDITOR
				loadStore_WithReceiptValidation();
#endif
				storeLoaded = true;
			}
		}

        public static void LoadStoreFake()
        {
            storeLoaded = true;
        }
        
        public static bool CanMakePurchases
		{
			get
			{
#if UNITY_IPHONE && !UNITY_EDITOR
				return canMakePurchases();
#else
                return true;
#endif
			}
		}
		
		public static void PurchaseProduct(string identifier)
		{
#if UNITY_IPHONE && !UNITY_EDITOR
			if (storeLoaded)
				purchaseProduct(identifier);
#endif
		}

        public static void RestorePurchases()
        {
#if UNITY_IPHONE && !UNITY_EDITOR
			if (storeLoaded)
				restorePurchases();
#endif
        }
    }
}
