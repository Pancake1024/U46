using UnityEngine;
using System.Collections;
using SBS.Core;

public class HelicopterTargetBehaviour : MonoBehaviour
{
    public GameObject helicopterRef;
    protected int flagEnter = -1;

    void OnTriggerEnter(Collider other)
    {
        if (helicopterRef == null || other.gameObject == null)
			return;

        if (other.gameObject.tag.Equals("Player"))
        {
            ++flagEnter;
            flagEnter = Mathf.Clamp(flagEnter, -1, 0);
            if (flagEnter == 0)
                helicopterRef.SendMessage("OnTargetEntered", null, SendMessageOptions.DontRequireReceiver);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (helicopterRef == null || other.gameObject == null)
			return;

        if (other.gameObject.tag.Equals("Player"))
        {
            --flagEnter;
            flagEnter = Mathf.Clamp(flagEnter, -1, 0);
            if (flagEnter == -1)
                helicopterRef.SendMessage("OnTargetExit", null, SendMessageOptions.DontRequireReceiver);
        }
    }
}