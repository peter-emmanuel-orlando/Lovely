

public class ResourceIntel : Intel<Resource>, System.IComparable<ResourceIntel>//ResourceIntel
{
    public ItemType recourceType { get { return subject.providedItemType; } }

    public ResourceIntel(Body requester, Resource subject) : base(requester, subject)
    {    }

    public int CompareTo(ResourceIntel other)
    {
        return base.CompareTo(other);
    }
}