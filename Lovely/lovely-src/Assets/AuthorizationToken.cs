using UnityEngine;


//token grant permissions
//  admin
//      -CANNOT DEMOTE OTHER ADMINS
//      -can revoke or change permissions
//      -can create or change any level token
//      -can use admin level features
//      -everything below
//
//  manager
//      -can revoke or change lower permissions
//      -can create or change lower level token
//      -can use manager level features
//      -everything below
//
//  ------------------------------------------------------------
//  || EVERYTHING BELOW MAY NOT ALLOW ACCESS AT CERTAIN TIMES ||
//  ------------------------------------------------------------
//  worker
//      -can access specific areas at specific times
//
//  User  
//      -can use user level features
//      -everything below
//
//  visitor
//      -can use public features
//

//must be extended in class creating them
public class AdminToken<T> : ManagerToken<T>
{
    protected AdminToken(T authSubject, float expiry = float.PositiveInfinity) : base(authSubject, expiry)
    {

    }
}
public class ManagerToken<T> : WorkerToken<T>
{
    protected ManagerToken(T authSubject, float expiry = float.PositiveInfinity) : base(authSubject, expiry)
    {

    }
}
public class WorkerToken<T> : VisitorToken<T>
{
    protected WorkerToken(T authSubject, float expiry = float.PositiveInfinity) : base(authSubject, expiry)
    {

    }
}
public class UserToken<T> : VisitorToken<T>
{
    protected UserToken(T authSubject, float expiry = float.PositiveInfinity) : base(authSubject, expiry)
    {

    }
}
public class VisitorToken<T> : AuthorizationToken<T>
{
    protected VisitorToken(T authSubject, float expiry = float.PositiveInfinity) : base(authSubject, expiry)
    {

    }
}
public interface IAuthorizationToken<out T>
{
    T AuthSubject { get; }
    float Expiry { get; }
    bool IsExpired { get; }
}

public class AuthorizationToken<T> : ExpirationToken, IAuthorizationToken<T>
{
    public T AuthSubject { get; }
    protected AuthorizationToken(T authSubject, float expiry = float.PositiveInfinity) : base(expiry)
    {
        AuthSubject = authSubject;
    }
}
public abstract class ExpirationToken
{
    protected ExpirationToken(float expiry = float.PositiveInfinity)
    {
        Expiry = expiry;
    }
    public float Expiry { get; }
    public bool IsExpired => Time.time >= Expiry;
}





// item type determines the purpose of materials. Should be interfaces
//crafting materials are materials used in building other things. 
//construction material are materials used specifically in building structures
//precious materials are materials whos value comes from themselves, they arnt necessarily used. This is precious metal, art, gems, fine wine and the like
//tools are objects to serve a purpose. This can be creating, destroying or anything in between. Probably a toolPurpose enum will futher disabiguate
//food is anything consumed for continued survival
//fuel is any stored energy substance where the energy can be directed by the user

/*
 * 
 * 
 * Metal
 * Wood
 * Stone
 * fiber
 * cloth
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * crafting recipies list out material by interface, thus anything imlementing that interface 
 * will suffice. This allows them to be as specific as possible. For example, a recipie could require
 * an ICarveable which would be anything carveable, or an IWood which would only accept wood,
 * not stone or clay. an IHardwood would only accept a wood that is a hardwood
 * 
 * Each Item all the way from raw material to finished Product has a method GetRecipie() 
 * that returns other items and tools that can be used to create the item
 * 
 * items have a consumeQuantity on use that consumes an ammount of the item, or durability of item
 * 
 * 
 * 
 */
