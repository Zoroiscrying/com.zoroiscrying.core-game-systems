using System;
using Zoroiscrying.ScriptableObjectCore;
using UnityEngine;
using Zoroiscrying.CoreGameSystems.CoreSystemUtility;

namespace Zoroiscrying.CoreGameSystems.AnimatableObject
{
    /// <summary>
    /// Use the ApplyValueChange method to apply the changed value to specific targets. 
    /// </summary>
    public class BaseTweenAnimatableObject : MonoBehaviour, IAnimatable
    {
        public float OnAnimateTime
        {
            get => onAnimateTime;
            set
            {
                onAnimateTime = value;
                if (_onTimer == null)
                {
                    _onTimer = new CustomTimer(onAnimateTime, OnTimerEndedOnce, loopAnimate);   
                }
                else
                {
                    _onTimer.ResetInitialTime(onAnimateTime);
                }
            }
        }
        
        public float OffAnimateTime
        {
            get => offAnimateTime;
            set
            {
                offAnimateTime = value;
                if (_offTimer == null)
                {
                    _offTimer = new CustomTimer(offAnimateTime, OnTimerEndedOnce, loopAnimate);   
                }
                else
                {
                    _offTimer.ResetInitialTime(offAnimateTime);
                }
            }
        }

        [SerializeField] private float onAnimateTime = 1f;
        [SerializeField] private float offAnimateTime = 1f;

        public EasingFunction.Ease AnimateEasingType
        {
            get => animateEasingType;
            set
            {
                animateEasingType = value;
                if (_easingInterpolator == null)
                {
                    _easingInterpolator = new EasingInterpolator(animateEasingType);   
                }
                else
                {
                    _easingInterpolator.ChangeEaseType(animateEasingType);
                }
            }
        }

        [SerializeField] private EasingFunction.Ease animateEasingType = EasingFunction.Ease.Linear;

        private CustomTimer _offTimer;
        private CustomTimer _onTimer;
        private EasingInterpolator _easingInterpolator;

        [SerializeField] protected bool loopAnimate = false;
        [SerializeField] protected bool pingPongAnimate = false;

        //[SerializeField] private bool shouldStartAnimateOnStart = false;
        
        private bool _shouldApplyValueChange = false;
        
        [SerializeField] private TimerTickMode tickMode;
        [SerializeField] private FloatVariableSO customScale;

        private Action _updateTimerMethod;

        private bool _fromTo = true;

        /// <summary>
        /// The t value of the animatable object, representing the state of the object correctly.
        /// </summary>
        protected float t = 0f;

        protected virtual void Start()
        {
            if (_onTimer == null)
            {
                _onTimer = new CustomTimer(onAnimateTime, OnTimerEndedOnce, loopAnimate);   
            }
            
            if (_offTimer == null)
            {
                _offTimer = new CustomTimer(offAnimateTime, OnTimerEndedOnce, loopAnimate);   
            }

            if (_easingInterpolator == null)
            {
                _easingInterpolator = new EasingInterpolator(animateEasingType);   
            }
            
            switch (tickMode)
            {
                case TimerTickMode.DeltaTime:
                    _updateTimerMethod = UpdateTimersOnceDeltaTime;
                    break;
                case TimerTickMode.UnscaledDeltaTime:
                    _updateTimerMethod = UpdateTimersOnceUnscaledDeltaTime;
                    break;
                case TimerTickMode.CustomScaledDeltaTime:
                    _updateTimerMethod = UpdateTimerOnceCustomScaledDeltaTime;
                    break;
                default:
                    _updateTimerMethod = UpdateTimersOnceDeltaTime;
                    break;
            }
        }

        protected virtual void Update()
        {
            if (_shouldApplyValueChange)
            {
                // first update the timer, the callback will be called if the timer ends.
                _updateTimerMethod();
                
                // update the values to the target.
                ApplyChangedValue();
            }
        }

        public void ChangeAnimateProperties(
            float onAnimTime = 1f,
            float offAnimTime = 1f,
            EasingFunction.Ease easeType = EasingFunction.Ease.Linear, bool loopAnim = false, bool pingPongAnim = false)
        {
            onAnimateTime = onAnimTime;
            offAnimateTime = offAnimTime;
            animateEasingType = easeType;
            loopAnimate = loopAnim;
            pingPongAnimate = pingPongAnim;
            _onTimer.ResetInitialTime(onAnimateTime);
            _offTimer.ResetInitialTime(offAnimTime);
            _easingInterpolator.ChangeEaseType(easeType);
            switch (tickMode)
            {
                case TimerTickMode.DeltaTime:
                    _updateTimerMethod = UpdateTimersOnceDeltaTime;
                    break;
                case TimerTickMode.UnscaledDeltaTime:
                    _updateTimerMethod = UpdateTimersOnceUnscaledDeltaTime;
                    break;
                case TimerTickMode.CustomScaledDeltaTime:
                    _updateTimerMethod = UpdateTimerOnceCustomScaledDeltaTime;
                    break;
                default:
                    _updateTimerMethod = UpdateTimersOnceDeltaTime;
                    break;
            }

            _shouldApplyValueChange = true;
        }

        public virtual void FinishBehavior()
        {
            if (_fromTo)
            {
                _onTimer.FinishTimer();
            }
            else
            {
                _offTimer.FinishTimer();
            }
            ApplyChangedValue();
        }

        /// <summary>
        /// Auto set the state to From.
        /// </summary>
        public void StartFromToBehavior()
        {
            _shouldApplyValueChange = true;
            _onTimer.InitializeTimer();
            _fromTo = true;
            ApplyChangedValue();
        }

        /// <summary>
        /// Auto set the state to To.
        /// </summary>
        public void StartToFromBehavior()
        {
            _shouldApplyValueChange = true;
            _offTimer.InitializeTimer();
            _fromTo = false;
            ApplyChangedValue();
        }

        public virtual void ApplyChangedValue()
        {
            t = _fromTo ? _onTimer.NormalizedValue() : 1.0f - _offTimer.NormalizedValue();
        }

        /// <summary>
        /// This functions is called when the timer ends once, 
        /// </summary>
        protected virtual void OnTimerEndedOnce()
        {
            _shouldApplyValueChange = false;

            // EXAMPLE: the timer finishes with the fromTo set to true;
            // If pingPongAnimate is true, then fromTo is false, else is true.
            if (pingPongAnimate)
            {
                _fromTo = !_fromTo;
            }
            
            // If loopAnimate is true, then fromTo is still from to, if pingPongAnimate is false.
            // The timer resets and runs again.
            if (loopAnimate)
            {
                if (_fromTo)
                {
                    StartFromToBehavior();
                }
                else
                {
                    StartToFromBehavior();
                }
            }
        }
        
        private void UpdateTimersOnceDeltaTime()
        {
            if (_fromTo)
            {
                _onTimer.Tick(Time.deltaTime);
            }
            else
            {
                _offTimer.Tick(Time.deltaTime);
            }
        }
        
        private void UpdateTimersOnceUnscaledDeltaTime()
        {
            if (_fromTo)
            {
                _onTimer.Tick(Time.unscaledDeltaTime);
            }
            else
            {
                _offTimer.Tick(Time.unscaledDeltaTime);
            }
        }

        private void UpdateTimerOnceCustomScaledDeltaTime()
        {
            UpdateTimerOnceCustomScaledDeltaTime(customScale.Value);
        }
        
        private void UpdateTimerOnceCustomScaledDeltaTime(float scale)
        {
            if (_fromTo)
            {
                _onTimer.Tick(Time.unscaledDeltaTime * scale);
            }
            else
            {
                _offTimer.Tick(Time.unscaledDeltaTime * scale);
            }
        }
    }
}