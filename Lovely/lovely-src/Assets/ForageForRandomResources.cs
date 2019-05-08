using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
public class ForageForRandomResources : IDecisionMaker
{


    IPerformable currentUndertaking;
    public override IPerformable GetDecisions()
    {
        //Debug.Log("boop");
        //if (currentUndertaking == null || currentUndertaking.isComplete)
        //    currentUndertaking = new FightChoreographer(being).GetDecisions();

        if (currentUndertaking == null || currentUndertaking.isComplete)
        {
            Resource r = null;
            var count = Resource.allResourceLists[ItemType.Wood].Count;
            if (count > 0)
            {
                var index = Random.Range(0, count);
                for (int i = 0; i < count; i++)
                {
                    if (count - 1 != 0)
                        index %= count - 1;
                    var possibleR = Resource.allResourceLists[ItemType.Wood][index];
                    if (possibleR.CanBeingHarvest(being))
                    {
                        r = possibleR;
                        break;
                    }
                    index++;
                }
            }
            if (r != null)
                currentUndertaking = new HarvestResourcePerformable(being, r);
        }
    }
    */