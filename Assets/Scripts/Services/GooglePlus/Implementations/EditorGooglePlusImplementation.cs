using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EditorGooglePlusImplementation : MonoBehaviour, GooglePlusImplementation
{
    const string userName = "Daniele Mori";
    const string userPhotoUrl = "https://lh4.googleusercontent.com/-9fXjw7JDE9k/AAAAAAAAAAI/AAAAAAAAABE/Mscp0AsW7gA/photo.jpg?sz=50";
    const string userProfileUrl = "https://plus.google.com/105479771674851561370";
    const string userEmail = "danielemori.tests@gmail.com";

    /*{
        "picureUrlsById": [
            {
                "userId": "107918808361150699391",
                "pictureUrl": "https://lh3.googleusercontent.com/-vuSaudwpIRY/AAAAAAAAAAI/AAAAAAAAAAA/K5TTbmGUkFY/photo.jpg?sz=50"
            },
            {
                "userId": "107806035033909502370",
                "pictureUrl": "https://lh3.googleusercontent.com/-e4Vz-jMExT8/AAAAAAAAAAI/AAAAAAAACkE/gPesFxA1-bk/photo.jpg?sz=50"
            },
            {
                "userId": "101052446263470941246",
                "pictureUrl": "https://lh5.googleusercontent.com/-gzDP3rg49WU/AAAAAAAAAAI/AAAAAAAAAIQ/uR5c-a5KiwY/photo.jpg?sz=50"
            }
        ]
    }*/

    GooglePlusImplementationCallbacksContainer callbacksContainer;

    public void SetCallbacksContainer(GooglePlusImplementationCallbacksContainer callbacksContainer)
    {
        this.callbacksContainer = callbacksContainer;
    }

    public void Login()
    {
        callbacksContainer.onLoginCompleted(true);
    }

    public void RequestUserData()
    {
        callbacksContainer.onUserDataRequestCompleted(new GooglePlusUser(userName, userPhotoUrl, userProfileUrl, userEmail));
    }

    public void RequestAccessToken(string userEmail)
    {
        callbacksContainer.onAccessTokenRequestCompleted("FAKE_ACCESS_TOKEN", false);
    }

    public void RequestOtherUsersPicureUrl(List<string> idsList)
    {
        Dictionary<string, string> urlsById = new Dictionary<string,string>();
        foreach (string userId in idsList)
            urlsById.Add(userId, userPhotoUrl);

        callbacksContainer.onOtherUsersPicureUrlRequestCompleted(urlsById);
    }
}