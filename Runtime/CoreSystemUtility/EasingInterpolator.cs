namespace Zoroiscrying.CoreGameSystems.CoreSystemUtility
{
    public class EasingInterpolator
    {
        public EasingInterpolator(EasingFunction.Ease type)
        {
            easeType = type;
        }

        public void ChangeEaseType(EasingFunction.Ease type)
        {
            easeType = type;
        }
    
        private EasingFunction.Ease easeType = EasingFunction.Ease.Linear;

        public EasingFunction.Function Function
        {
            get
            {
                if (_function == null)
                {
                    _function = EasingFunction.GetEasingFunction(easeType);
                }
                return _function;
            }
            private set => _function = value;
        }
    
        private EasingFunction.Function _function = null;

        public float Evaluate(float from, float to, float t)
        {
            return Function(from, to, t);
        }

        public float Evaluate01(float t)
        {
            return Function(0, 1, t);
        }

        public float Evaluate10(float t)
        {
            return Function(1, 0, t);
        }
    }
}
