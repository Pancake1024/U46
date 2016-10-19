#if UNITY_ANDROID && UNITY_KINDLE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class AmazonReceipt 
{
	public string type;
	public string token;
	public string sku;
	public string subscriptionStartDate;
	public string subscriptionEndDate;
	
	
	public static List<AmazonReceipt> fromArrayList( ArrayList array )
	{
		var items = new List<AmazonReceipt>();

		// create DTO's from the Hashtables
		foreach( Hashtable ht in array )
			items.Add( new AmazonReceipt( ht ) );
		
		return items;
	}
	
	
	public AmazonReceipt( Hashtable ht )
	{
		type = ht["type"].ToString();
		token = ht["token"].ToString();
		sku = ht["sku"].ToString();
		
		if( ht.ContainsKey( "subscriptionStartDate" ) )
			subscriptionStartDate = ht["subscriptionStartDate"].ToString();
		
		if( ht.ContainsKey( "subscriptionEndDate" ) )
			subscriptionStartDate = ht["subscriptionEndDate"].ToString();
	}
	
	
	public override string ToString()
	{
		return string.Format( "<AmazonReceipt> type: {0}, token: {1}, sku: {2}", type, token, sku );
	}

}

#endif