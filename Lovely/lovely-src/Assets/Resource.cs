﻿



using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a resource is an occupiablelocation that you can extract Commodities or items from
/// </summary>
// a resource should only return items. an item can be food, constructionMaterials, magic, PreciousMaterials or any combination thereof. Each of these is an interface so a resource can implement any one
public abstract class Resource : OccupiableLocation
{
    static readonly Dictionary<ItemType, HashSet<Resource>> allResourceLists = Item.GetItemTypeDictionary<HashSet<Resource>>();

    public static HashSet<Resource> GetAllResources()
    {
        var result = new HashSet<Resource>();
        foreach (var item in allResourceLists.Values)
        {
            result.UnionWith(item);
        }
        return result;
    }

    public static HashSet<Resource> GetAllResources(params ItemType[] types)
    {
        return GetAllResources(new List<ItemType>(types));
    }

    public static HashSet<Resource> GetAllResources(List<ItemType> types)
    {
        var result = new HashSet<Resource>();
        foreach (var type in types)
        {
            result.UnionWith(allResourceLists[type]);
        }
        return result;
    }




    public static Dictionary<ItemType, List<ResourceIntel>> GetIntelOnAllResourcesInSightRange(Body harvester)
    {
        return GetIntelOnAllResourcesInSightRange(harvester, (ItemType[])Enum.GetValues(typeof(ItemType)));
    }

    public static Dictionary<ItemType, List<ResourceIntel>> GetIntelOnAllResourcesInSightRange(Body harvester, params ItemType[] resourceTypes)
    {
        return GetIntelOnAllResourcesInSightRange(harvester, new List<ItemType>(resourceTypes));
    }

    public static Dictionary<ItemType, List<ResourceIntel>> GetIntelOnAllResourcesInSightRange(Body harvester, List<ItemType> resourceTypes)
    {
        var result = new Dictionary<ItemType, List<ResourceIntel>>();
        foreach (var code in resourceTypes)
        {
            result.Add(code, new List<ResourceIntel>());
            foreach (var resource in allResourceLists[code])
            {
                if ((resource.transform.position - harvester.transform.position).sqrMagnitude <= harvester.Mind.SightRadius * harvester.Mind.SightRadius)
                    result[code].Add(new ResourceIntel(harvester, resource));
            }
        }
        return result;
    }

    /*
    List<Resource> GetResourcesInRange( Vector3 centerPoint, float searchRadius, ItemType resourcesToSearchFor)
    {
        var result = new List<Resource>();
        foreach (var code in allResourceLists.Keys)
        {
            if(resourcesToSearchFor.contanisAny(code))
            {

            }
        }
        return result;
    }
    */







    public abstract ItemType providedItemType { get; }
    protected abstract IItem SpawnHarvestedItem();//when a resource is harvested, this is the resulting item
    public abstract float harvestTime { get; }//in game hrs it takes to get one of the item
    public abstract float harvestCount { get; }//how many items are left to be harvested    
    public bool hasResources { get { return harvestCount != 0; } }

    private void OnEnable()
    {
        foreach (var t in providedItemType.Enumerate())
        {
            allResourceLists[t].Add(this);
        }
    }

    private void OnDisable()
    {
        foreach (var t in providedItemType.Enumerate())
        {
            allResourceLists[t].Remove(this);
        }
    }

    public bool HarvestResource(Body harvester, out IItem value)
    {
        var result = CanBeingHarvest(harvester);
        if (result)
            value = SpawnHarvestedItem();
        else
            value = null;
        return result;
    }

    public virtual bool CanBeingHarvest(Body potentialHarvester)
    {
        var result = false;
        if (hasResources && isActiveAndEnabled && IsInHarvestRange(potentialHarvester))
            result = false;
        Debug.LogWarning("checking if being qualifies to harvest resource is only partially implemented");
        return result;//checks if the being has the tools neccessary to harvest
    }

    // harvester can only harvest the resource if it in in the occupiableArea
    public bool IsInHarvestRange(Body potentialHarvester)
    {
        var result = IsOccupant(potentialHarvester) && isActiveAndEnabled;
        return result;
    }

    public abstract HarvestResourcePerformable GetHarvestPerformable(Body performer);
}










/*
public class Resource : MonoBehaviour
{
	//citizen requests slot from resource,
	//resource returns compound task where:
	//	citizen navigates to resource while calling renewLease() so as to not loose its spot
	//	citizen performs harvesting animation while calling harvestResource()
	//end Task
	//citizen should alter task.CallBack to get notification when task is complete

	public static readonly Dictionary <ItemType, List<Resource>> allResourceLists = Item.GetItemTypeDictionary<List<Resource>>();

	static ItemType[] _allItemTypes;
	public static ItemType[] allItemTypes
	{
		get 
		{
			if (_allItemTypes == null)
			{
				_allItemTypes = (ItemType[])Enum.GetValues (typeof(ItemType));
			}
			return _allItemTypes;
		}
	}

	public List<Resource> allFoodSourcesInEditorViewable = allResourceLists[ItemType.Food];
	public List<Resource> allGoldSourcesInEditorViewable = allResourceLists[ItemType.Gold];
	public List<Resource> allStoneSourcesInEditorViewable = allResourceLists[ItemType.Stone];
	public List<Resource> allWoodSourcesInEditorViewable = allResourceLists[ItemType.Wood];
	public List<Resource> allMagicSourcesInEditorViewable = allResourceLists[ItemType.Magic];

	ItemType prevResourceType = ItemType.Nothing;
	public ItemType resourceType = ItemType.Nothing;
	public float resourceQuantity = Mathf.Infinity;
	public float resourceDeteriorationRate = 0;// if this is bigger than 0, the resource will slowly diminish over time. When its empty, it will destroy itself
	public float baseHarvestSpeed = 1;//in units per in game hour

	public CommodityPacket passiveResourceGainRatios = CommodityPacket.empty;

	public List<OccupiableArea> occupiableResourseAreas = new List<OccupiableArea>();
	public List<OccupiablePoint> occupiableResourcePoints = new List<OccupiablePoint>();
	Dictionary <CivilizationUnit, Occupiable> harvestersToOccupiable = new Dictionary<CivilizationUnit, Occupiable>();
	Dictionary <CivilizationUnit, int> remoteHarvesters = new Dictionary<CivilizationUnit, int>();


	public static Dictionary<ItemType, T> GetResourceDictionary<T>() where T : new()
	{
		var result = new Dictionary<ItemType, T> ();
		foreach(ItemType rc in Item.allItemTypes)
		{
			result.Add(rc, new T());
		}
		return result;
	}

	public int GetMaxHarvesters()
	{
		int result = occupiableResourcePoints.Count;
		foreach (var item in occupiableResourseAreas) 
		{
			result += item.maxOccupants;
		}
		return result;
	}


	void Start()
	{
		//Debug.Log ("AllResourceLists: " + AllResourceLists);
		//Debug.Log ("resourceType: " + resourceType);
		//Debug.Log ("AllResourceLists [(int)resourceType].Count: " + AllResourceLists [(int)resourceType].Count);
		allResourceLists [prevResourceType].Add (this);
		occupiableResourcePoints.AddRange( GetComponentsInChildren<OccupiablePoint> ());
		occupiableResourseAreas.AddRange( GetComponentsInChildren<OccupiableArea> ());
	}



	public bool isSpaceAvailible;

	public virtual CommodityPacket HarvestResource(CivilizationUnit c)//this is the quantity of the primary resource
	{
		if (harvestersToOccupiable.ContainsKey (c) || remoteHarvesters.ContainsKey (c)) 
		{
			RenewLease (c);
			float harvestedAmmount = Mathf.Clamp (baseHarvestSpeed * GameTime.gameDeltaTimeHours, 0, resourceQuantity);
			resourceQuantity -= harvestedAmmount;
			CommodityPacket rp = passiveResourceGainRatios * harvestedAmmount;
			rp [resourceType] += harvestedAmmount;
			return rp;
		}
		else
		{
			Task emptyTask;
			if (RequestHarvestPoint (c, out emptyTask, true)) 
			{
				return HarvestResource (c);
			} 
			else
			{
				return CommodityPacket.empty;
			}
		}
	}













	public bool RequestHarvestPoint (CivilizationUnit c, out Task resourceTask, bool canHarvestRemotely)
	{
		bool success = false;
		resourceTask = new Task (Vector3.zero, Vector3.zero, null);

		if ( canHarvestRemotely == true && remoteHarvesters.ContainsKey(c) == false)
		{
			harvestersToOccupiable.Remove (c);
			remoteHarvesters.Add (c, 5);
			resourceTask = new Task (c.transform.position, c.transform.eulerAngles, null);
			success = true;
		}

		if (success == false && harvestersToOccupiable.ContainsKey (c) == false) 
		{
			remoteHarvesters.Remove (c);
			//gets an availible Location, but if those are all full, gets a space on the meshes. This can be overridden in children
			foreach (OccupiablePoint op in occupiableResourcePoints)
			{
				if (op.RequestOccupiableSpot (c.gameObject, out resourceTask))
				{
					success = true;
					break;
				}
			}
			if(success == false)
			{
				foreach (OccupiableArea oa in occupiableResourseAreas)
				{
					if (oa.RequestOccupiableSpot (c.gameObject, out resourceTask))
					{
						success = true;
						break;
					}
				}
			}
		}
		if(success)
		{
			resourceTask.lookHere = transform.position;
			resourceTask.untilThisCondition = () => {return resourceQuantity <= 0;};
			resourceTask.callThisWhileMoving = () => {RenewLease (c);Debug.Log("BOOP");};
			resourceTask.callOnEndSuccess = () => {	c.IntakeResources (HarvestResource (c));};
		}
		return success;
	}

	void RenewLease(CivilizationUnit c)
	{
		if (harvestersToOccupiable.ContainsKey(c))
		{
			harvestersToOccupiable [c].RenewLease (c.gameObject);
		}
		if (remoteHarvesters.ContainsKey(c))
		{
			remoteHarvesters [c] = 5;
		}
		Debug.Log (c + " tried to renew its lease for " + gameObject);
	}


	public List<CivilizationUnit> harvestersToOccupiableInEditorViewable = new List<CivilizationUnit>();
	public List <CivilizationUnit> remoteHarvestersInEditorViewable = new List<CivilizationUnit>();
	// Update is called once per frame
	void Update () 
	{
		harvestersToOccupiableInEditorViewable = new List<CivilizationUnit>();
		remoteHarvestersInEditorViewable = new List<CivilizationUnit>();
		harvestersToOccupiableInEditorViewable.AddRange(harvestersToOccupiable.Keys);
		remoteHarvestersInEditorViewable.AddRange(remoteHarvesters.Keys);
		if (resourceType != prevResourceType)
		{
			//Debug.Log("resourceType: " + resourceType + ", prevResourceType: " + prevResourceType);
			foreach(KeyValuePair<ItemType, List<Resource>>  kv in allResourceLists)
			{
				//Debug.Log ("resource = " + kv.Key + " count = " + kv.Value.Count);
			}
			allResourceLists [prevResourceType].Remove (this);
			//AllResourceLists [(int)resourceType].Remove (this);
			allResourceLists [resourceType].Add (this);
			prevResourceType = resourceType;
		}

		Dictionary<CivilizationUnit, int> tmp = new Dictionary<CivilizationUnit, int> ();
		foreach (var harvester in remoteHarvesters.Keys) 
		{
			tmp.Add( harvester, remoteHarvesters[harvester] - 1);
		}
		remoteHarvesters = tmp;

		resourceQuantity -= resourceDeteriorationRate * GameTime.gameDeltaTime * 3600;// per in-game hr;
		if(resourceQuantity <= 0)
		{
			Destroy (gameObject);
		}

	}

	void OnDestroy()
	{
		allResourceLists [resourceType].Remove (this);
		Destroy (gameObject);
	}
}
*/