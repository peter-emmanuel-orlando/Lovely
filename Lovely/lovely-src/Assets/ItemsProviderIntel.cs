

using System;
using System.Collections.Generic;

public interface IItemsProviderIntel<out T> : IIntel<IItemsProvider<T>> where T : IItem
{
    IEnumerable<Type> ItemTypes { get; }
}

public class ItemsProviderIntel<T> : Intel<IItemsProvider<T>>, IItemsProviderIntel<T> where T : IItem
{
    public IEnumerable<Type> ItemTypes { get { return Subject.ItemTypes; } }

    public ItemsProviderIntel(Body requester, IItemsProvider<T> subject) : base(requester, subject)
    { }
}
