#if USE_UNISWF
using System;
using System.Collections.Generic;
using UnityEngine;
using pumpkin.display;
using pumpkin.events;

namespace SBS.uniSWF
{
	class ScrollingContainer : DisplayObjectContainer
	{
        protected ScrollingController mController = null;
        protected bool mDisableInputs = false;
        protected int reEnableInputsCounter = 0;

        public ScrollingContainer(ScrollingController controller)
        {
            mController = controller;
        }

        public void disableInputs()
        {
        	//Debug.Log("### disableInputs");
            mDisableInputs = true;
        }

        public void enableInputs()
        {
        	//Debug.Log("### enableInputs");
            //mDisableInputs = false;
        	if (mDisableInputs && 0 == reEnableInputsCounter)
                reEnableInputsCounter = 2;
        }

        public override DisplayObject getObjectUnderPoint(Vector2 point)
        {
            return mDisableInputs ? null : base.getObjectUnderPoint(point);
        }

        public override void updateBounds()
        {
            base.updateBounds();

            mController.setChildrenBounds(m_BoundsRect);
        }

        public override void updateFrame(CEvent e)
        {
            base.updateFrame(e);

            mController.updateFrame(e);

            if (mDisableInputs && reEnableInputsCounter > 0)
            {
            	if (0 == --reEnableInputsCounter)
            	    mDisableInputs = false;
            }
        }
    }
}
#endif