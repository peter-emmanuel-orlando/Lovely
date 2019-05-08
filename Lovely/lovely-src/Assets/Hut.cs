using System.Collections;
using UnityEngine;

public class Hut
{

}

public class Construction
{
    float constructionProgress = 0;
    Vector3 requiredSize = Vector3.one;

    public LandPlot emptyLand = null;
    public GameObject[] progressLevelPrefabs = null;

    public Construction(LandPlot emptyLand)
    {
        this.emptyLand = emptyLand;
        emptyLand.StartCoroutine( Construct());
    }

    public IEnumerator Construct()
    {
        emptyLand.Flatten();

        while (constructionProgress < 1)
        {

        }

        GameObject.Instantiate();
        yield break;
    }

}