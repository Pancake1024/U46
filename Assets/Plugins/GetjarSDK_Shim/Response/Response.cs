#if UNITY_ANDROID

using System;
using LitJson;

namespace GetJar.Android.SDK.Unity.Response {
	
	/// <summary>
	/// The base class for types that are returned to the callback for calls to GetJarPage.ShowPageAsync().
	/// Use the Response.GetInstance() method for parsing the JSON received by that callback into a Response instance.
	/// </summary>
	public abstract class Response {
		
		/// <summary>
		/// The string representation of the Java type of the Response subclass that this instance is.
		/// </summary>
		public string responseType;
		
		public static Response GetInstance(String jsonText) {
			
			// Validate arguments
	    	if(String.IsNullOrEmpty(jsonText)) { throw(new ArgumentException("'jsonText' cannot be NULL or empty")); }

			// Parse the Response JSON
			JsonData data = JsonMapper.ToObject(jsonText);
			String type = ((JsonData)data["responseType"]).ToString();
			if(String.IsNullOrEmpty(type)) {
				throw(new ArgumentException("'jsonText' contains no 'responseType' and is not valid Response JSON"));
			}

			// Parse the Response JSON into an instance of the correct object type
			if(type.EndsWith(".PurchaseSucceededResponse")) {
				return(JsonMapper.ToObject<PurchaseSucceededResponse>(jsonText));
			} else if(type.EndsWith(".CloseResponse")) {
				return(JsonMapper.ToObject<CloseResponse>(jsonText));
			} else if(type.EndsWith(".BlacklistedResponse")) {
				return(JsonMapper.ToObject<BlacklistedResponse>(jsonText));
			} else if(type.EndsWith(".DeviceUnsupportedResponse")) {

				// TODO: Support parsing the 'deviceMetadata', etc.
				DeviceUnsupportedResponse dur = new DeviceUnsupportedResponse();
				dur.responseType = type;
				return(dur);
			}

			// Return NULL for unrecognized Response types
			return(null);
		}

	}

}

#endif