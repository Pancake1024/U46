using System;
using UnityEngine;
using SBS.Core;
using SBS.Math;
using System.Collections.Generic;

namespace SBS.UI
{
    public class AnimatedOverlay : UIElement
    {
        #region Protected members
        protected Overlay overlay;
        protected List<UIAnimation> animations;
        protected UIAnimation currentAnimation;

        protected float frameTimer;
        protected int currentFrame;
        protected bool looped;
        protected bool paused;
        protected float delay;
        protected float playTime;

        protected bool finished;
        protected Action<AnimatedOverlay> onUpdate;
        protected Action<AnimatedOverlay> onFinish;

        protected TimeSource timeSource = null;
        #endregion

        #region Public properties
        public OverlaysBatch Batch
        {
            get
            {
                return overlay.Batch;
            }
            set
            {
                overlay.Batch = value;
            }
        }

        public SBSMatrix4x4 Transform
        {
            get
            {
                return overlay.Transform;
            }
            set
            {
                overlay.Transform = value;
            }
        }

        public Rect ScreenRect
        {
            get
            {
                return overlay.ScreenRect;
            }
            set
            {
                overlay.ScreenRect = value;
            }
        }

        public Rect ImageRect
        {
            get
            {
                return overlay.ImageRect;
            }
        }

        public Color Color
        {
            get
            {
                return overlay.Color;
            }
            set
            {
                overlay.Color = value;
            }
        }
        /*
        public bool Clipped
        {
            get
            {
                overlay.Clipped
            }
        }
        */
        public bool Visibility
        {
            get
            {
                return overlay.Visibility;
            }
            set
            {
                overlay.Visibility = value;
            }
        }

        public int Depth
        {
            get
            {
                return overlay.Depth;
            }
            set
            {
                overlay.Depth = value;
            }
        }

        public UIAnimation CurrentAnimation
        {
            get
            {
                return currentAnimation;
            }
        }

        public int CurrentFrame
        {
            get
            {
                return currentFrame;
            }
            set
            {
                currentFrame = value;
                overlay.ImageRect = currentAnimation.frames[currentFrame];
            }
        }

        public Action<AnimatedOverlay> OnUpdate
        {
            get
            {
                return onUpdate;
            }
            set
            {
                onUpdate = value;
            }
        }

        public float PlayTime
        {
            get
            {
                return playTime;
            }
        }

        public List<UIAnimation> Animations
        {
            get
            {
                return animations;
            }
            set
            {
                animations = value;
            }
        }

        public TimeSource TimeSource
        {
            get
            {
                return timeSource;
            }
            set
            {
                timeSource = value;
            }
        }
        #endregion

        #region Ctors
        private AnimatedOverlay()
        { }

        public AnimatedOverlay(SBSMatrix4x4 _transform, Rect _scrRect, Color _color, UIAnimation _animation)
        {
            animations = new List<UIAnimation>();
            animations.Add(_animation);
            currentAnimation = null;
            overlay = new Overlay(_animation.batch, _transform, _scrRect, _animation.frames[0], _color);

            frameTimer = 0.0f;
            currentFrame = 0;
            looped = true;
            paused = false;
            delay = 0.0f;
            playTime = 0.0f;
            finished = false;
            onFinish = null;
            onUpdate = null;
        }
        #endregion

        #region Public methods
        public void SetScissorRect(string name, Rect rect)
        {
            overlay.SetScissorRect(name, rect);
        }

        public void SetScissorRectWithTransform(string name, Rect scissorRect, SBSMatrix4x4 newTransform)
        {
            overlay.SetScissorRectWithTransform(name, scissorRect, newTransform);
        }

        public void RemoveScissorRect()
        {
            overlay.RemoveScissorRect();
        }

        public string GetScissorRectName()
        {
            return overlay.GetScissorRectName();
        }

        public void Destroy()
        {
            overlay.Destroy();
            animations.Clear();
        }

        public UIAnimation GetAnimation(string name)
        {
            foreach (UIAnimation anim in animations)
            {
                if (anim.name == name)
                    return anim;
            }
            return null;
        }

        public void PlayAnimation(string animName, float _frameRate, bool _looped /*= true*/, Action<AnimatedOverlay> _onFinish/* = null*/)//, float _playTime = 0.0f)
        {
            currentFrame = 0;
            frameTimer = 0.0f;
            looped = _looped;
            delay = 0.0f;
            if (_frameRate > 0.0f)
                delay = 1.0f / _frameRate;
            onFinish = _onFinish;
            //playTime = _playTime;
            playTime = (null == timeSource ? TimeManager.Instance.MasterSource.TotalTime : timeSource.TotalTime);

            currentAnimation = GetAnimation(animName);

            if (currentAnimation != null)
            {
                if (overlay.Batch != currentAnimation.batch)
                    overlay.Batch = currentAnimation.batch;

                finished = delay <= 0.0f;

                overlay.ImageRect = currentAnimation.frames[currentFrame];
            }
        }

        public void Pause()
        {
            paused = true;
        }

        public void Play()
        {
            paused = false;
        }

        public void Stop()
        {
            currentAnimation = null;
        }

        public void Update()//float dt)
        {
            float dt = (null == timeSource ? TimeManager.Instance.MasterSource.DeltaTime : timeSource.DeltaTime);
            if ((currentAnimation != null) && (!paused) && (delay > 0.0f) && (looped || !finished))
            {
                frameTimer += dt;
                if (frameTimer > delay)
                {
                    frameTimer -= delay;
                    if (currentFrame == currentAnimation.frames.Count - 1)
                    {
                        finished = true;
                        if (looped)
                            currentFrame = 0;
                        else if (onFinish != null)
                            onFinish.Invoke(this);
                    }
                    else
                        currentFrame++;

                    overlay.ImageRect = currentAnimation.frames[currentFrame];
                }
            }
            if (onUpdate != null)
                onUpdate.Invoke(this);
        }
        #endregion
    }
}