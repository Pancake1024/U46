#if USE_UNIWF
using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;
using SBS.uniSWF;

[AddComponentMenu("SBS/uniSWF/ScrollersBehaviour")]
public class ScrollersBehaviour : MonoBehaviour
{
    #region Protected members
    protected TimeSource timeSource = null;
    protected ScrollersDirector director = null;
    #endregion

    #region Public properties
    public bool IsDragging
    {
        get
        {
            return director.isDragging();
        }
    }
    #endregion

    #region Public methods
    public ScrollingController CreateController(Rect scrollArea, ScrollingController.Settings settings)
    {
        return director.addController(scrollArea, settings);
    }

    public void DestroyController(ScrollingController controller)
    {
        director.removeController(controller);
    }
    #endregion

    #region Unity callbacks
    void Start()
    {
        timeSource = new TimeSource();
        TimeManager.Instance.AddSource(timeSource);

        director = new ScrollersDirector(timeSource);
    }

    void Update()
    {
        director.updateScrollers();
    }

    void OnDestroy()
    {
        if (TimeManager.Instance != null)
            TimeManager.Instance.RemoveSource(timeSource);
    }
    #endregion
}
#endif