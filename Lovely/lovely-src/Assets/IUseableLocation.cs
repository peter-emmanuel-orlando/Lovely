//these are items you never really take into your inventory, rather you have to go to them.
//the location of these may be stored

public interface IUseableLocation : IBounded, IRestrictedAccess
{
    bool AcquireUse<T>(out ILocationUseToken<IUseableLocation> useToken);
}

//certain recipies may require being in a specific location. This token is added to the recipie as if it were an item
// all it does is check if Holders current bounds overlaps its location
public interface ILocationUseToken<T> where T : IUseableLocation
{
    IBounded Holder { get; }
    T Location { get; }
    bool IsValid { get; }
}

