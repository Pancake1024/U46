#if UNITY_ANDROID

using System;
using UnityEngine;

namespace GetJar.Android.SDK.Unity {

	/// <summary>
	/// This is <b>THE</b> GetJarManager class used by developers to register with the GetJar SDK.
	/// </summary>
	public class GetJarManager {
		private GetJarManager() {}
	
		/// <summary>
		/// This call registers with the GetJarManager and creates a context to use for making calls to the GetJar SDK.
		/// This method is non-blocking and returns the new GetJarContext instance immediately. This new instance can be used 
		/// for SDK calls. SDK call operations that require it will implicitly block until asynchronous authentication work is finished.
		/// <p>
		/// GetJarContext instances created with this method can be retrieved from the GetJarManager at any time via the
		/// GetJarManager.retrieveContext(String) method using the ID returned by GetJarContext.getGetJarContextId().
		/// </summary>
		/// <param name="applicationToken"> The application token value assigned by GetJar.</param>
		/// <returns>The new GetJarContext instance that is created.</returns>
		public static GetJarContext CreateContext(String applicationToken) {
			if(String.IsNullOrEmpty(applicationToken)){ throw(new ArgumentException("'applicationToken' cannot be NULL or empty")); }

			Debug.Log("GETJAR: GetJarManager.CreateContext() START");
			
			// Interop to our Java SDK to create a context
			GetJarContext getJarContext = null;
			using(AndroidJavaClass unityGetJarManagerClass = new AndroidJavaClass("com.getjar.sdk.unity.GetJarManager")) {
				string getJarContextId = unityGetJarManagerClass.CallStatic<string>("createContext", applicationToken);
				if(!String.IsNullOrEmpty(getJarContextId)) {
					getJarContext = new GetJarContext(getJarContextId);
				}
			}
			
			// Do some result logging
			if(getJarContext == null) {
				Debug.Log("GETJAR: GetJarManager.CreateContext() failed to create a context");
			} else {
				Debug.Log("GETJAR: GetJarManager.CreateContext() created and returning a context with ID " + getJarContext.GetGetJarContextId());
			}
			Debug.Log("GETJAR: GetJarManager.CreateContext() DONE");

			return(getJarContext);
		}

		/// <summary>
		/// This call registers with the GetJarManager and creates a context to use for making calls to the GetJar SDK.
		/// This method is non-blocking and returns the new GetJarContext instance immediately. This new instance can be used 
		/// for SDK calls. SDK call operations that require it will implicitly block until asynchronous authentication work is finished.
		/// <p>
		/// GetJarContext instances created with this method can be retrieved from the GetJarManager at any time via the
		/// GetJarManager.retrieveContext(String) method using the ID returned by GetJarContext.getGetJarContextId().
		/// </summary>
		/// <param name="applicationToken"> The application token value assigned by GetJar.</param>
		/// <param name="encryptionKey">The encodedPublicKey Base64-encoded public key used for Licensing</param>
		/// <returns>The new GetJarContext instance that is created.</returns>
		public static GetJarContext CreateContext(String applicationToken, String encryptionKey) {
			if(String.IsNullOrEmpty(applicationToken)){ throw(new ArgumentException("'applicationToken' cannot be NULL or empty")); }
			if(String.IsNullOrEmpty(encryptionKey)){ throw(new ArgumentException("'encryptionKey' cannot be NULL or empty")); }
			
			Debug.Log("GETJAR: GetJarManager.CreateContext() START");
			
			// Interop to our Java SDK to create a context
			GetJarContext getJarContext = null;
			using(AndroidJavaClass unityGetJarManagerClass = new AndroidJavaClass("com.getjar.sdk.unity.GetJarManager")) {
				string getJarContextId = unityGetJarManagerClass.CallStatic<string>("createContext", applicationToken, encryptionKey);
				if(!String.IsNullOrEmpty(getJarContextId)) {
					getJarContext = new GetJarContext(getJarContextId);
				}
			}
			
			// Do some result logging
			if(getJarContext == null) {
				Debug.Log("GETJAR: GetJarManager.CreateContext() failed to create a context");
			} else {
				Debug.Log("GETJAR: GetJarManager.CreateContext() created and returning a context with ID " + getJarContext.GetGetJarContextId());
			}
			Debug.Log("GETJAR: GetJarManager.CreateContext() DONE");

			return(getJarContext);
		}
	
		/// <summary>
		/// Returns the GetJarContext for the given context identifier or NULL if the given identifier is not found.
		/// </summary>
		/// <param name="contextId">The unique ID for a GetJarContext instance, as returned by GetJarContext.getGetJarContextId().</param>
		/// <returns>The GetJarContext for the given context identifier or NULL if the given identifier is not found.</returns>
		public static GetJarContext RetrieveContext(String contextId) {
			if(String.IsNullOrEmpty(contextId)){ throw(new ArgumentException("'contextId' cannot be NULL or empty")); }
			//return(new GetJarContext(CommManager.retrieveContext(contextId)));
			return(null);
		}

//		public static GetJarContext DoTests(String applicationToken) {
//			if(String.IsNullOrEmpty(applicationToken)){ throw(new ArgumentException("'applicationToken' cannot be NULL or empty")); }
//			Debug.Log("GETJAR: GetJarManager.CreateContext() START");
//
//			// Interop to our Java SDK to create a context
//			GetJarContext getJarContext = null;
//			using(AndroidJavaClass unityGetJarManagerClass = new AndroidJavaClass("com.getjar.sdk.unity.GetJarManager")) {
//				string getJarContextId = unityGetJarManagerClass.CallStatic<string>("createContext", applicationToken);
//				if(!String.IsNullOrEmpty(getJarContextId)) {
//					getJarContext = new GetJarContext(getJarContextId);
//				}
//			}
//			
//						// Do some result logging
//			if(getJarContext == null) {
//				Debug.Log("GETJAR: GetJarManager.CreateContext() failed to create a context");
//			} else {
//				Debug.Log("GETJAR: GetJarManager.CreateContext() created and returning a context with ID " + getJarContext.GetGetJarContextId());
//			}
//			Debug.Log("GETJAR: GetJarManager.CreateContext() DONE");
//
//			return(getJarContext);
//		}
	
	}

}

#endif