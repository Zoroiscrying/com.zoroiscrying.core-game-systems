using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zoroiscrying.CoreGameSystems.AnimatableObject
{
    public class BaseAnimatorOrganizer<TC> : ScriptableObject
    {
        [SerializeField] private List<BaseComponentAnimatorSo<TC>> animators;

        public void UpdateComponentViaAnimator(TC component, float t)
        {
            foreach (var animator in animators)
            {
                animator.ChangeComponent(component, t);
            }
        }
    }
}