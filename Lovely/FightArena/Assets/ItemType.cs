using System;

[Flags]///should be powers of 2
public enum ItemType : Byte
{
    None =                  0,
    CraftingMaterial =      1 << 0,
    ConstructionMaterial =  1 << 1,
    PreciousMaterial =      1 << 2,
    Tool =                  1 << 3,
    Food =                  1 << 4,
    Fuel =                  1 << 5
}

//crafting materials are materials used in building other things. 
//construction material are materials used specifically in building structures
//precious materials are materials whos value comes from themselves, they arnt necessarily used. This is precious metal, art, gems, fine wine and the like
//tools are objects to serve a purpose. This can be creating, destroying or anything in between. Probably a toolPurpose enum will futher disabiguate
//food is anything consumed for continued survival
//fuel is any stored energy substance where the energy can be directed by the user