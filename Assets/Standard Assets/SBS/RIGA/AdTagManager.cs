using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RIGA
{
    /// <summary>
    /// This class requests an adtag from the server and extracts the 
    /// image and tracking url. The image is downloaded from the 
    /// image-url and converted in a .NET Image object. The tracking-url
    /// is only invoked to generate an impression.
    /// </summary>
    public class AdTagManager : MonoBehaviour
    {
		#region Properties
	    public string BaseUrl = "http://reloadediga.com/ads/www/delivery/ajs.php";
        public bool UseBaseUrlAsImageUrl = true;
        public int ZoneId;
        public string Location = "http://www.reloadediga.com/";
		public float PollTime = 120;		// in seconds
		public float FirstPoll = 0;			// in seconds
		public bool EnablePolling = false;	// change to true and the PollTimer will run.
		public Material AdMaterial;

		public float DownloadProgress { get; set; }
		public bool ErrorWasThrown { get; set; }
		public string ErrorMessage { get; set; }
		
		private float _pollTimer;
		private float _pollTime;
		#endregion
		
		#region Event Handlers
		void Start()
		{
			// Initialize our poll timer
			_pollTimer = PollTime;
			// Initialize our first poll time
			_pollTime = FirstPoll;
			// If the user does not define a destination material
			//   use the current game object's renderer.
			if (AdMaterial == null)
			{
				if (this.GetComponent<Renderer>() != null)
					AdMaterial = this.GetComponent<Renderer>().material;
				else
				{
					// See if one of my parents has a renderer.
					Transform parent = this.gameObject.transform.parent;
					while(parent != null)
					{
						if (parent.GetComponent<Renderer>() != null)
						{
							AdMaterial = parent.GetComponent<Renderer>().material;
							break;
						}
						else
							parent = parent.parent;
					}
				}
			}
			if (!EnablePolling)
			{
				// If polling was disabled, just get the add one time.
				StartCoroutine(GetAdImage());
			}
		}
		
		void Update()
		{
			if (EnablePolling)
			{
				// Decrement our poll timer
				_pollTime -= Time.deltaTime;
				// When it expires,
				if (_pollTime <= 0)
				{
					// Reset it
					_pollTime = _pollTimer;
					// Get an ad
					StartCoroutine(GetAdImage());
				}
			}
		}
		#endregion
		
		#region Public Methods
        /// <summary>
        /// Calls the adtag and creates the .NET image. Invokes the tracking-url to generate an impression.
        /// Runs as a coroutine.
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetAdImage()
        {
			// Initialize
			ErrorWasThrown = false;
			ErrorMessage = "";
			
			// Assume no image is returned.
            Texture2D adImage = null;

            // Create random number, this is used to call the adtag
            int randNr = (int)((new System.Random()).NextDouble() * int.MaxValue);

            // Build the adtag url
            string sTagUrl = string.Format("{0}?zoneid={1}&cb={2}&charset=windows-1252&loc={3}", BaseUrl, ZoneId, randNr, Location);
			
			Debug.Log(sTagUrl);
			
			// Request the adtag content from the webserver
			WWW www = new WWW(sTagUrl);	       	
			// Wait for it to finish.
			while(!www.isDone) {
				DownloadProgress = www.progress;
				yield return null;
			}
			// If an error occurred, abort the mission.			
			if(www.error != null) {
				ErrorWasThrown = true;
				ErrorMessage = string.Format("GetAdImage: Error getting image for zone {0}.", ZoneId);
				ErrorMessage += www.error;
				yield break;
			}
			
			// If we got a response
			if (!string.IsNullOrEmpty(www.text))
			{
#if !UNITY_WP8
				Debug.Log(System.Text.Encoding.UTF8.GetString(www.bytes));
#endif
				// Extract image-url and tracking-url
	            string imageUrl, trackingUrl;
	            ExtractImageUrl(www.text, out imageUrl, out trackingUrl);
				
				// Make sure our urls are valid
				if (!string.IsNullOrEmpty(imageUrl) && !string.IsNullOrEmpty(trackingUrl))
				{
					// Get texture
					www = new WWW(imageUrl);	       	
					// Wait for it to finish
					while(!www.isDone) {
						DownloadProgress = www.progress;
						yield return null;
					}
					// If an error occurred, abort the mission.			
					if(www.error != null) {
						ErrorWasThrown = true;
						ErrorMessage = "GetAdImage: Error getting Image.\n";
						ErrorMessage += www.error;
						yield break;
					}
					// Get our texture
					adImage = www.texture;

					trackingUrl = trackingUrl.Replace("&amp;", "&");
					
					Debug.Log(trackingUrl);

					// Record impression
					www = new WWW(trackingUrl);
					while(!www.isDone) {
						DownloadProgress = www.progress;
						yield return null;
					}
					
					if(www.error != null) {
						ErrorWasThrown = true;
						ErrorMessage = "GetAdImage: Error tracking impression.\n";
						ErrorMessage += www.error;
						yield break;
					}
					
					Debug.Log(www.text);
				
					// if we have a material, update its texture
					if (AdMaterial != null) {
						Texture2D newTex = new Texture2D(adImage.width, adImage.height);
						newTex.SetPixels(adImage.GetPixels());
						newTex.Apply(true);
						AdMaterial.mainTexture = newTex;
					}
				}
				else
				{
					ErrorWasThrown = true;
				}
			}
			else
			{
				ErrorMessage = "GetAdImage: Invalid response from server.";
				ErrorWasThrown = true;
			}
        }
		#endregion
		
		#region Private Methods
        private void ExtractImageUrl(string tagContent, out string imageUrl, out string trackingUrl)
        {
			try
			{
	            string stringRight = ".jpg";
				// Find the first occurrence of ".jpg".
	            int posRight = tagContent.IndexOf(stringRight, StringComparison.OrdinalIgnoreCase);
				// If not found, return error.
	            if (posRight == -1) {
					// You cannot throw an exception. Just return.
					imageUrl = "";
					trackingUrl = "";
					ErrorMessage = "ExtractImageUrl: jpg token not found.";
					return;
				}
				// Find the substring that contains the tracking url
	            string trackPart = tagContent.Substring(posRight + stringRight.Length, tagContent.Length - posRight - stringRight.Length);
	            string leftPart = tagContent.Substring(0, posRight + stringRight.Length);
	
	            string stringLeft = "http";
	            int posLeft = leftPart.LastIndexOf(stringLeft);
	            if (posLeft == -1)
				{
					// You cannot throw an exception. Just return.
					imageUrl = "";
					trackingUrl = "";
					ErrorMessage = "ExtractImageUrl: http token not found.";
					return;
				}
	            imageUrl = leftPart.Substring(posLeft, leftPart.Length - posLeft);
	
	            // tracking url
	
			    stringLeft = "img src=\\'";
			    posLeft = trackPart.IndexOf(stringLeft, StringComparison.OrdinalIgnoreCase);
	            if (posLeft == -1)
				{
					// You cannot throw an exception.
					imageUrl = "";
					trackingUrl = "";
					ErrorMessage = "ExtractImageUrl: tracking img-tag not found.";
					return;
				}
	            string trackingUrlPart = trackPart.Substring(posLeft + stringLeft.Length, trackPart.Length - posLeft - stringLeft.Length);
	
			    stringRight = "\\'";
	            posRight = trackingUrlPart.IndexOf(stringRight);
	
	            trackingUrl = trackingUrlPart.Substring(0, posRight);
			}
			catch(Exception ex)
			{
				// You cannot throw an exception.
				imageUrl = "";
				trackingUrl = "";
				ErrorMessage = ex.Message;
				if (ex.InnerException != null)
					ErrorMessage += "\n" + ex.InnerException.Message;
			}
        }
		#endregion
    }
}
