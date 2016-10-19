using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

[AddComponentMenu("OnTheRun/UI/UIEnterExitAnimations")]
public class UIEnterExitAnimations : MonoBehaviour
{
    public static int activeAnimationsCounter = 0;

    public GameObject[] objectsToMove;
    public AnimationData[] animations;

    protected Dictionary<AnimationType, AnimationData> animationsDictionary;

    public enum AnimationType
    {
        EnterNavBar,
        ExitNavBar,
        FadeIn,
        FadeOut,
        EnterPage,
        ExitPage,
        EnterPopup,
        ExitPopup,
        FadeInPopup,
        FadeOutPopup,
        EnterRewardNavBar,
        ExitRewardNavBar,
        NextPageStepEnter,
        NextPageStepExit
    }

    [Serializable]
    public class AnimationData
    {
        public AnimationType type;
        public float endPoint;
        public float duration;
        public string coordinateToMove;
        public float delay;
        public iTween.EaseType easingType;
    }

    void Awake()
    {
        InitializeDictionary();
    }

    #region Messages
    void StopRunningAnimations()
    {
        for (int i = 0; i < objectsToMove.Length; ++i)
        {
            iTween.Stop(objectsToMove[i]);
        }
        ResetAnimationsCounter();
    }
    
    void ResetAnimationsCounter( )
    {
        activeAnimationsCounter = 0;
    }

    protected float additionalDelay = 0.0f;
    public void StartEnterExitAnimationWithParameters(AnimationType animType, float _additionalDelay)
    {
        additionalDelay = _additionalDelay;
        this.StartEnterExitAnimation(animType);
    }

    void StartEnterExitAnimation(AnimationType animType)
    {
        if (animType == AnimationType.EnterNavBar)
            ResetEnterExitAnimation(AnimationType.ExitNavBar);
        else if (animType == AnimationType.EnterPage)
            ResetEnterExitAnimation(AnimationType.ExitPage);
        else if (animType == AnimationType.EnterPopup)
            ResetEnterExitAnimation(AnimationType.ExitPopup);
        else if (animType == AnimationType.EnterRewardNavBar)
            ResetEnterExitAnimation(AnimationType.ExitRewardNavBar);
        else if (animType == AnimationType.FadeInPopup)
            ResetEnterExitAnimation(AnimationType.FadeOutPopup);
        else if (animType == AnimationType.NextPageStepEnter)
            ResetEnterExitAnimation(AnimationType.NextPageStepExit);

        AnimationData currAnimation;
        animationsDictionary.TryGetValue(animType, out currAnimation);

        if (currAnimation != null)
        {

            currAnimation.delay += additionalDelay;
            currAnimation.delay += OnTheRunUITransitionManager.delayFactor;

            for (int i = 0; i < objectsToMove.Length; ++i)
            {
                if (objectsToMove[i] == null)
                    continue;

                bool activateCounter = currAnimation.type == AnimationType.FadeOut || currAnimation.type == AnimationType.FadeOutPopup || currAnimation.type == AnimationType.ExitNavBar || currAnimation.type == AnimationType.ExitPage || currAnimation.type == AnimationType.ExitPopup;
                if (activateCounter)
                {
                    if (objectsToMove[i].activeInHierarchy)
                        ++activeAnimationsCounter;
                    //Debug.Log("*** start animation: " + objectsToMove[i].name);
                }

                string callbackFunction = "";
                GameObject callbackTarget = objectsToMove[i];
                if (callbackTarget.GetComponent<UIEnterExitAnimations>() == null)
                    callbackTarget = callbackTarget.transform.parent.gameObject;

                if (activateCounter)
                {
                    callbackFunction = "OnTweenEnd";
                }

                //FADE --> "easeOutCirc"
                //MOVE --> "easeOutQuad"
                if (currAnimation.type == AnimationType.FadeIn || currAnimation.type == AnimationType.FadeInPopup)
                {
                    if (currAnimation.coordinateToMove.Equals(""))
                        iTween.ValueTo(objectsToMove[i], iTween.Hash("from", 0.0f, "to", 1.0f, "time", currAnimation.duration, "delay", currAnimation.delay, "easetype", currAnimation.easingType, "onupdate", "setAlpha", "oncomplete", callbackFunction, "oncompletetarget", callbackTarget));               
                    else
                        iTween.ValueTo(objectsToMove[i], iTween.Hash("from", 0.0f, "to", currAnimation.endPoint, "time", currAnimation.duration, "delay", currAnimation.delay, "easetype", currAnimation.easingType, "onupdate", "setAlpha", "oncomplete", callbackFunction, "oncompletetarget", callbackTarget));               
                }
                else if (currAnimation.type == AnimationType.FadeOut || currAnimation.type == AnimationType.FadeOutPopup)
                {
                    if (currAnimation.coordinateToMove.Equals(""))
                        iTween.ValueTo(objectsToMove[i], iTween.Hash("from", 1.0f, "to", 0.0f, "time", currAnimation.duration, "delay", currAnimation.delay, "easetype", currAnimation.easingType, "onupdate", "setAlpha", "oncomplete", callbackFunction, "oncompletetarget", callbackTarget));
                    else
                        iTween.ValueTo(objectsToMove[i], iTween.Hash("from", currAnimation.endPoint, "to", 0.0f, "time", currAnimation.duration, "delay", currAnimation.delay, "easetype", currAnimation.easingType, "onupdate", "setAlpha", "oncomplete", callbackFunction, "oncompletetarget", callbackTarget));
                }
                else
                    iTween.MoveTo(objectsToMove[i], iTween.Hash(currAnimation.coordinateToMove, currAnimation.endPoint, "islocal", true, "delay", currAnimation.delay, "easeType", currAnimation.easingType, "time", currAnimation.duration, "oncomplete", callbackFunction, "oncompletetarget", callbackTarget));

                /*if (animType == AnimationType.EnterPopup || animType == AnimationType.ExitPopup)
                {
                    if (!Manager<UIManager>.Get().disableInputs)
                    {
                        Manager<UIManager>.Get().disableInputs = true;
                        this.StartCoroutine(this.EnablePlayerInput(currAnimation.duration));
                    }
                }*/
            }


            currAnimation.delay -= additionalDelay;
            additionalDelay = 0.0f;
        }
    }

    IEnumerator EnablePlayerInput(float duration)
    {
        yield return new WaitForSeconds(duration);

        Manager<UIManager>.Get().disableInputs = false;
    }

    void ResetEnterExitAnimation(AnimationType animType)
    {
        if (animationsDictionary == null) InitializeDictionary();

        AnimationData currAnimation;
        animationsDictionary.TryGetValue(animType, out currAnimation);
        if (currAnimation != null)
        {
            for (int i = 0; i < objectsToMove.Length; ++i)
            {
                if (objectsToMove[i] == null)
                    continue;

                if (currAnimation.type == AnimationType.FadeIn || currAnimation.type == AnimationType.FadeInPopup)
                {
                    iTween.ValueTo(objectsToMove[i], iTween.Hash("from", 1.0f, "to", 1.0f, "time", 0.0f, "easetype", "easeOutCirc", "onupdate", "setAlpha"));
                }
                else if (currAnimation.type == AnimationType.FadeOut || currAnimation.type == AnimationType.FadeOutPopup)
                    iTween.ValueTo(objectsToMove[i], iTween.Hash("from", 0.0f, "to", 0.0f, "time", 0.0f, "easetype", "easeOutCirc", "onupdate", "setAlpha"));
                else if (currAnimation.type == AnimationType.EnterPopup || currAnimation.type == AnimationType.ExitPopup)
                {
                    switch (currAnimation.coordinateToMove)
                    {
                        case "x": objectsToMove[i].transform.localPosition = new Vector3(currAnimation.endPoint, objectsToMove[i].transform.localPosition.y, objectsToMove[i].transform.localPosition.z); break;
                        case "y": objectsToMove[i].transform.localPosition = new Vector3(objectsToMove[i].transform.localPosition.x, currAnimation.endPoint, objectsToMove[i].transform.localPosition.z); break;
                    }
                }
                else
                {
                    switch (currAnimation.coordinateToMove)
                    {
                        case "x": objectsToMove[i].transform.localPosition = new Vector3(currAnimation.endPoint, objectsToMove[i].transform.localPosition.y, objectsToMove[i].transform.localPosition.z); break;
                        case "y": objectsToMove[i].transform.localPosition = new Vector3(objectsToMove[i].transform.localPosition.x, currAnimation.endPoint, objectsToMove[i].transform.localPosition.z); break;
                    }
                }
            }
        }
    }

    void OnTweenEnd()
    {
        --activeAnimationsCounter;
        //Debug.Log("*** stop animation: " + gameObject.name + " " + activeAnimationsCounter);
    }
    #endregion

    #region Utils
    protected void InitializeDictionary()
    {
        animationsDictionary = new Dictionary<AnimationType, AnimationData>();
        for (int i = 0; i < animations.Length; ++i)
        {
            animationsDictionary.Add(animations[i].type, animations[i]);
        }
    }

    public void setAlpha(float newAlpha)
    {
		//Debug.Log("SET ALPHA: " + newAlpha);
		//SBS.Miniclip.UtilsBindings.ConsoleLog("SET ALPHA: " + newAlpha);

        if (GetComponent<Renderer>() != null)
        {
            foreach (Material mObj in GetComponent<Renderer>().materials)
            {
                mObj.color = new Color(mObj.color.r, mObj.color.g, mObj.color.b, newAlpha);
            }
        }
        else
        {
            Color mObjColor = transform.GetComponent<UINineSlice>().color;
            mObjColor = new Color(mObjColor.r, mObjColor.g, mObjColor.b, newAlpha);
            transform.GetComponent<UINineSlice>().color = mObjColor;
            transform.GetComponent<UINineSlice>().ApplyParameters();
        }
    }
    #endregion
}