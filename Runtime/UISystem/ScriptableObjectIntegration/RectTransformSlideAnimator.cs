using System;
using System.Runtime.Serialization;
using UnityEngine;
using Zoroiscrying.CoreGameSystems.AnimatableObject;

namespace Zoroiscrying.CoreGameSystems.UISystem.ScriptableObjectIntegration
{
    [CreateAssetMenu(fileName = "RectTransformPositionAnimator", menuName = "Unity Core/Unity Panel System/SO Animator/RectTransformPosition")]
    public class RectTransformSlideAnimator : BaseComponentAnimatorSo<RectTransform>
    {
        [SerializeField] private Vector2 anchoredPositionFrom = Vector2.zero;
        [SerializeField] private Vector2 anchoredPositionTo = Vector2.zero;
        
        [HideInInspector] public Vector2 runtimeAnchorPositionFrom = Vector2.zero;
        [HideInInspector] public Vector2 runtimeAnchorPositionTo = Vector2.zero;

        private void OnEnable()
        {
            runtimeAnchorPositionFrom = anchoredPositionFrom;
            runtimeAnchorPositionTo = anchoredPositionTo;
        }
        
        public override void ChangeComponent(RectTransform component, float t)
        {
            component.anchoredPosition = Vector2.Lerp(runtimeAnchorPositionFrom, runtimeAnchorPositionTo, EasedT(t));
        }
    }
}