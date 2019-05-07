

//these are items you never really take into your inventory, rather you have to go to them.
//the location of these may be stored
using System.Linq;
using UnityEngine;

public class EmptyRealEstate : IRealEstate
{
    public Bounds Bounds { get; }
    public bool IsFlat { get; private set; } = false;
    public bool Flatten()
    {
        throw new System.NotImplementedException();
    }

    private EmptyRealEstate( Bounds bounds)
    {
        Bounds = bounds;
        TrackedComponent.Track(this);
    }

    public static bool AcquireRealEstate<T> (Bounds requestedBounds,  out IAuthorizationToken<EmptyRealEstate> result, IAuthorizationToken<EmptyRealEstate> ownershipToken = null )
    {
        //ownershipToken is so you can get subplots of land within your land
        result = null;
        var success = false;
        var overlapping = TrackedComponent.GetOverlapping<EmptyRealEstate>(requestedBounds, true);
        if(overlapping.Any())
        {
            if (ownershipToken == null) return false;
            foreach (var land in overlapping)
            {
                if (ownershipToken.AuthSubject != land)
                    return false;
            }
        }
        var newLand = new EmptyRealEstate(requestedBounds);
        return success;
    }

    public bool AcquireUse<T>(/**/ out IAuthorizationToken<IUseableLocation> useToken, IAuthorizationToken<IUseableLocation> authorisingToken )
    {
        throw new System.NotImplementedException();
    }

    public bool AcquireUse<T>(T requester, out IAuthorizationToken<IUseableLocation> useToken)
    {
        throw new System.NotImplementedException();
    }
}



