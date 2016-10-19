#if UNITY_ANDROID && UNITY_KINDLE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class AmazonItem
{
	public string description;
	public string type;
	public string price;
	public string sku;
	public string smallIconUrl;
	public string title;
	
	
	public static List<AmazonItem> fromArrayList( ArrayList array )
	{
		var items = new List<AmazonItem>();

		// create DTO's from the Hashtables
		foreach( Hashtable ht in array )
			items.Add( new AmazonItem( ht ) );
		
		return items;
	}
	
	
	public AmazonItem( Hashtable ht )
	{
		description = ht["description"].ToString();
		type = ht["type"].ToString();
		price = ht["price"].ToString();
		sku = ht["sku"].ToString();
		smallIconUrl = ht["smallIconUrl"].ToString();
		title = ht["title"].ToString();
	}
	
	
	public override string ToString()
	{
		return string.Format( "<AmazonItem> type: {0}, sku: {1}, price: {2}, title: {3}, description: {4}", type, sku, price, title, description );
	}

}

#endif