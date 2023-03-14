using System;
using System.Collections.Generic;
using Zoroiscrying.ScriptableObjectCore;
using UnityEngine;

namespace Zoroiscrying.CoreGameSystems.CoreSystemUtility
{
    public enum TimerTickMode
    {
        DeltaTime,
        UnscaledDeltaTime,
        CustomScaledDeltaTime
    }
    
    public class TimerManager : MonoSingleton<TimerManager>
    {
        private List<CustomTimer> _timers = new List<CustomTimer>();
        [SerializeField] private TimerTickMode tickMode;
        [SerializeField] private FloatVariableSO customScale;

        private void Start()
        {
            tickMode = TimerTickMode.DeltaTime;
        }

        private void Update()
        {
            switch (tickMode)
            {
                case TimerTickMode.DeltaTime:
                    UpdateTimersOnceDeltaTime();
                    break;
                case TimerTickMode.UnscaledDeltaTime:
                    UpdateTimersOnceUnscaledDeltaTime();
                    break;
                case TimerTickMode.CustomScaledDeltaTime:
                    if (customScale != null)
                    {
                        UpdateTimerOnceCustomScaledDeltaTime(customScale.Value);   
                    }
                    else
                    {
                        Debug.LogWarning("Custom Scale Variable not assigned!");
                        UpdateTimersOnceDeltaTime();
                    }
                    break;
                default:
                    UpdateTimersOnceDeltaTime();
                    break;
            }
            
        }

        private void UpdateTimersOnceDeltaTime()
        {
            foreach (var timer in _timers)
            {
                timer.Tick(Time.deltaTime);
            }
        }
        
        private void UpdateTimersOnceUnscaledDeltaTime()
        {
            foreach (var timer in _timers)
            {
                timer.Tick(Time.unscaledDeltaTime);
            }
        }

        private void UpdateTimerOnceCustomScaledDeltaTime(float scale)
        {
            foreach (var timer in _timers)
            {
                timer.Tick(Time.unscaledDeltaTime * scale);
            }
        }
    }
}