using System;
using UnityEngine;
using Zoroiscrying.CoreGameSystems.AnimatableObject;

namespace Zoroiscrying.CoreGameSystems.UISystem.ScriptableObjectIntegration
{
    [CreateAssetMenu(fileName = "CanvasGroupAlphaAnimator", menuName = "Unity Core/Unity Panel System/SO Animator/CanvasGroupAlpha")]
    public class CanvasGroupAlphaAnimator : BaseComponentAnimatorSo<CanvasGroup>
    { 
        [SerializeField] private float offAlpha = 0f;
        [SerializeField] private float onAlpha = 1f;

        [HideInInspector] public float runtimeOffAlpha = 0f;
        [HideInInspector] public float runtimeOnAlpha = 1f;

        private void OnEnable()
        {
            runtimeOffAlpha = offAlpha;
            runtimeOnAlpha = onAlpha;
        }

        public override void ChangeComponent(CanvasGroup component, float t)
        {
            component.alpha = Mathf.Lerp(runtimeOffAlpha, runtimeOnAlpha, EasedT(t));
        }
    }
}