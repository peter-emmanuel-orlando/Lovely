//these are items you never really take into your inventory, rather you have to go to them.
//the location of these may be stored

public interface IUseableLocation : IBounded
{
    bool IsAuthorized(IAuthorizationToken<IUseableLocation> authToken);
    bool AcquireUse<T>(out ILocationUseToken<IUseableLocation> useToken);
}

public interface ILocationUseToken<T> where T : IUseableLocation{}

