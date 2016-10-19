using System;
using System.Collections.Generic;
using UnityEngine;
using SBS.Core;

namespace SBS.UI
{
    public class UIController : FSM.Object<UIController, int>
    {
        #region Protected members
        protected Dictionary<int, Function> onGUIExecDict = new Dictionary<int,Function>();
        protected Function onGUIExec = null;
        #endregion

        #region Public properties
        new public int State
        {
            set
            {
                base.State = value;

                onGUIExec = null;
                onGUIExecDict.TryGetValue(value, out onGUIExec);
            }

            get
            {
                return base.State;
            }
        }
        #endregion

        #region Public methods
        public void AddState(int key, Function onEnter, Function onExec, Function onGUIExec, Function onExit)
        {
            base.AddState(key, onEnter, onExec, onExit);

            onGUIExecDict.Add(key, onGUIExec);
        }

        public void GUIUpdate()
        {
            if (null == onGUIExec)
                return;

            onGUIExec(this, timeSource.TotalTime);
        }
        #endregion
    }
}
