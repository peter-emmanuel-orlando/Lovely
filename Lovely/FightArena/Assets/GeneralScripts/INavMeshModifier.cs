namespace UnityEngine.AI
{
    public interface INavMeshModifier
    {
        int area { get; }
        bool ignoreFromBuild { get;  }
        bool overrideArea { get; }
        GameObject gameObject { get; }
        Transform transform { get; }
        MonoBehaviour monoBehaviour { get; }

        bool AffectsAgentType(int agentTypeID);

    }
}