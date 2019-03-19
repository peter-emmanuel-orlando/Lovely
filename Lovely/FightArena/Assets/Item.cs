using System;
using System.Collections.Generic;
public class Item
{
    static ItemType[] _allItemTypes;
    public static ItemType[] allItemTypes
    {
        get
        {
            if (_allItemTypes == null)
            {
                _allItemTypes = (ItemType[])Enum.GetValues(typeof(ItemType));
            }
            return _allItemTypes;
        }
    }

    public static Dictionary<ItemType, T> GetItemTypeDictionary<T>() where T : new()
    {
        var result = new Dictionary<ItemType, T>();
        foreach (ItemType rc in Item.allItemTypes)
        {
            result.Add(rc, new T());
        }
        return result;
    }
}