using UnityEngine;

public class UIAnims : MonoBehaviour
{
    void OnDeactivateMe()
    {
        this.gameObject.SetActive(false);
    }
}