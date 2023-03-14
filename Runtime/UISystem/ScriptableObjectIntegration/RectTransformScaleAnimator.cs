using UnityEngine;
using Zoroiscrying.CoreGameSystems.AnimatableObject;

namespace Zoroiscrying.CoreGameSystems.UISystem.ScriptableObjectIntegration
{
    [CreateAssetMenu(fileName = "RectTransformScaleAnimator", menuName = "Unity Core/Unity Panel System/SO Animator/RectTransformScale")]
    public class RectTransformScaleAnimator : BaseComponentAnimatorSo<RectTransform>
    {
        [SerializeField] private Vector2 localScaleFrom = Vector2.zero;
        [SerializeField] private Vector2 localScaleTo = Vector2.zero;
        
        [HideInInspector] public Vector2 runtimeLocalScaleFrom = Vector2.zero;
        [HideInInspector] public Vector2 runtimeLocalScaleTo = Vector2.zero;

        private void OnEnable()
        {
            runtimeLocalScaleFrom = localScaleFrom;
            runtimeLocalScaleTo = localScaleTo;
        }
        
        public override void ChangeComponent(RectTransform component, float t)
        {
            component.localScale = Vector2.Lerp(runtimeLocalScaleFrom, runtimeLocalScaleTo, EasedT(t));
        }
    }
}