

using System;
using System.Collections.Generic;

public class ResourceIntel : Intel<IItemsProvider<IResource>>, System.IComparable<ResourceIntel>
{
    public IEnumerable<Type> recourceType { get { return subject.ItemTypes; } }

    public ResourceIntel(Body requester, IItemsProvider<IResource> subject) : base(requester, subject)
    { }

    public int CompareTo(ResourceIntel other)
    {
        return base.CompareTo(other);
    }
}
