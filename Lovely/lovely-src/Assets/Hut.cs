using System.Collections;
using UnityEngine;

public class Hut
{

}

public class Construction : MonoBehaviour
{
    public ItemRecipie recipie { get; }
    public float constructionProgress { get; private set; }
    /// <summary>
    /// in percent / hr
    /// </summary>
    public const float constructionSpeed = 2;
    Vector3 requiredSize = Vector3.one;

    public LandPlot emptyLand = null;
    public GameObject[] progressLevelPrefabs = null;

    private void Awake()
    {
        emptyLand.StartCoroutine(Construct());
    }
    /*
    public Construction(LandPlot emptyLand)
    {
        this.emptyLand = emptyLand;
        emptyLand.StartCoroutine( Construct());
    }*/

    public IEnumerator Construct()
    {
        emptyLand.Flatten();

        GameObject currentInstance = null;
        var currentIndex = -1;
        while (constructionProgress < 1 && progressLevelPrefabs.Length > 0)
        {
            constructionProgress = Mathf.Min(constructionProgress + Time.deltaTime, recipie.Progress);
            var levelLength = 1f / progressLevelPrefabs.Length;
            var level = Mathf.CeilToInt(constructionProgress / levelLength) -1;
            level = Mathf.Clamp( level, 0, progressLevelPrefabs.Length-1);
            if(currentIndex < level)
            {
                currentIndex = level;
                GameObject.Destroy(currentInstance);
                currentInstance = GameObject.Instantiate(progressLevelPrefabs[currentIndex], emptyLand.transform );
                currentInstance.transform.localPosition = Vector3.zero;
                currentInstance.transform.localEulerAngles = Vector3.zero;
            }
            yield return null;
        }

        yield break;
    }

}