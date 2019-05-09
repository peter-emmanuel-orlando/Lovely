using System.Collections;
using UnityEngine;

public class Construction
{
    IConstructionInfo Info { get; }
    float ConstructionProgress { get; private set; }
    public Construction(IConstructionInfo constructionInfo)
    {
        Info = constructionInfo;
    }

    public void BeginConstruction()
    {
        if((Info.Land ?? false) || !Info.Land.Bounds.e <)
        Info.Land?.StartCoroutine(Construct());
    }
    private IEnumerator Construct()
    {
        Info.Land.Flatten();

        GameObject currentInstance = null;
        var currentIndex = -1;
        while (ConstructionProgress < 1 && Info.ProgressLevelPrefabs.Length > 0)
        {
            ConstructionProgress = Mathf.Min(ConstructionProgress + GameTime.DeltaTimeGameHours * Info.ConstructionSpeed);//, recipie.Progress);
            var levelLength = 1f / Info.ProgressLevelPrefabs.Length;
            var level = Mathf.CeilToInt(ConstructionProgress / levelLength) -1;
            level = Mathf.Clamp( level, 0, Info.ProgressLevelPrefabs.Length-1);
            if(currentIndex < level)
            {
                currentIndex = level;
                GameObject.Destroy(currentInstance);
                currentInstance = GameObject.Instantiate(Info.ProgressLevelPrefabs[currentIndex], Info.Land.transform );
                currentInstance.transform.localPosition = Vector3.zero;
                currentInstance.transform.localEulerAngles = Vector3.zero;
            }
            yield return null;
        }

        yield break;
    }

}
public interface IConstructionInfo
{
    Vector3 RequiredWorldSize { get; }
    LandPlot Land { get; }
    ItemRecipie Recipie { get; }
    // in percent / hr
    float ConstructionSpeed { get; }
    GameObject[] ProgressLevelPrefabs { get; }
}