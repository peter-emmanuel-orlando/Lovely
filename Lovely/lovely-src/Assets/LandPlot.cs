

//these are items you never really take into your inventory, rather you have to go to them.
//the location of these may be stored
using System.Linq;
using UnityEngine;

public class LandPlot : MonoBehaviour, IRealEstate
{
    public Bounds Bounds { get; }
    public bool IsFlat { get; private set; } = false;
    public bool Flatten()
    {
        throw new System.NotImplementedException();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Bounds.center, Bounds.size);
    }

    private LandPlot( Bounds bounds)
    {
        Bounds = bounds;
        TrackedComponent.Track(this);
    }

    private class LandPlotAuthToken : AdminToken<LandPlot>
    {
        public LandPlotAuthToken(LandPlot authSubject, float expiry = float.PositiveInfinity) : base(authSubject, expiry)
        {

        }
    }
    public static bool AcquireRealEstate<T> (out IAuthorizationToken<LandPlot> result, Bounds requestedBounds, IAuthorizationToken<LandPlot> ownershipToken = null )
    {
        //ownershipToken is so you can get subplots of land within your land
        result = null;
        var success = false;
        var overlapping = TrackedComponent.GetOverlapping<LandPlot>(requestedBounds, true);
        if(overlapping.Any())
        {
            if (ownershipToken == null) return false;
            foreach (var land in overlapping)
            {
                if (ownershipToken.AuthSubject != land)
                    return false;
            }
        }
        var newLand = new LandPlot(requestedBounds);
        result = new LandPlotAuthToken(newLand);
        return success;
    }

    public bool IsAuthorized(IAuthorizationToken<IUseableLocation> authToken)
    {
        throw new System.NotImplementedException();
    }

    public bool AcquireUse<T>(out ILocationUseToken<IUseableLocation> useToken)
    {
        throw new System.NotImplementedException();
    }
}



