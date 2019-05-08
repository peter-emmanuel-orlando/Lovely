//these are items you never really take into your inventory, rather you have to go to them.
//the location of these may be stored

public interface IRestrictedAccess
{
    bool IsAuthorized(IAuthorizationToken<IUseableLocation> authToken);
}

