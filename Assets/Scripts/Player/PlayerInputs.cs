using UnityEngine;
using System.Collections;

[AddComponentMenu("Player/Inputs")]
public class PlayerInputs : MonoBehaviour
{
    const int MOUSE_LEFT = 0;

    #region Public members
    #endregion

    #region Protected members
    protected PlayerKinematics playerKinematics = null;
    protected Vector3 newDirection = Vector3.zero;

    protected bool inputsLocked = false; //inputs are locked (used during slide)
    protected bool stopInputs = false; //completely stop inputs

    protected bool isTimeout = false;
    protected bool isFinished = false;

    protected bool wasPaused = false;
    protected float pauseTimer = -1.0f;
    #endregion

    #region Public properties
    #endregion

    #region Public methods
    #endregion
        
    #region Unity callbacks
    void Update()
    {
        if (stopInputs && !OnTheRunTutorialManager.Instance.IsTutorialOnScreen)
            return;

        float now = TimeManager.Instance.MasterSource.TotalTime;

        if (Input.GetKeyDown(KeyCode.LeftArrow))// || Input.GetKey(KeyCode.A))
            gameObject.SendMessage("OnLeftInputDown");
        else if (Input.GetKeyUp(KeyCode.LeftArrow))// || Input.GetKeyUp(KeyCode.A))
        {
            OnTheRunTutorialManager.Instance.OnInputProcessed(OnTheRunTutorialManager.TutorialActions.TapLeft);
            gameObject.SendMessage("OnLeftInputUp");
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))// || Input.GetKey(KeyCode.D))
            gameObject.SendMessage("OnRightInputDown");
        else if (Input.GetKeyUp(KeyCode.RightArrow))// || Input.GetKeyUp(KeyCode.D))
        {
            OnTheRunTutorialManager.Instance.OnInputProcessed(OnTheRunTutorialManager.TutorialActions.TapRight);
            gameObject.SendMessage("OnRightInputUp");
        }
        
        newDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0.0f, Input.GetAxisRaw("Vertical"));
        if (!wasPaused)
        {
        }
        else
        {
            if (pauseTimer > 0f && now - pauseTimer > 0.1f)
            {
                wasPaused = false;
                pauseTimer = -1.0f;
            }
        }
    }

    void Awake()
    {
        playerKinematics = gameObject.GetComponent<PlayerKinematics>();
        stopInputs = true;
    }

    void Start()
    {
    }
    #endregion

    #region Messages
    void ReturnFromPause()
    {
        wasPaused = true;
        pauseTimer = TimeManager.Instance.MasterSource.TotalTime;
    }

    void OnReset()
    {
        stopInputs = false;
        isTimeout = false;
        isFinished = false;
    }

    void OnInitSession()
    {
        stopInputs = false;
    }

    void OnStartGameplay()
    {
        stopInputs = false;
    }

    void OnStartRunning()
    {
        stopInputs = false;
    }

    void OnSlideShow()
    {
        stopInputs = true;
    }

    void OnDead()
    {
        stopInputs = true;
    }
    void OnFinished()
    {
        Debug.Log("PlayerInputs - OnFinished");
        isFinished = true;
        stopInputs = true;
    }

    void OnTimeout()
    {
        isTimeout = true;
        OnFinished();
    }


    void OnPause()
    {
        stopInputs = true;
    }

    void OnResume()
    {
        stopInputs = false;
    }
   
    #endregion
}
