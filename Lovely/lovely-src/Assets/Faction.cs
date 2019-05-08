using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Faction
{
    string factionName;

    public string FactionName
    {
        get
        {
            return factionName;
        }
    }

    private Faction() { }
    public Faction(string factionName)
    {
        this.factionName = factionName;
    }
}
/*
{
    string factionName;
    readonly Dictionary<Faction, HashSet<Relationship>> factionRelationships = new Dictionary<Faction, HashSet<Relationship>>();
    readonly Dictionary<Being, HashSet<Relationship>> individualOverrideRelationships = new Dictionary<Being, HashSet<Relationship>>();


    //putting in your faction returns a dictionary where the keys are other factions and the hashset is a set of relationships between your faction and that faction

    private Faction() { }
    public Faction(string factionName)
    {
        this.factionName = factionName;
    }

    public void SetRelationships(Faction otherFaction, params Relationship[] relationships)
    {
        if (factionRelationships.ContainsKey(otherFaction))
            factionRelationships.Remove(otherFaction);
        factionRelationships.Add(otherFaction, new HashSet<Relationship>(relationships));
    }

    public void SetRelationships(Being otherBeing, params Relationship[] relationships)
    {
        if (individualOverrideRelationships.ContainsKey(otherBeing))
            individualOverrideRelationships.Remove(otherBeing);
        individualOverrideRelationships.Add(otherBeing, new HashSet<Relationship>(relationships));
    }




    public HashSet<Relationship> GetRelationships(Faction otherFaction)
    {
        if (factionRelationships.ContainsKey(otherFaction))
            return Relationships.GetNetRelationships(factionRelationships[otherFaction]);
        else
            return new HashSet<Relationship>();
    }
    public HashSet<Relationship> GetRelationships(params Faction[] otherFactions)
    {
        var result = GetRelationships(otherFactions);
        return result;
    }
    public HashSet<Relationship> GetRelationships(IEnumerable<Faction> otherFactionCollection)
    {
        var result = new HashSet<Relationship>();
        foreach (var faction in otherFactionCollection)
        {
            result.UnionWith(GetRelationships(faction));
        }
        result = Relationships.GetNetRelationships(result);
        return result;
    }



    public HashSet<Relationship> GetRelationships(Being otherBeing)
    {
        var result = new HashSet<Relationship>();
        result.UnionWith(GetRelationships( otherBeing.mind.JoinedFactions));

        if (individualOverrideRelationships.ContainsKey(otherBeing))
            result.UnionWith(individualOverrideRelationships[otherBeing]);
        result = Relationships.GetNetRelationships(result);
        return result;
        
    }




    class FactionRelationshipPair
    {
        string factionName;
        List<Relationship> relationships;
    }
}
*/