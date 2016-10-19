using UnityEngine;
using System.Collections;

public class GooglePlusUser
{
    string name = string.Empty;
    string photoUrl = string.Empty;
    string profileUrl = string.Empty;
    string email = string.Empty;

    public string Name { get { return name; } }
    public string PhotoUrl { get { return photoUrl; } }
    public string ProfileUrl { get { return profileUrl; } }
    public string Email { get { return email; } }

    public GooglePlusUser()
    {
        name = "n/a";
        photoUrl = "n/a"; ;
        profileUrl = "n/a"; ;
        email = "n/a"; ;
    }

    public GooglePlusUser(string name, string photoUrl, string profileUrl, string email)
    {
        this.name = name;
        this.photoUrl = photoUrl;
        this.profileUrl = profileUrl;
        this.email = email;
    }

    public override string ToString()
    {
        return string.Format("Name: {0}\nPhotoUlr: {1}\nProfileUrl: {2}\nEmail: {3}", name, photoUrl, profileUrl, email);
    }
}