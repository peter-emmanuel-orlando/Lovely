
public interface IInteractable<out T> where T : IPerformable
{
    T GetInteractionPerformable(Body performer);
}
