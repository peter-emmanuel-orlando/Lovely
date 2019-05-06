



using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// a resource is an occupiablelocation that you can extract Commodities or items from
/// </summary>
// a resource should only return items. an item can be food, constructionMaterials, magic, PreciousMaterials or any combination thereof. Each of these is an interface so a resource can implement any one

public abstract class ItemsProvider : MonoBehaviour, IItemsProvider<IResource>
{
    public abstract IEnumerable<Type> ItemTypes{ get; }
    public abstract float harvestTime { get; protected set; }//in game hrs it takes to get one of the item
    public abstract float harvestCount { get; protected set; }//how many items are left to be harvested    
    public bool HasItems { get { return harvestCount != 0; } }

    [SerializeField]
    private Vector3 harvestDistance = Vector3.one * 4;
    private Collider boundMin = null;
    public virtual Bounds Bounds => (boundMin == null)? new Bounds(transform.position, harvestDistance ) : new Bounds(boundMin.bounds.center, boundMin.bounds.size + harvestDistance);

    //public IEnumerable<IInteractiveLocation> InteractiveLocations => new IInteractiveLocation[] { this };

    private void OnDrawGizmosSelected()
    {
        if(boundMin == null)
            boundMin = GetComponent<Collider>();
        Gizmos.color = Color.Lerp(Color.blue, Color.magenta, 0.5f);
        Gizmos.DrawWireCube(Bounds.center, Bounds.size);
    }

    protected virtual void Awake()
    {
        boundMin = GetComponent<Collider>();
    }
    protected virtual void OnEnable()
    {
        TrackedComponent.Track(this);
    }
    protected virtual void OnDestroy()
    {
        OnDisable();
    }
    protected virtual void OnDisable()
    {
        TrackedComponent.Untrack(this);
    }

    public virtual bool CanBeAcquiredBy<T>(T potentialHarvester)
    {
        bool result = HasItems && isActiveAndEnabled;
        return result;//checks if the being has the tools neccessary to harvest
    }

    public abstract bool Acquire<T>(T acquisitioner, out List<IItem> acquiredItems, out List<ISpawnedItem<IItem>> spawnedResources, bool requestSuccessOverride = false);

    public virtual AcquireItemPerformable GetInteractionPerformable(Body performer)
    {
        return new AcquireItemPerformable(performer.Mind, this);
    }
}




/*
public abstract class ResourceProvider<T> : ResourceProvider, IItemProvider<T> where T : IResource
{

    public bool HarvestResource(Body harvester, out T harvestedResources, out ISpawnedItem<T> spawnedResources)
    {
        harvestedResources = default;
        spawnedResources = default;
        var result = CanBeingHarvest(harvester);
        if (result)
            Harvest( out harvestedResources, out spawnedResources);
        return result;
    }

    protected override void Harvest(out IResource harvestedResources, out ISpawnedItem<IResource> spawnedResources)
    {
        Harvest(out T tmp1, out ISpawnedItem<T> tmp2);
        harvestedResources = tmp1;
        spawnedResources = (ISpawnedItem<IResource>)tmp2;
    }
    protected abstract void Harvest(out T harvestedResources, out ISpawnedItem<T> spawnedResources);
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
