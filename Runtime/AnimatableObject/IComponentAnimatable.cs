namespace Zoroiscrying.CoreGameSystems.AnimatableObject
{
    public interface IComponentAnimatable<T>
    {
        public abstract void ChangeComponent(T component, float t);
    }
}