

using System;

public class ResourceIntel : Intel<ResourceProvider<IResource>>, System.IComparable<ResourceIntel>
{
    public Type recourceType { get { return subject.ProvidedResource; } }

    public ResourceIntel(Body requester, ResourceProvider<IResource> subject) : base(requester, subject)
    { }

    public int CompareTo(ResourceIntel other)
    {
        return base.CompareTo(other);
    }
}