using System.Globalization;
using UnityEngine;

[AddComponentMenu("OnTheRun/UILocationTrigger")]
public class UILocationTrigger : MonoBehaviour
{
    void Signal_OnLocationButtonPressed(UIButton button)
    {
        int locationIndex = int.Parse(button.name.Substring("location".Length, 1), CultureInfo.InvariantCulture);

        bool[] comingSoonActive = OnTheRunDataLoader.Instance.GetTiersComingSoon();
        if (comingSoonActive[locationIndex - 1])
            return;

        Manager<UIManager>.Get().ActivePage.SendMessage("OnLocationReleased", locationIndex);
    }
}