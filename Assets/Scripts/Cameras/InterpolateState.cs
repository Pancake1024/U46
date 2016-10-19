using System;
using System.Collections.Generic;
using UnityEngine;


public class InterpolateState : MonoBehaviour
{
    protected GameObject player;   
    protected FiniteStateMachine fsm;
    protected string prevStateExecMsg;
    protected string nextStateExecMsg;
    protected int nextStateId;
    protected float interpolateInTime;
    protected float lerpTime;
    protected float stateEnterTime;

    public int NextStateId
    {
        get
        {
            return nextStateId;
        }
        set
        {
            nextStateId = value;
        }
    }

    public float InterpolateInTime
    {
        get
        {
            return interpolateInTime;
        }
        set
        {
            interpolateInTime = value;
        }
    }

    void Awake()
    {
        fsm = gameObject.GetComponent<FiniteStateMachine>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    public void OnInterpolateStateEnter(float time)
    {
        prevStateExecMsg = fsm.states[fsm.PrevState].onExecMessage;
        nextStateExecMsg = fsm.states[nextStateId].onExecMessage;

        lerpTime = 0.0f;

        stateEnterTime = TimeManager.Instance.MasterSource.TotalTime;
        Debug.Log("interpolate state enter" + prevStateExecMsg + "next" + nextStateExecMsg);
    }

    public void OnInterpolateStateExec(float time)
    {
        if (TimeManager.Instance.MasterSource.IsPaused)
            return;

        float dt = TimeManager.Instance.MasterSource.DeltaTime;

        lerpTime = (time - stateEnterTime) / interpolateInTime; 
        lerpTime = Mathf.Clamp(lerpTime, 0.0f, 1.0f);
        //Debug.Log("LERP" + lerpTime + "prev" + prevStateExecMsg);

        gameObject.SendMessage(prevStateExecMsg, time);
        Vector3 afterPrevStatePos = transform.position;
        Quaternion afterPrevStateRot = transform.rotation;

        gameObject.SendMessage(nextStateExecMsg, time);
        //Vector3 afterNextStatePos = transform.position;
        //Quaternion afterNextStateRot = transform.rotation;

       
        transform.position = Vector3.Lerp(afterPrevStatePos, transform.position, lerpTime);
        transform.rotation = Quaternion.Slerp(afterPrevStateRot, transform.rotation, lerpTime);



        if (lerpTime >= 1.0f)   //finished interpolation
        {
            fsm.State = nextStateId;
            gameObject.SendMessage("NoFirstFrame", SendMessageOptions.DontRequireReceiver);
            //Debug.Log("FINISHED AT" + (time -stateEnterTime) );
        }

       
    }

    public void OnInterpolateStateExit(float time)
    {

    }
}
