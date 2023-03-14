using UnityEngine;
using Zoroiscrying.CoreGameSystems.AnimatableObject;

namespace Zoroiscrying.CoreGameSystems.UISystem.ScriptableObjectIntegration
{
    [CreateAssetMenu(fileName = "RectTransformOffsetPositionAnimator", menuName = "Unity Core/Unity Panel System/SO Animator/RectTransformOffsetPosition")]
    public class RectTransformOffsetSlideAnimator : BaseComponentAnimatorSo<RectTransform>
    {
        //  /*Left*/ rectTransform.offsetMin.x;
        //  /*Right*/ rectTransform.offsetMax.x;
        //  /*Top*/ rectTransform.offsetMax.y;
        //  /*Bottom*/ rectTransform.offsetMin.y;
        [SerializeField] private Vector4 offsetLeftLowerRightUpperFrom = Vector4.zero;
        [SerializeField] private Vector4 offsetLeftLowerRightUpperTo = Vector4.zero;

        [HideInInspector] public Vector4 runtimeOffsetLeftLowerRightUpperFrom = Vector4.zero;
        [HideInInspector] public Vector4 runtimeOffsetLeftLowerRightUpperTo = Vector4.zero;

        private void OnEnable()
        {
            runtimeOffsetLeftLowerRightUpperFrom = offsetLeftLowerRightUpperFrom;
            runtimeOffsetLeftLowerRightUpperTo = offsetLeftLowerRightUpperTo;
        }
        
        public override void ChangeComponent(RectTransform component, float t)
        {
            var offset = Vector4.Lerp(runtimeOffsetLeftLowerRightUpperFrom, runtimeOffsetLeftLowerRightUpperTo,
                EasedT(t));
            component.offsetMin = new Vector2(offset.x, offset.y);
            component.offsetMax = new Vector2(offset.z, offset.w);
        }
    }
}