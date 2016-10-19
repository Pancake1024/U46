using System;
using System.Collections.Generic;
using SBS.Core;
using UnityEngine;

namespace SBS.UI
{
    public class UIAnimation
    {
        #region Protected members
        public string name;
        public OverlaysBatch batch;
        public List<Rect> frames;
        #endregion

        #region Ctors
        private UIAnimation()
        { }

        public UIAnimation(string _name, OverlaysBatch _batch, List<Rect> _frames)
        {
            name = _name;
            batch = _batch;
            frames = _frames;
        }
        #endregion
    }
}
