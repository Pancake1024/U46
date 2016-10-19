using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using com.miniclip;

namespace com.miniclip.currencies
{
	public class MiniclipCurrencies : AbstractService, ICurrenciesAPI
	{	
		//External Function Name Constants
		public const string CURRENCIES_INIT							= "currencies_Init";
	
		public const string CURRENCIES_GET_BALANCE					= "currencies_GetBalance";
		public const string CURRENCIES_GET_BALANCES					= "currencies_GetBalances";
		public const string CURRENCIES_GET_USER_ITEM_QUANTITY		= "currencies_GetUserItemQuantity";
		
		public const string CURRENCIES_GET_USER_ITEMS_BY_GAME_ID	= "currencies_GetUserItemsByGameId";
		public const string CURRENCIES_GET_ITEM_BY_ID				= "currencies_GetItemById";
		public const string CURRENCIES_GET_ITEMS_BY_GAME_ID			= "currencies_GetItemsByGameId";
		
		public const string CURRENCIES_GET_BUNDLES					= "currencies_GetBundles";
		public const string CURRENCIES_GET_CURRENCY_BY_ID			= "currencies_GetCurrencyById";
		public const string CURRENCIES_GET_AVAILABLE_CURRENCIES		= "currencies_GetAvailableCurrencies"; //---
		
		public const string CURRENCIES_PURCHASE_BUNDLE				= "currencies_PurchaseBundle";
		public const string CURRENCIES_PURCHASE_ITEM				= "currencies_PurchaseItem";
		public const string CURRENCIES_PURCHASE_ITEMS				= "currencies_PurchaseItems";
		
		public const string CURRENCIES_ADJUST_CURRENCY_BALANCE		= "currencies_AdjustCurrencyBalance";
		public const string CURRENCIES_DECREMENT_ITEM_BALANCE		= "currencies_DecrementItemBalance";
		
		public const string CURRENCIES_GIVE_ITEM					= "currencies_GiveItem";
		
		public const string CURRENCIES_TOP_UP_CURRENCY				= "currencies_TopUpCurrency";
		public const string CURRENCIES_CONVERT_CURRENCY				= "currencies_ConvertCurrency";
		public const string CURRENCIES_SHOW_OFFER					= "currencies_ShowOffer";
		public const string CURRENCIES_OFFER_AVAILABLE				= "currencies_CurrenciesOfferAvailable";
		
				
		//init
		public event EventHandler<CurrenciesReadyEventArgs>	Ready;
		
		
		//currencies
		public event EventHandler<CurrenciesEventArgs>		CurrenciesInfoReceived;
		public event EventHandler							TopUpClosed;
		public event EventHandler<ConversionEventArgs>		CurrencyConverted;
		public event EventHandler<FailedEventArgs>      	CurrencyConversionFailed;
		public event EventHandler<FailedEventArgs>			Failed;
		public event EventHandler<MessageEventArgs>			Error;
		public event EventHandler<BalancesEventArgs>		BalancesInfoReceived;
		
		//items
		public event EventHandler<UserQuantitiesEventArgs>		UserItemQuantitiesReceived;
		public event EventHandler<ItemsEventArgs>				ItemsInfoReceived;
		public event EventHandler<ItemBalanceAdjustedEventArgs>	ItemBalanceAdjusted;
		public event EventHandler<ItemsPurchasedEventArgs>		ItemsPurchased;
		public event EventHandler								PurchaseCancelled;
		public event EventHandler<FailedEventArgs>				ItemsPurchaseFailed;
		
		//bundles
		public event EventHandler<BundlesEventArgs>			BundlesInfoReceived;
		public event EventHandler<BalancesEventArgs>		BundlePurchased;
		public event EventHandler<FailedEventArgs>			BundlePurchaseFailed;

		//offer
		public event EventHandler<OfferEventArgs>			OfferAvailability;	
		public event EventHandler							OfferShown;
		
		public event EventHandler<MessageEventArgs>			DebugMessage;
		
		
		public MiniclipCurrencies()
		{
			//
		}
		
		//-------------------------------------
		// Currencies AMF Service methods
		//-------------------------------------
		
		public void Init()
		{
			JSCaller.Call( CURRENCIES_INIT );
		}	
		
		//-------------------------------------
		
		public void GetBalance( int currencyId )
		{
			string jsonStr = "{ \"currency_id\" : " + currencyId + " }";
			JSCaller.Call( CURRENCIES_GET_BALANCE, jsonStr );  
		}
		
		public void GetBalances()
		{
			JSCaller.Call( CURRENCIES_GET_BALANCES );
		}
		
		public void GetUserItemQuantity( int itemId )
		{
			string jsonStr = "{ \"item_id\" : " + itemId + " }";
			JSCaller.Call( CURRENCIES_GET_USER_ITEM_QUANTITY, jsonStr );
		}
		
		public void GetUserItemsByGameId()
		{
			//game_id is supplied to the services_wrapper -> AMF service via flashvar
			
			Debug.Log("-> MiniclipCurrencies::GetUserItemsByGameId() - call: " + CURRENCIES_GET_USER_ITEMS_BY_GAME_ID );
			JSCaller.Call( CURRENCIES_GET_USER_ITEMS_BY_GAME_ID );
		}
					
		public void GetItemById( int itemId )
		{
			string jsonStr = "{ \"item_id\" : " + itemId + " }";
			JSCaller.Call( CURRENCIES_GET_ITEM_BY_ID, jsonStr);
		}
			
		public void GetItemsByGameId()
		{
			JSCaller.Call( CURRENCIES_GET_ITEMS_BY_GAME_ID );
		}
		
		public void GetBundles(int currencyId)
		{
			GetBundles(currencyId, null);
		}
		
		public void GetBundles(int currencyId, int[] currencies)
		{			
			Dictionary<string,object> dict = new Dictionary<string, object>();
			dict["currency_id"] = currencyId;
			dict["currencies"] = currencies;
			
			string jsonStr =  Json.Serialize(dict);
			JSCaller.Call( CURRENCIES_GET_BUNDLES, jsonStr );	
		}
		
		
		public void GetCurrencyById( int currencyId )
		{
			string jsonStr = "{ \"currency_id\" : " + currencyId + " }";
			JSCaller.Call( CURRENCIES_GET_CURRENCY_BY_ID, jsonStr );
		}
		
		
		public void GetAvailableCurrencies()
		{
			JSCaller.Call( CURRENCIES_GET_AVAILABLE_CURRENCIES );
		}
		
			
		public void PurchaseBundle( int bundleId, int currencyId )
		{
			Dictionary<string,object> dict = new Dictionary<string, object>();
			dict["bundle_id"] 	= bundleId;
			dict["currency_id"] 	= currencyId;
			
			string jsonStr = Json.Serialize( dict );
			
			//_flashSwitcher.DisplayServicesAndCall( CURRENCIES_PURCHASE_BUNDLE, jsonStr );
			JSCaller.Call( CURRENCIES_PURCHASE_BUNDLE, jsonStr );
		}
		
		public void PurchaseItem( int itemId, int currencyId, bool skipMax )
		{
			Dictionary<string,object> dict = new Dictionary<string, object>();
			dict["item_id"] 		= itemId;
			dict["currency_id"] 	= currencyId;
			dict["skip_max"] 		= skipMax;
			
			string jsonStr = Json.Serialize( dict );
		
			//_flashSwitcher.DisplayServicesAndCall( CURRENCIES_PURCHASE_ITEM, jsonStr );
			JSCaller.Call( CURRENCIES_PURCHASE_ITEM, jsonStr );
		}
		
		public void PurchaseItems( int[] itemIds, int currencyId, bool skipMax )
		{
			Dictionary<string,object> dict = new Dictionary<string, object>();
			dict["item_ids"] 		= itemIds;
			dict["currency_id"] 	= currencyId;
			dict["skip_max"] 		= skipMax;
			
			string jsonStr = Json.Serialize( dict );
			
			//_flashSwitcher.DisplayServicesAndCall( CURRENCIES_PURCHASE_ITEMS, jsonStr );
			JSCaller.Call( CURRENCIES_PURCHASE_ITEMS, jsonStr );
		}
		
		public void AdjustCurrencyBalance( int currencyId, decimal amount )
		{
			Dictionary<string,object> dict = new Dictionary<string, object>();
			dict["currency_id"] = currencyId;
			dict["amount"] 		= amount;
			
			string jsonStr = Json.Serialize( dict );
			
			JSCaller.Call( CURRENCIES_ADJUST_CURRENCY_BALANCE, jsonStr );
		}
		
		public void DecrementItemBalance( int itemId, decimal amount )
		{
			Dictionary<string,object> dict = new Dictionary<string, object>();
			dict["item_id"] = itemId;
			dict["amount"] 	= amount;
			
			string jsonStr = Json.Serialize( dict );
			
			JSCaller.Call( CURRENCIES_DECREMENT_ITEM_BALANCE, jsonStr );
		}
		
		public void GiveItem( int itemId, decimal amount )
		{
			Dictionary<string,object> dict = new Dictionary<string, object>();
			dict["item_id"] = itemId;
			dict["amount"] 	= amount;
			
			string jsonStr = Json.Serialize( dict );
			JSCaller.Call( CURRENCIES_GIVE_ITEM, jsonStr );
		}
		
		public void TopUpCurrency()
		{	
			JSCaller.Call( CURRENCIES_TOP_UP_CURRENCY );
		}
		
		public void ConvertCurrency( int sourceId, int destinationId, decimal sourceAmount)
		{
			Dictionary<string,object> dict = new Dictionary<string, object>();
			dict["src_id"] 		= sourceId;
			dict["des_id"] 		= destinationId;
			dict["src_amount"] 	= sourceAmount;
			
			string jsonStr = Json.Serialize( dict );
			
			JSCaller.Call( CURRENCIES_CONVERT_CURRENCY, jsonStr );
		}
		
		public void ShowOffer( int currencyId, string offerType)
		{
			Dictionary<string,object> dict = new Dictionary<string, object>();
			dict["currency_id"] 	= currencyId;
			dict["offer_type"] 		= offerType;

			string jsonStr = Json.Serialize( dict );
			
			JSCaller.Call( CURRENCIES_SHOW_OFFER, jsonStr );
		}
		
		public void CurrenciesOfferAvailable( int currencyId, string offerType)
		{
			Dictionary<string,object> dict = new Dictionary<string, object>();
			dict["currency_id"] 	= currencyId;
			dict["offer_type"] 		= offerType;

			string jsonStr = Json.Serialize( dict );
			
			JSCaller.Call( CURRENCIES_OFFER_AVAILABLE, jsonStr );
		}
			
		//----------------------------
		
		internal override void ProcessData( string noticeID, string json )
		{		
			string msg = "";
			
			Dictionary<string, object> fromJson;
			
			switch(noticeID)
			{
				case NoticeID.CURRENCIES_READY:	
					
					if(this.Ready != null)
					{
				
						fromJson = Json.Deserialize (json) as Dictionary<string,object>;
						Dictionary<string, CurrencyCurrency> currencies = null;	
						Dictionary<string, CurrencyItem> items = null;	
		
						if(fromJson.ContainsKey("currencies"))
						{
							//Application.ExternalEval("console.log('  ProcessCurrenciesData Currencies Before " + Time.realtimeSinceStartup + "')");
							Dictionary<string,object> currenciesDict = fromJson["currencies"] as Dictionary<string,object>;
							currencies = CurrenciesFactory.BuildCurrencies( currenciesDict );
							//Application.ExternalEval("console.log('  ProcessCurrenciesData Currencies After " + Time.realtimeSinceStartup + "')");
							//Debug.Log("Currencies Count: " + _currencies.Count);
						}
				
						if(fromJson.ContainsKey("items"))
						{
							//Application.ExternalEval("console.log('  ProcessCurrenciesData Items Before " + Time.realtimeSinceStartup + "')");
							Dictionary<string,object> itemsDict = fromJson["items"] as Dictionary<string,object>;
							items = CurrenciesFactory.BuildItems( itemsDict );   
							//Application.ExternalEval("console.log('  ProcessCurrenciesData Items After " + Time.realtimeSinceStartup + "')");
							//Debug.Log("Items Count: " + _items.Count);
						}
			
						this.Ready( this, new CurrenciesReadyEventArgs(currencies, items));
					}
				
					break;
				
				case NoticeID.CURRENCIES_ADJUST_CURRENCY_BALANCE:
				case NoticeID.CURRENCIES_BALANCES:
				case NoticeID.CURRENCIES_BALANCE:
				
					//msg = "-> MiniclipCurrencies::ProcessData() - NoticeID.CURRENCIES_BALANCE, json: " + json;
					//this.DebugMessage( this, new MessageEventArgs(msg) );
				
					if(this.BalancesInfoReceived != null)
					{		
						this.BalancesInfoReceived(this, new BalancesEventArgs( CurrenciesFactory.BuildBalances(json) ) );
					}
				
					break;
			
				case NoticeID.CURRENCIES_USER_ITEM_QUANTITY:
				case NoticeID.CURRENCIES_USER_ITEMS_BY_GAME_ID:
					
					//msg = "-> MiniclipCurrencies::ProcessData() - *NoticeID.CURRENCIES_USER_ITEM_QUANTITY, json: " + json;
					//this.DebugMessage(this, new MessageEventArgs(msg) );
							
					/*
					Dictionary<string, CurrencyUserQuantity> userQuantities = CurrenciesFactory.BuildUserQuantities( json );
				
					if(userQuantities == null)	
					{
						this.DebugMessage(this, new MessageEventArgs("--> userQuantities is null! :(") );
					}
					*/
					
					if(this.UserItemQuantitiesReceived != null)
					{
						this.UserItemQuantitiesReceived(this, new UserQuantitiesEventArgs( CurrenciesFactory.BuildUserQuantities( json ) ) );				
					}
				
					break;
					
				case NoticeID.CURRENCIES_GET_ITEM_BY_ID:
				case NoticeID.CURRENCIES_GET_ITEMS_BY_GAME_ID:
				
					//msg = "-> MiniclipCurrencies::ProcessData() - NoticeID.CURRENCIES_GET_ITEM_BY_ID, json:\n" + json + "\n";
					//this.DebugMessage(this, new MessageEventArgs(msg) );
				
					if(this.ItemsInfoReceived != null)
					{
						this.ItemsInfoReceived(this, new ItemsEventArgs( CurrenciesFactory.BuildItems( json ) ) );
					}
				
					break;
							
				case NoticeID.CURRENCIES_GET_CURRENCY_BY_ID:
				case NoticeID.CURRENCIES_GET_AVAILABLE_CURRENCIES:
				
					//msg = "-> MiniclipCurrencies::ProcessData() - NoticeID.CURRENCIES_GET_CURRENCY_BY_ID, json:\n" + json + "\n";
					//this.DebugMessage(this, new MessageEventArgs(msg) );
				
					if(this.CurrenciesInfoReceived != null)
					{
						this.CurrenciesInfoReceived(this, new CurrenciesEventArgs( CurrenciesFactory.BuildCurrencies( json ) ) );
					}
				
					break;
	
				case NoticeID.CURRENCIES_GET_BUNDLES:
					
					//msg = "-> MiniclipCurrencies::ProcessData() - NoticeID.CURRENCIES_GET_BUNDLES, json: " + json;
					//this.DebugMessage( this, new MessageEventArgs(msg) );
					
					if(this.BundlesInfoReceived != null)
					{
						this.BundlesInfoReceived(this, new BundlesEventArgs( CurrenciesFactory.BuildBundles(json) ) );
					}
					
					break;
				
				case NoticeID.CURRENCIES_BUNDLE_PURCHASED:
				
					if(this.BundlePurchased != null)
					{
						this.BundlePurchased(this, new BalancesEventArgs( CurrenciesFactory.BuildBalances(json) ) );
					}
				
					break;
				
				case NoticeID.CURRENCIES_PURCHASE_CANCELLED_BY_USER:
				
					if(this.PurchaseCancelled != null)
					{
						this.PurchaseCancelled(this, EventArgs.Empty);
					}
				
					break;
				
				case NoticeID.CURRENCIES_DECREMENT_ITEM_BALANCE:
				case NoticeID.CURRENCIES_GIVE_ITEM:
				
					if(this.ItemBalanceAdjusted != null)
					{
	
						//msg = "-> MiniclipCurrencies::ProcessData() - NoticeID.CURRENCIES_DECREMENT_ITEM_BALANCE, json: " + json;
						//this.DebugMessage( this, new MessageEventArgs(msg) );
					
						fromJson = Json.Deserialize(json) as Dictionary<string,object>;
						Dictionary<string, CurrencyUserQuantity> itemBalance = null;
						int itemId = 0;
						int amount = 0;
					
						itemBalance = CurrenciesFactory.ExtractUserQuantities( fromJson );
					
						if(fromJson.ContainsKey("item_id"))
						{
							itemId = Convert.ToInt32( fromJson["item_id"] );
							//msg = "-> MiniclipCurrencies::ProcessData() - item_id: " + itemId + "\n";
							this.DebugMessage( this, new MessageEventArgs(msg) );
						}
					
						if(fromJson.ContainsKey("amount"))
						{
							amount = Convert.ToInt32( fromJson["amount"] );
							//msg = "-> MiniclipCurrencies::ProcessData() - amount: " + amount + "\n";
							this.DebugMessage( this, new MessageEventArgs(msg) );
						}
					
						this.ItemBalanceAdjusted(this, new ItemBalanceAdjustedEventArgs( itemBalance, itemId, amount ) );
					}
				
					break;
					
				case NoticeID.CURRENCIES_PURCHASED:
				
					//msg = "-> MiniclipCurrencies::ProcessData() - NoticeID.CURRENCIES_PURCHASED, json: " + json + "\n";
					//this.DebugMessage( this, new MessageEventArgs(msg) );
					//this.DebugMessage( this, new MessageEventArgs("-> ItemsPurchased Fired! :)") );
				
					if(this.ItemsPurchased != null)
					{
						fromJson = Json.Deserialize(json) as Dictionary<string,object>;
						Dictionary<string, CurrencyUserQuantity> itmsPurchased = null;
						int[] itemIds = null;
				
						itmsPurchased = CurrenciesFactory.ExtractUserQuantities( fromJson );
					
					
						if(fromJson.ContainsKey("item_ids"))
						{
							//Debug.Log("-------> TestPurchaseItems() - contains 'item_ids' ");
							//Debug.Log("-------> TestPurchaseItems() - item_: "  + fromJson["item_ids"].ToString() );
							
							List<object> itemIdsList = fromJson["item_ids"] as List<object>;
							
							//Debug.Log("-------> TestPurchaseItems() - itemIdsList.Count: " + itemIdsList.Count);
							
							if( itemIdsList.Count > 0 )
							{
								itemIds = new int[ itemIdsList.Count ];
								
								for(int i = 0; i < itemIdsList.Count; i++)
								{
									itemIds[i] = Convert.ToInt32( itemIdsList[i] );
									Debug.Log("> TestPurchaseItems() - itemIds[" + i + "] : " + itemIds[i] );
									
								}
								
								//Debug.Log("-------> TestPurchaseItems() - itemIds.Length : " + itemIds.Length);	
							}			
						}
						
						this.ItemsPurchased(this, new ItemsPurchasedEventArgs( itmsPurchased, itemIds ));
					}
				
					break;
				
				case  NoticeID.CURRENCIES_CURRENCY_CONVERTED:
					
					if(this.CurrencyConverted != null)
					{
				
						fromJson = Json.Deserialize(json) as Dictionary<string,object>;
					
						int sourceId = 0;
						int destinationId = 0;
						int sourceAmount = 0;
					
						
						if( fromJson.ContainsKey("src_id") )
						{
							sourceId = Convert.ToInt32( fromJson["src_id"] );
						}
					
						if( fromJson.ContainsKey("des_id") )
						{
							destinationId = Convert.ToInt32( fromJson["des_id"] );
						}
					
						if( fromJson.ContainsKey("src_amount") )
						{
							sourceAmount = Convert.ToInt32( fromJson["src_amount"] );
						}
					
						this.CurrencyConverted( this, new ConversionEventArgs( CurrenciesFactory.BuildBalances(json), sourceId, destinationId, sourceAmount ) );
					}
				
					break;
				
				case NoticeID.CURRENCIES_TOPUP_CLOSED:
					//msg = "-> MiniclipCurrencies::ProcessData() - NoticeID.CURRENCIES_TOPUP_CLOSED, json:\n" + json + "\n";
					//this.DebugMessage(this, new MessageEventArgs(msg) );
					if(this.TopUpClosed != null)
					{
						this.TopUpClosed( this, EventArgs.Empty );
					}
				
					break;
				case NoticeID.CURRENCIES_OFFER_AVAILABILITY:
				
					if(this.OfferAvailability != null)
					{
						fromJson = Json.Deserialize(json) as Dictionary<string,object>;
					
						int currencyId = 0;
						string offerType = "";
						bool available = false;
					
						if( fromJson.ContainsKey("currency_id") )
						{
							currencyId = Convert.ToInt32( fromJson["currency_id"] );
						}
					
						if( fromJson.ContainsKey("offer_type") )
						{
							offerType = fromJson["offer_type"].ToString();
						}
					
						if( fromJson.ContainsKey("available") )
						{
							available = Convert.ToBoolean( fromJson["available"] );
						}
					
					
						this.OfferAvailability( this, new OfferEventArgs( currencyId, offerType, available ) );
					}
				
					break;
				
				case NoticeID.CURRENCIES_OFFER_COMPLETE:
				
					if(this.OfferShown != null)
					{
						this.OfferShown(this, EventArgs.Empty);
					}
				
					break;
				
				//-- Errors --
				
				case NoticeID.CURRENCIES_FAILED:
					//msg = "-> MiniclipCurrencies::ProcessData() - NoticeID.CURRENCIES_FAILED, json:\n" + json + "\n";
					//this.DebugMessage(this, new MessageEventArgs(msg) );
					
					if(this.Failed != null)
					{
						this.Failed(this, new FailedEventArgs( MiniclipUtils.ExtractCode(json), MiniclipUtils.ExtractMessage(json) ) );
					}
				
					break;
				
				case NoticeID.CURRENCIES_BUNDLE_PURCHASE_FAILED:
				
					if(this.BundlePurchaseFailed != null)
					{
						this.BundlePurchaseFailed(this, new FailedEventArgs( MiniclipUtils.ExtractCode(json), MiniclipUtils.ExtractMessage(json) ) );
					}
				
					break;
				
				case NoticeID.CURRENCIES_PURCHASE_FAILED:
								
					if(this.ItemsPurchaseFailed != null)
					{
						this.ItemsPurchaseFailed(this, new FailedEventArgs( MiniclipUtils.ExtractCode(json), MiniclipUtils.ExtractMessage(json) ) );
					}
				
					break;
				
				case NoticeID.CURRENCIES_CONVERSION_FAILED:
				
					if(this.CurrencyConversionFailed != null)
					{
						this.CurrencyConversionFailed(this, new FailedEventArgs( MiniclipUtils.ExtractCode(json), MiniclipUtils.ExtractMessage(json) ) );
					}
				
					break;
				
				case NoticeID.CURRENCIES_ERROR:
					
					if(this.Error != null)
					{
						this.Error( this, new MessageEventArgs( MiniclipUtils.ExtractMessage(json) ) );
					}
				
					break;
			}
		}
		
		/*
		private void DispatchDebugMessage( string message )
		{
			this.DebugMessage( this, new MessageEventArgs(message) );
		}
		*/
			

	}
}
