namespace Zoroiscrying.CoreGameSystems.AnimatableObject
{
    public interface IAnimatable
    {
        public EasingFunction.Ease AnimateEasingType { get; }

        /// <summary>
        /// Start to move from start to end.
        /// </summary>
        public void StartFromToBehavior();

        /// <summary>
        /// Start to move from end to start.
        /// </summary>
        public void StartToFromBehavior();

        /// <summary>
        /// Apply the changed value to specific bounded objects.
        /// </summary>
        public void ApplyChangedValue();

    }
}