using UnityEngine;
using SBS.Core;
using System.Globalization;

[AddComponentMenu("OnTheRun/UI/UIWheelArrow")]
public class UIWheelArrow : MonoBehaviour
{
    public Rigidbody2D wheelRb; 
    protected Transform currSliceActive = null;
    protected Transform sliceNext = null;
    protected Transform slicePrev = null;

    protected OnTheRunInterfaceSounds interfaceSounds;

    protected bool enableTickSound = false;

    public Transform CurrentSliceActive
    {
        get { return currSliceActive; }
    }

    public GameObject lastDotCollided = null;
    public GameObject actualDotCollided = null;
    private float dotLightOffTimer = -1.0f;
    private UIWheelPopup wheelPopup;
    
    /*
    void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position - transform.up);
    }
    */

    void Awake()
    {
        wheelPopup = transform.parent.parent.parent.gameObject.GetComponent<UIWheelPopup>();
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();
    }

    void FixedUpdate()
    {
        if (UIWheelPopup.ACTIVATE_LIGHTS)
            return;

        float currentAngle = gameObject.transform.rotation.eulerAngles.z;
        if (!enableTickSound)
            enableTickSound = currentAngle > 30.0f;
        if (enableTickSound && currentAngle < 30.0f)
        {
            interfaceSounds.WheelTick();
            enableTickSound = false;
        }
    }

    /*void Update()
    {
        if (wheelRb.isKinematic)
            return;

        if (lastDotCollided == null)
            return;

        //actualDotCollided.GetComponent<UIWheelActiveElement>().selectedGraphic.SetActive(true);
        //actualDotCollided.GetComponent<UIWheelActiveElement>().normalGraphic.SetActive(!false);

        if (Vector3.Cross(-transform.up, lastDotCollided.transform.position - transform.position).z <= 0.0f)
            return;

        string dotName = lastDotCollided.name;

        int index = int.Parse(dotName.Substring(3, dotName.Length - 3));

        bool firstCheck = index == 0 || index == 21 || index == 18 || index == 15 || index == 12 || index == 9 || index == 6 || index == 3; // index == 0 ||
        if (firstCheck)
        {
            Transform parent = lastDotCollided.transform.parent;
            sliceNext = index == 0 ? parent.parent.FindChild("slices/slice7").transform : parent.parent.FindChild("slices/slice" + ((index / 3) - 1)).transform;
            slicePrev = index == 0 ? parent.parent.FindChild("slices/slice0").transform : parent.parent.FindChild("slices/slice" + ((index / 3))).transform;
            bool secondCheck = currSliceActive != sliceNext;

            if (secondCheck)
            {
                currSliceActive = sliceNext;

                wheelPopup.ResetSlices();

                sliceNext.GetComponent<UIWheelActiveElement>().selectedGraphic.SetActive(true);
                sliceNext.GetComponent<UIWheelActiveElement>().normalGraphic.SetActive(false);
            }
        }


        if (dotLightOffTimer > 0.0f)
        {
            if (TimeManager.Instance.MasterSource.TotalTime - dotLightOffTimer > 0.85f)
            {
                actualDotCollided.GetComponent<UIWheelActiveElement>().selectedGraphic.SetActive(false);
                actualDotCollided.GetComponent<UIWheelActiveElement>().normalGraphic.SetActive(true);
                dotLightOffTimer = -1.0f;
            }
        }
    }*/

    void OnActiveDotChanged(string dotName)
    {
        if (lastDotCollided == null)
            return;

        int index = int.Parse(dotName.Substring(3, dotName.Length - 3), CultureInfo.InvariantCulture);
        bool firstCheck = index == 0 || index == 23 || index == 20 || index == 17 || index == 14 || index == 11 || index == 8 || index == 5 || index == 2; // index == 0 ||
        if (firstCheck)
        {
            Transform parent = lastDotCollided.transform.parent;
            int sliceNextIndex = ((index + 1) / 3) - 1;
            int slicePrevIndex = sliceNextIndex + 1;
            if (index == 0)
            {
                sliceNextIndex = 0;
                slicePrevIndex = 1;
            }
            else if (index == 23)
            {
                sliceNextIndex = 7;
                slicePrevIndex = 0;
            }

            sliceNext = parent.parent.FindChild("slices/slice" + sliceNextIndex).transform;
            slicePrev = parent.parent.FindChild("slices/slice" + slicePrevIndex).transform;
            bool secondCheck = currSliceActive != sliceNext;

            if (secondCheck)
            {
                currSliceActive = sliceNext;

                wheelPopup.ResetSlices();

                sliceNext.GetComponent<UIWheelActiveElement>().selectedGraphic.SetActive(true);
                sliceNext.GetComponent<UIWheelActiveElement>().normalGraphic.SetActive(false);

            }
        }
    }

    /*void SetupDots(Transform parent, int initIndex, bool lightOn)
    {
        if (lightOn)
        {
            for (int i = initIndex; i > initIndex - 4; --i)
            {
                int tmp = i;
                if (tmp == 24) tmp = 0;
                Transform currDot = parent.FindChild("dot" + tmp).transform;
                if (currDot == actualDotCollided.transform)
                {
                    currDot.GetComponent<UIWheelActiveElement>().selectedGraphic.SetActive(lightOn);
                    currDot.GetComponent<UIWheelActiveElement>().normalGraphic.SetActive(!lightOn);
                }
            }

            sliceNext.GetComponent<UIWheelActiveElement>().selectedGraphic.SetActive(lightOn);
            sliceNext.GetComponent<UIWheelActiveElement>().normalGraphic.SetActive(!lightOn);
            currSliceActive = sliceNext;
        }
        else
        {
            for (int i = initIndex; i < initIndex + 4; ++i)
            {
                int tmp = i;
                if (tmp >= 24) tmp = i - 24;
                Transform currDot = parent.FindChild("dot" + tmp).transform;
                currDot.GetComponent<UIWheelActiveElement>().selectedGraphic.SetActive(lightOn);
                currDot.GetComponent<UIWheelActiveElement>().normalGraphic.SetActive(!lightOn);
            }

            slicePrev.GetComponent<UIWheelActiveElement>().selectedGraphic.SetActive(lightOn);
            slicePrev.GetComponent<UIWheelActiveElement>().normalGraphic.SetActive(!lightOn);
        }
    }*/
}