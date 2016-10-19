using System;

namespace com.miniclip
{
	public interface ICurrenciesService
	{			
		void Init();
		void GetBalance( int currencyId );
		void GetBalances();
		void GetUserItemQuantity( int itemId );
		void GetUserItemsByGameId();
		void GetItemById( int itemId );
		void GetItemsByGameId();
		void GetBundles(int currencyId);
		void GetBundles(int currencyId, int[] currencies);
		void GetCurrencyById( int currencyId );
		void GetAvailableCurrencies();
		void PurchaseBundle( int bundleId, int currencyId );
		void PurchaseItem( int itemId, int currencyId, bool skipMax );
		void PurchaseItems( int[] itemIds, int currencyId, bool skipMax );
		void AdjustCurrencyBalance( int currencyId, decimal amount );
		void DecrementItemBalance( int itemId, decimal amount );
		void GiveItem( int itemId, decimal amount );
		void TopUpCurrency();
		void ConvertCurrency( int sourceId, int destinationId, decimal sourceAmount);
		void ShowOffer( int currencyId, string offerType);
		void CurrenciesOfferAvailable( int currencyId, string offerType);
	}
}

