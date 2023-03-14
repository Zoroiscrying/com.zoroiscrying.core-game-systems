using System;
using UnityEngine;

namespace Zoroiscrying.CoreGameSystems.AnimatableObject
{
    public class BaseComponentAnimatorSo<T> : ScriptableObject, IComponentAnimatable<T>
    {
        [SerializeField] private EasingFunction.Ease easeType = EasingFunction.Ease.EaseInOutExpo;
        
        private EasingFunction.Function _easeFunction = null;

        protected EasingFunction.Function EaseFunction
        {
            get
            {
                if (_easeFunction == null)
                {
                    _easeFunction = EasingFunction.GetEasingFunction(easeType);
                }

                if (_easeFunction == null)
                {
                    return EasingFunction.Linear;
                }
                
                return _easeFunction;
            }
        }

        protected float EasedT(float t) => EaseFunction(0, 1, t);

        public virtual void ChangeComponent(T component, float t)
        {
            throw new NotImplementedException("Not implemented.");
        }
    }
}