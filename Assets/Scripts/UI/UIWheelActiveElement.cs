using UnityEngine;
using SBS.Core;

[AddComponentMenu("OnTheRun/UI/UIWheelActiveElement")]
public class UIWheelActiveElement : MonoBehaviour
{
    public GameObject selectedGraphic;
    public GameObject normalGraphic;

    OnTheRunInterfaceSounds interfaceSounds;
    protected Rigidbody2D wheelRb;
    protected Transform centerTransformRef;
    protected UIWheelArrow wheelArrow;
    protected bool isADot;

    void Awake()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
        centerTransformRef = transform.parent.parent.parent.FindChild("centerRef");
        wheelRb = transform.parent.parent.GetComponent<Rigidbody2D>();
        wheelArrow = transform.parent.parent.parent.FindChild("red_arrow").GetComponent<UIWheelArrow>();
        isADot = gameObject.name.Contains("slice") ? false : true;
    }
    
	float tickTimer = -1f;
    void FixedUpdate()
    {
        if (wheelRb.isKinematic)
            return;

        if (!UIWheelPopup.ACTIVATE_LIGHTS)
            return;

        if (isADot && wheelArrow.actualDotCollided != gameObject)
        {
            float rotGap = Vector3.Angle(Vector3.up, transform.position - centerTransformRef.position);

            if (rotGap < 7.5f)
            {
                float now = TimeManager.Instance.MasterSource.TotalTime;
                if (now - tickTimer > 0.1f)
                {
                    tickTimer = now;
                    interfaceSounds.WheelTick();
                }
                wheelArrow.lastDotCollided = wheelArrow.actualDotCollided;
                wheelArrow.actualDotCollided = gameObject;
                selectedGraphic.SetActive(true);
                normalGraphic.SetActive(false);
                wheelArrow.SendMessage("OnActiveDotChanged", gameObject.name);
            }
            else
            {
                selectedGraphic.SetActive(false);
                normalGraphic.SetActive(true);
            }
        }
    }
}