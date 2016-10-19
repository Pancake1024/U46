using UnityEngine;

public class UIMiniclipBtn : MonoBehaviour
{
    protected OnTheRunInterfaceSounds interfaceSounds;
    public const int kArrowDistance = 30;

    public Sprite arrow;
    public Sprite arrowOver;

    public Color textColorOver;

    protected UIButton button;
    protected UITextField textField;
    protected Color textColor;

    protected float pixelsToUnit;
    protected GameObject arrowGO;
    protected GameObject arrowOverGO;

    void Awake()
    {
        interfaceSounds = GameObject.FindGameObjectWithTag("Sounds").GetComponent<OnTheRunInterfaceSounds>();

        button = gameObject.GetComponent<UIButton>();

        button.onMouseOverEvent.AddTarget(gameObject, "MiniclipMouseOver");
        button.onMouseOutEvent.AddTarget(gameObject, "MiniclipMouseOut");
        button.onReleaseOutsideEvent.AddTarget(gameObject, "MiniclipMouseOut");

        textField = gameObject.GetComponentInChildren<UITextField>();
        if (textField != null)
            textColor = textField.color;
#if UNITY_WP8
        return;
#endif
        if (arrow != null && textField != null)
        {
            button.resizeOffset.right += (Mathf.RoundToInt(arrow.textureRect.width));// + kArrowDistance);

            pixelsToUnit = arrow.bounds.size.y / arrow.textureRect.height;

            arrowGO = new GameObject("arrow");
            arrowGO.transform.parent = transform;
            arrowGO.transform.localPosition = Vector3.zero;
            arrowGO.transform.localRotation = Quaternion.identity;
            arrowGO.layer = gameObject.layer;
            SpriteRenderer spriteRndr = arrowGO.AddComponent<SpriteRenderer>();
            spriteRndr.sprite = arrow;
            spriteRndr.sortingOrder = textField.sortingOrder;

            arrowOverGO = new GameObject("arrowOver");
            arrowOverGO.transform.parent = transform;
            arrowOverGO.transform.localPosition = Vector3.zero;
            arrowOverGO.transform.localRotation = Quaternion.identity;
            arrowOverGO.layer = gameObject.layer;
            spriteRndr = arrowOverGO.AddComponent<SpriteRenderer>();
            spriteRndr.sprite = arrowOver;
            spriteRndr.sortingOrder = textField.sortingOrder;

            arrowOverGO.SetActive(false);
        }
    }

    public void MiniclipMouseOver(UIButton btn)
    {
        //Debug.Log("MiniclipMouseOver");
        if (textField != null)
        {
            textField.color = textColorOver;
            textField.ApplyParameters();

            if (arrowGO != null)
            {
                arrowGO.SetActive(false);
                arrowOverGO.SetActive(true);
            }
        }
    }

    public void MiniclipMouseOut(UIButton btn)
    {
        //Debug.Log("MiniclipMouseOut");
        if (textField != null)
        {
            textField.color = textColor;
            textField.ApplyParameters();

            if (arrowGO != null)
            {
                arrowGO.SetActive(true);
                arrowOverGO.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (arrowGO != null)
        {
            arrowGO.transform.position = new Vector3(textField.GetTextBounds().xMax + kArrowDistance * pixelsToUnit, textField.GetTextBounds().center.y, button.normal.transform.position.z);// + 0.01f);
            arrowOverGO.transform.position = arrowGO.transform.position;
        }
    }
}
