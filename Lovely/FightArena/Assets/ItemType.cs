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