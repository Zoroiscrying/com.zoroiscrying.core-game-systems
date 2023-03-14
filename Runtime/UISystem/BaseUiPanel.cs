using System;
using System.Collections.Generic;
using UnityEngine;
using Zoroiscrying.CoreGameSystems.AnimatableObject;
using Zoroiscrying.CoreGameSystems.AudioSystem;
using Zoroiscrying.CoreGameSystems.AudioSystem.ScriptableObjectIntegration;
using Zoroiscrying.CoreGameSystems.CoreSystemUtility;
using Zoroiscrying.CoreGameSystems.UISystem.ScriptableObjectIntegration;

namespace Zoroiscrying.CoreGameSystems.UISystem
{
    public enum PanelMotionType
    {
        Instant,
        Animator,
        ScriptableTween,
        AnimatorAndScriptableTween
    }

    public enum CustomPanelApplyChangeAction
    {
        ActivateDeactivate,
        SetAlphaTo01,
        SetScaleTo01,
    }
    
    /// <summary>
    /// Action types that will pass to other child panels
    /// </summary>
    public enum PanelActionType
    {
        Null,
        TurnOn,
        TurnOff,
        ForceFinish,
    }
    
    [Serializable]
    public struct PanelActionPair
    {
        public BaseUiPanel baseUiPanel;
        public PanelActionType panelActionType;
    }
    
    [RequireComponent(typeof(Animator), typeof(RectTransform), typeof(CanvasGroup))]
    public abstract class BaseUiPanel : BaseTweenAnimatableObject
    {
        protected event Action onPanelBeginToOpen = delegate {  };
        protected event Action onPanelBeginToClose = delegate {  };
        protected event Action onPanelOpened = delegate {  };
        protected event Action onPanelClosed = delegate {  };
        
        [SerializeField] private PanelMotionType onMotionType = PanelMotionType.Instant;
        [SerializeField] private PanelMotionType offMotionType = PanelMotionType.Instant;

        public PanelTypeSO PanelTypeSo => panelType;
        [SerializeField] private PanelTypeSO panelType;

        [SerializeField] private bool turnPanelOnAtStart = false;

        public bool IsOn => m_isOn;
        private bool m_isOn = false;
        
        private Animator _animator;
        private bool _checkAnimator = false;

        private bool _checkTween = false;
        
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        
        private static readonly int On = Animator.StringToHash("On");
        private static readonly int Enabled = Animator.StringToHash("Enabled");

        public bool ShouldDoOnMotion => _shouldDoOnMotion;
        private bool _shouldDoOnMotion = false;
        
        private bool _animating = false;

        private bool _delayedTurnOn = false;
        private bool _delayedTurnOff = false;

        //public int PanelLevelFromRoot
        //{
        //    get => panelLevelFromRoot;
        //    private set => panelLevelFromRoot = value;
        //}
        //
        ///// <summary>
        ///// 0 represents root panels, that are persistent panels
        ///// other numbers greater than 0 represent child panels
        ///// </summary>
        //private int panelLevelFromRoot = -1;
        
        // Set by editor
        public BaseUiPanel ParentPanel => parentPanel;
        protected BaseUiPanel parentPanel = null;
        public List<BaseUiPanel> ChildPanels => childPanels;
        protected List<BaseUiPanel> childPanels = new List<BaseUiPanel>();
        public bool IsChild => childPanels.Count == 0;

        // Tween animation settings
        [SerializeField] private bool useTweenOrganizerSO = false;
        [SerializeField] private CustomPanelApplyChangeAction customPanelAction;
        [SerializeField] private RectTransformAnimatorOrganizer rectTransformAnimatorOrganizer;
        [SerializeField] private CanvasGroupAnimatorOrganizer canvasGroupAnimatorOrganizer;
        
        // Audio integration
        private PlayAudioEventRaiser _audioEventRaiser;
        private AudioObjectSo _onPanelOpenAudio;
        private AudioObjectSo _onPanelCloseAudio;
        private bool _initializedAudioIntegration;
        public bool CustomAudioIntegration => customAudioIntegration;
        [SerializeField] private bool customAudioIntegration;
        
        // Turn Panel Action Integration
        private TurnPanelEventRaiser _turnPanelEventRaiser;
        [SerializeField] private List<PanelActionPair> panelActionPairsBeginToOpen 
            = new List<PanelActionPair>();
        [SerializeField] private List<PanelActionPair> panelActionPairsBeginToClose 
            = new List<PanelActionPair>();
        [SerializeField] private List<PanelActionPair> panelActionPairsOpened 
            = new List<PanelActionPair>();
        [SerializeField] private List<PanelActionPair> panelActionPairsClosed 
            = new List<PanelActionPair>();

        private bool _initializedActionPairIntegration;
        

        #region Unity Function

        protected virtual void OnEnable()
        {
            if (onMotionType == PanelMotionType.Animator || 
                offMotionType == PanelMotionType.Animator ||
                onMotionType == PanelMotionType.AnimatorAndScriptableTween || 
                offMotionType == PanelMotionType.AnimatorAndScriptableTween)
            {
                CheckAnimatorIntegrity();   
            }
            else
            {
                _animator = this.GetComponent<Animator>();
                _animator.enabled = false;
            }

            CheckCanvasGroupIntegrity();
            
            CheckRectTransformIntegrity();
        }

        protected override void Start()
        {
            base.Start();

            if (onMotionType == PanelMotionType.Animator || onMotionType == PanelMotionType.AnimatorAndScriptableTween)
            {
                _animator.SetBool(Enabled, true);
            }

            if (offMotionType == PanelMotionType.Animator || offMotionType == PanelMotionType.AnimatorAndScriptableTween)
            {
                _animator.SetBool(Enabled, true);
            }
            
            m_isOn = !turnPanelOnAtStart;
            AnimatePanel(turnPanelOnAtStart);
        }

        protected override void Update()
        {
            base.Update();

            if (_checkAnimator && _checkTween && _animating)
            {
                var targetState = _shouldDoOnMotion ? "On" : "Off";
                var targetTValue = _shouldDoOnMotion ? 1f : 0f;
                var finishedTransitioning = Math.Abs(t - targetTValue) < 0.0001f &&
                                            _animator.GetCurrentAnimatorStateInfo(0).IsName(targetState) && 
                                            _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1;
                
                if (!finishedTransitioning) {return;}
                
                FinishBehavior();
            }

            if (_checkAnimator && !_checkTween && _animating)
            {
                var targetState = _shouldDoOnMotion ? "On" : "Off";
                var finishedTransitioning = 
                    _animator.GetCurrentAnimatorStateInfo(0).IsName(targetState) &&
                    _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1;
                if (!finishedTransitioning) {return;}
            
                FinishBehavior();
            }

            if (_checkTween && !_checkAnimator && _animating)
            {
                var targetTValue = _shouldDoOnMotion ? 1f : 0f;
                var finishedTransitioning = Math.Abs(t - targetTValue) < 0.0001f;
                
                if (!finishedTransitioning) {return;}
                
                FinishBehavior();
            }
        }

        #endregion

        #region Public Function

        public void InjectEventRaiser(TurnPanelEventRaiser turnPanelEventRaiser)
        {
            _turnPanelEventRaiser = turnPanelEventRaiser;
        }

        public void InjectPanelActions(List<PanelActionPair> actionPairs, PanelEventType eventType)
        {
            switch (eventType)
            {
                case PanelEventType.OnOpened:
                    panelActionPairsOpened.AddRange(actionPairs);
                    break;
                case PanelEventType.OnClosed:
                    panelActionPairsClosed.AddRange(actionPairs);
                    break;
                case PanelEventType.OnBeginOpen:
                    panelActionPairsBeginToOpen.AddRange(actionPairs);
                    break;
                case PanelEventType.OnBeginClose:
                    panelActionPairsBeginToClose.AddRange(actionPairs);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, "Not implemented.");
            }

            if (!_initializedActionPairIntegration)
            {
                _initializedActionPairIntegration = true;
                onPanelBeginToOpen += TurnPanelBeginToOpen;
                onPanelBeginToClose += TurnPanelBeginToClose;
                onPanelOpened += TurnPanelOpened;
                onPanelClosed += TurnPanelClosed;   
            }
        }

        public void InjectEventRaiser(PlayAudioEventRaiser audioEventRaiser)
        {
            _audioEventRaiser = audioEventRaiser;
            if (_audioEventRaiser != null && _onPanelOpenAudio != null & _onPanelCloseAudio != null && !_initializedAudioIntegration)
            {
                _initializedAudioIntegration = true;
                onPanelBeginToOpen += AudioOnPanelBeginToOpen;
                onPanelBeginToClose += AudioOnPanelBeginToClose;
            }
        }

        public void InjectAudioSo(AudioObjectSo panelOpenAudio, AudioObjectSo panelCloseAudio)
        {
            _onPanelOpenAudio = panelOpenAudio;
            _onPanelCloseAudio = panelCloseAudio;
            if (_audioEventRaiser != null && _onPanelOpenAudio != null & _onPanelCloseAudio != null && !_initializedAudioIntegration)
            {
                _initializedAudioIntegration = true;
                onPanelBeginToOpen += AudioOnPanelBeginToOpen;
                onPanelBeginToClose += AudioOnPanelBeginToClose;
            }
        }
        
        public void InjectAudioSo(PlayAudioEventRaiser audioEventRaiser, AudioObjectSo panelOpenAudio, AudioObjectSo panelCloseAudio)
        {
            _audioEventRaiser = audioEventRaiser;
            _onPanelOpenAudio = panelOpenAudio;
            _onPanelCloseAudio = panelCloseAudio;
            if (_audioEventRaiser != null && _onPanelOpenAudio != null & _onPanelCloseAudio != null && !_initializedAudioIntegration)
            {
                _initializedAudioIntegration = true;
                onPanelBeginToOpen += AudioOnPanelBeginToOpen;
                onPanelBeginToClose += AudioOnPanelBeginToClose;
            }
        }
        
        public bool AnimatePanel(bool turnOn)
        {
            if (parentPanel != null && !parentPanel.gameObject.activeSelf)
            {
                return false;
            }
            
            if (turnOn)
            {
                switch (onMotionType)
                {
                    case PanelMotionType.Instant:
                        return TurnPanelInstant(true);
                    case PanelMotionType.Animator:
                        return TurnPanelAnimator(true);
                    case PanelMotionType.ScriptableTween:
                        return TurnPanelTween(true);
                    case PanelMotionType.AnimatorAndScriptableTween:
                        return TurnPanelAnimatorAndScriptableTween(true);
                    default:
                        return TurnPanelInstant(true);
                }
            }
            else
            {
                switch (offMotionType)
                {
                    case PanelMotionType.Instant:
                        return TurnPanelInstant(false);
                    case PanelMotionType.Animator:
                        return TurnPanelAnimator(false);
                    case PanelMotionType.ScriptableTween:
                        return TurnPanelTween(false);
                    case PanelMotionType.AnimatorAndScriptableTween:
                        return TurnPanelAnimatorAndScriptableTween(false);
                    default:
                        return TurnPanelInstant(false);
                }
            }
        }

        public override void ApplyChangedValue()
        {
            base.ApplyChangedValue();
            if (_checkTween)
            {
                // Apply the changed value to the organizer.
                if (useTweenOrganizerSO)
                {
                    ApplyAnimatorOrganizers();
                }
                else
                {
                    switch (customPanelAction)
                    {
                        case CustomPanelApplyChangeAction.ActivateDeactivate:
                            //Do nothing
                            break;
                        case CustomPanelApplyChangeAction.SetAlphaTo01:
                            _canvasGroup.alpha = t;
                            break;
                        case CustomPanelApplyChangeAction.SetScaleTo01:
                            _rectTransform.localScale = t * Vector3.one;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
        
        public override void FinishBehavior()
        {
            base.FinishBehavior();
            
            _checkTween = false;
            _checkAnimator = false;
            _animating = false;

            if (_shouldDoOnMotion)
            {
                m_isOn = true;
                InvokeFinishedEventBasedOnShouldTurnOn(m_isOn);
            }
            else
            {
                m_isOn = false;
                InvokeFinishedEventBasedOnShouldTurnOn(m_isOn);
                if (!_delayedTurnOn)
                {
                    gameObject.SetActive(false);   
                }
            }

            _delayedTurnOff = false;
            _delayedTurnOn = false;
        }

        public void ShouldTurnOffAfterwards()
        {
            _delayedTurnOff = true;
        }

        public void ShouldTurnOnAfterwards()
        {
            _delayedTurnOn = true;
        }

        public void RegisterChildPanel(UiChildPanel childPanel)
        {
            if (!childPanels.Contains(childPanel))
            {
                childPanels.Add(childPanel);   
            }
        }
        
        public void UnregisterChildPanel(UiChildPanel childPanel)
        {
            if (childPanels.Contains(childPanel))
            {
                childPanels.Remove(childPanel);   
            }
        }
        
        public void RegisterOnPanelBeginToOpen(Action action)
        {
            onPanelBeginToOpen += action;
        }
        
        public void UnregisterOnPanelBeginToOpen(Action action)
        {
            onPanelBeginToOpen -= action;
        }
        
        public void RegisterOnPanelBeginToClose(Action action)
        {
            onPanelBeginToClose += action;
        }
        
        public void UnregisterOnPanelBeginToClose(Action action)
        {
            onPanelBeginToClose -= action;
        }
        
        public void RegisterOnPanelOpened(Action action)
        {
            onPanelOpened += action;
        }
        
        public void UnregisterOnPanelOpened(Action action)
        {
            onPanelOpened -= action;
        }
        
        public void RegisterOnPanelClosed(Action action)
        {
            onPanelClosed += action;
        }
        
        public void UnregisterOnPanelClosed(Action action)
        {
            onPanelClosed -= action;
        }

        #endregion

        #region Private Function

        private void DoChildPanelAction(BaseUiPanel panel, PanelActionType actionType)
        {
            switch (actionType)
            {
                case PanelActionType.TurnOn:
                    panel.FinishBehavior();
                    panel.AnimatePanel(true);
                    break;
                case PanelActionType.TurnOff:
                    panel.FinishBehavior();
                    panel.AnimatePanel(false);
                    break;
                case PanelActionType.ForceFinish:
                    panel.FinishBehavior();
                    break;
                default:
                    break;
            }
        }
        private void TurnPanelBeginToOpen()
        {
            foreach (var childPanelActionPair in panelActionPairsBeginToOpen)
            {
                DoChildPanelAction(childPanelActionPair.baseUiPanel, childPanelActionPair.panelActionType);
            }
        }

        private void TurnPanelBeginToClose()
        {
            foreach (var childPanelActionPair in panelActionPairsBeginToClose)
            {
                DoChildPanelAction(childPanelActionPair.baseUiPanel, childPanelActionPair.panelActionType);
            }
        }

        private void TurnPanelOpened()
        {
            foreach (var childPanelActionPair in panelActionPairsOpened)
            {
                DoChildPanelAction(childPanelActionPair.baseUiPanel, childPanelActionPair.panelActionType);
            }
        }

        private void TurnPanelClosed()
        {
            foreach (var childPanelActionPair in panelActionPairsClosed)
            {
                DoChildPanelAction(childPanelActionPair.baseUiPanel, childPanelActionPair.panelActionType);
            }
        }
        
        private void AudioOnPanelBeginToOpen()
        {
            _audioEventRaiser.RaiseEvent(new PlayAudioSetting()
            {
                audioAction = AudioSystemPersistentManager.AudioJobAction.RESTART,
                audioObject = _onPanelOpenAudio,
                delay = 0f,
                fade = false,
                fadeTime = 0f
            });
        }

        private void AudioOnPanelBeginToClose()
        {
            _audioEventRaiser.RaiseEvent(new PlayAudioSetting()
            {
                audioAction = AudioSystemPersistentManager.AudioJobAction.RESTART,
                audioObject = _onPanelCloseAudio,
                delay = 0f,
                fade = false,
                fadeTime = 0f
            });
        }
        
        /// <summary>
        /// This virtual function opens up possibility for more organizers for other class that derive the base ui panel.
        /// </summary>
        protected virtual void ApplyAnimatorOrganizers()
        {
            if (canvasGroupAnimatorOrganizer)
            {
                canvasGroupAnimatorOrganizer.UpdateComponentViaAnimator(_canvasGroup, t);   
            }

            if (rectTransformAnimatorOrganizer)
            {
                rectTransformAnimatorOrganizer.UpdateComponentViaAnimator(_rectTransform, t);   
            }
        }

        private bool TurnPanelInstant(bool turnOn)
        {
            if (m_isOn == turnOn && !_animating)
            {
                return false;
            }

            _shouldDoOnMotion = turnOn;
            InvokeBeginEventBasedOnTurnOn(turnOn);
            FinishBehavior();
            if (!turnOn)
            {
                gameObject.SetActive(false);   
            }
            return true;
        }

        private bool TurnPanelAnimator(bool turnOn)
        {
            if (m_isOn == turnOn && !_animating)
            {
                return false;
            }
            
            if (_animating)
            {
                return false;
            }

            _animating = true;
            _animator.SetBool(On, turnOn);
            _animator.SetBool(Enabled, true);
            _checkAnimator = true;
            _checkTween = false;
            _shouldDoOnMotion = turnOn;
            
            InvokeBeginEventBasedOnTurnOn(turnOn);
            
            return true;
        }

        private bool TurnPanelTween(bool turnOn)
        {
            // The panel is not animating, and the instruction remains the same.
            if (m_isOn == turnOn && !_animating)
            {
                return false;
            }

            if (_animating)
            {
                return false;
            }

            if (turnOn)
            {
                StartFromToBehavior();
            }
            else StartToFromBehavior();
            
            _animating = true;
            _checkAnimator = false;
            _checkTween = true;
            _shouldDoOnMotion = turnOn;
            
            InvokeBeginEventBasedOnTurnOn(turnOn);
            
            return true;
        }

        private bool TurnPanelAnimatorAndScriptableTween(bool turnOn)
        {
            // The panel is not animating, and the instruction remains the same.
            if (m_isOn == turnOn && !_animating)
            {
                return false;
            }
            
            if (_animating)
            {
                return false;
            }
            
            if (turnOn)
            {
                StartFromToBehavior();
            }
            else StartToFromBehavior();
            
            _animating = true;
            _animator.SetBool(On, turnOn);
            _animator.SetBool(Enabled, true);
            _checkAnimator = true;
            _checkTween = true;
            _shouldDoOnMotion = turnOn;
            
            InvokeBeginEventBasedOnTurnOn(turnOn);

            return true;
        }

        private void InvokeFinishedEventBasedOnShouldTurnOn(bool shouldTurnOn)
        {
            // The panel opened
            if (shouldTurnOn)
            {
                onPanelOpened.Invoke();
                if (_delayedTurnOff)
                {
                    AnimatePanel(false);
                }
            }
            // The panel closed
            else
            {
                onPanelClosed.Invoke();
                if (_delayedTurnOn)
                {
                    AnimatePanel(true);
                }
            }
        }
        
        private void InvokeBeginEventBasedOnTurnOn(bool turnOn)
        {
            if (turnOn)
            {
                onPanelBeginToOpen.Invoke();
            }
            else
            {
                onPanelBeginToClose.Invoke();
            }
        }

        /// <summary>
        /// This function is called before all the values are set to target state.
        /// </summary>
        protected override void OnTimerEndedOnce()
        {
            base.OnTimerEndedOnce();
            //Debug.Log("Timer ends once. " + "Current Panel Should do: " + _shouldDoOnMotion);
        }

        private void CheckAnimatorIntegrity()
        {
            _animator = this.GetComponent<Animator>();
            if (!_animator)
            {
                LogManager.LogWarning("The animator for page" + this.gameObject.name + " is not added! \n " +
                           "A raw animator is going to be added and perform no function, please fix the issue.");
                _animator = this.gameObject.AddComponent<Animator>();
            }

            if (!_animator.GetBool(On))
            {
                LogManager.LogWarning("The animator for page" + this.gameObject.name + "has no 'On' parameter.");
            }
        }

        private void CheckCanvasGroupIntegrity()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                LogManager.Log("No Canvas Group for page: " + this.panelType.Value +
                               ", this can lead to no effects when page get animated.");
            }
        }
        
        private void CheckRectTransformIntegrity()
        {
            _rectTransform = GetComponent<RectTransform>();
            if (_rectTransform == null)
            {
                LogManager.Log("No Rect Transform for page: " + this.panelType.Value +
                               ", this can lead to no effects when page get animated.");
            }
        }
        
        #endregion
        
    }
}