using System.Collections.Generic;

namespace UnityEngine.AI
{
    [ExecuteInEditMode]
    [DefaultExecutionOrder(-102)]
    [AddComponentMenu("Navigation/NavMeshSurfaceOther", 30)]
    [HelpURL("https://github.com/Unity-Technologies/NavMeshComponents#documentation-draft")]
    public abstract class NavMeshSurfaceBase : MonoBehaviour
    {
        [SerializeField]
        int m_AgentTypeID;
        public int agentTypeID { get { return m_AgentTypeID; } set { m_AgentTypeID = value; } }

        [SerializeField]
        CollectObjects m_CollectObjects = CollectObjects.All;
        public CollectObjects collectObjects { get { return m_CollectObjects; } set { m_CollectObjects = value; } }

        [SerializeField]
        Vector3 m_Size = new Vector3(10.0f, 10.0f, 10.0f);
        public Vector3 size { get { return m_Size; } set { m_Size = value; } }

        [SerializeField]
        Vector3 m_Center = new Vector3(0, 2.0f, 0);
        public Vector3 center { get { return m_Center; } set { m_Center = value; } }

        [SerializeField]
        LayerMask m_LayerMask = ~0;
        public LayerMask layerMask { get { return m_LayerMask; } set { m_LayerMask = value; } }

        [SerializeField]
        NavMeshCollectGeometry m_UseGeometry = NavMeshCollectGeometry.RenderMeshes;
        public NavMeshCollectGeometry useGeometry { get { return m_UseGeometry; } set { m_UseGeometry = value; } }

        [SerializeField]
        int m_DefaultArea;
        public int defaultArea { get { return m_DefaultArea; } set { m_DefaultArea = value; } }//{ get { return NavMesh.GetAreaFromName("NavMeshOther"); } }//does not yet support nesting. This is where youd modify to implement that

        [SerializeField]
        bool m_IgnoreNavMeshAgent = true;
        public bool ignoreNavMeshAgent { get { return m_IgnoreNavMeshAgent; } set { m_IgnoreNavMeshAgent = value; } }

        [SerializeField]
        bool m_IgnoreNavMeshObstacle = true;
        public bool ignoreNavMeshObstacle { get { return m_IgnoreNavMeshObstacle; } set { m_IgnoreNavMeshObstacle = value; } }

        [SerializeField]
        bool m_OverrideTileSize;
        public bool overrideTileSize { get { return m_OverrideTileSize; } set { m_OverrideTileSize = value; } }
        [SerializeField]
        int m_TileSize = 256;
        public int tileSize { get { return m_TileSize; } set { m_TileSize = value; } }
        [SerializeField]
        bool m_OverrideVoxelSize;
        public bool overrideVoxelSize { get { return m_OverrideVoxelSize; } set { m_OverrideVoxelSize = value; } }
        [SerializeField]
        float m_VoxelSize;
        public float voxelSize { get { return m_VoxelSize; } set { m_VoxelSize = value; } }

        // Currently not supported advanced options
        [SerializeField]
        bool m_BuildHeightMesh;
        public bool buildHeightMesh { get { return m_BuildHeightMesh; } set { m_BuildHeightMesh = value; } }

        // Reference to whole scene navmesh data asset.
        [UnityEngine.Serialization.FormerlySerializedAs("m_BakedNavMeshData")]
        [SerializeField]
        NavMeshData m_NavMeshData;
        public NavMeshData navMeshData { get { return m_NavMeshData; } set { m_NavMeshData = value; } }

        // Do not serialize - runtime only state.
        NavMeshDataInstance m_NavMeshDataInstance;
        Vector3 m_LastPosition = Vector3.zero;
        Quaternion m_LastRotation = Quaternion.identity;


        static readonly List<NavMeshSurfaceBase> s_NavMeshSurfaces = new List<NavMeshSurfaceBase>();

        public static List<NavMeshSurfaceBase> activeSurfaces
        {
            get { return s_NavMeshSurfaces; }
        }

        
        protected virtual void OnEnable()
        {
            Register(this);
            AddData();
        }

        protected virtual void OnDisable()
        {
            RemoveData();
            Unregister(this);
        }

        public void AddData()
        {
            if (m_NavMeshDataInstance.valid)
                return;

            if (m_NavMeshData != null)
            {
                m_NavMeshDataInstance = NavMesh.AddNavMeshData(m_NavMeshData, transform.position, transform.rotation);
                m_NavMeshDataInstance.owner = this;
            }

            m_LastPosition = transform.position;
            m_LastRotation = transform.rotation;
        }

        public void RemoveData()
        {
            m_NavMeshDataInstance.Remove();
            m_NavMeshDataInstance = new NavMeshDataInstance();
        }

        public NavMeshBuildSettings GetBuildSettings()
        {
            var buildSettings = NavMesh.GetSettingsByID(m_AgentTypeID);
            if (buildSettings.agentTypeID == -1)
            {
                Debug.LogWarning("No build settings for agent type ID " + agentTypeID, this);
                buildSettings.agentTypeID = m_AgentTypeID;
            }

            if (overrideTileSize)
            {
                buildSettings.overrideTileSize = true;
                buildSettings.tileSize = tileSize;
            }
            if (overrideVoxelSize)
            {
                buildSettings.overrideVoxelSize = true;
                buildSettings.voxelSize = voxelSize;
            }
            return buildSettings;
        }

        public void BuildNavMesh()
        {
            var sources = CollectSources();

            // Use unscaled bounds - this differs in behaviour from e.g. collider components.
            // But is similar to reflection probe - and since navmesh data has no scaling support - it is the right choice here.
            var sourcesBounds = new Bounds(m_Center, Abs(m_Size));
            if (m_CollectObjects == CollectObjects.All || m_CollectObjects == CollectObjects.Children)
            {
                sourcesBounds = CalculateWorldBounds(sources);
            }

            var data = NavMeshBuilder.BuildNavMeshData(GetBuildSettings(),
                    sources, sourcesBounds, transform.position, transform.rotation);

            if (data != null)
            {
                data.name = gameObject.name;
                RemoveData();
                m_NavMeshData = data;
                if (isActiveAndEnabled)
                    AddData();
            }
        }

        public AsyncOperation UpdateNavMesh(NavMeshData data)
        {
            var sources = CollectSources();

            // Use unscaled bounds - this differs in behaviour from e.g. collider components.
            // But is similar to reflection probe - and since navmesh data has no scaling support - it is the right choice here.
            var sourcesBounds = new Bounds(m_Center, Abs(m_Size));
            if (m_CollectObjects == CollectObjects.All || m_CollectObjects == CollectObjects.Children)
                sourcesBounds = CalculateWorldBounds(sources);

            return NavMeshBuilder.UpdateNavMeshDataAsync(data, GetBuildSettings(), sources, sourcesBounds);
        }

        static void Register(NavMeshSurfaceBase surface)
        {
            if (s_NavMeshSurfaces.Count == 0)
                NavMesh.onPreUpdate += UpdateActive;

            if (!s_NavMeshSurfaces.Contains(surface))
                s_NavMeshSurfaces.Add(surface);
        }

        static void Unregister(NavMeshSurfaceBase surface)
        {
            s_NavMeshSurfaces.Remove(surface);

            if (s_NavMeshSurfaces.Count == 0)
                NavMesh.onPreUpdate -= UpdateActive;
        }

        static void UpdateActive()
        {
            for (var i = 0; i < s_NavMeshSurfaces.Count; ++i)
                s_NavMeshSurfaces[i].UpdateDataIfTransformChanged();
        }

        void AppendModifierVolumes(ref List<NavMeshBuildSource> sources)
        {
            // Modifiers
            List<NavMeshModifierVolume> modifiers;
            if (m_CollectObjects == CollectObjects.Children)
            {
                modifiers = new List<NavMeshModifierVolume>(GetComponentsInChildren<NavMeshModifierVolume>());
                modifiers.RemoveAll(x => !x.isActiveAndEnabled);
            }
            else
            {
                modifiers = new List<NavMeshModifierVolume>(NavMeshModifierVolume.activeModifiers);
            }

            foreach (var m in modifiers)
            {
                if ((m_LayerMask & (1 << m.gameObject.layer)) == 0)
                    continue;
                if (!m.AffectsAgentType(m_AgentTypeID))
                    continue;
                var mcenter = m.transform.TransformPoint(m.center);
                var scale = m.transform.lossyScale;
                var msize = new Vector3(m.size.x * Mathf.Abs(scale.x), m.size.y * Mathf.Abs(scale.y), m.size.z * Mathf.Abs(scale.z));

                var src = new NavMeshBuildSource
                {
                    shape = NavMeshBuildSourceShape.ModifierBox,
                    transform = Matrix4x4.TRS(mcenter, m.transform.rotation, Vector3.one),
                    size = msize,
                    area = m.area
                };
                sources.Add(src);
            }
        }

        List<NavMeshBuildSource> CollectSources()
        {
            var sources = new List<NavMeshBuildSource>();
            var markups = new List<NavMeshBuildMarkup>();

            List<INavMeshModifier> modifiers;
            if (m_CollectObjects == CollectObjects.Children)
            {
                modifiers = new List<INavMeshModifier>(GetComponentsInChildren<INavMeshModifier>());
                modifiers.RemoveAll(x => !x.monoBehaviour.isActiveAndEnabled );
            }
            else
            {
                modifiers =  new List<INavMeshModifier>( NavMeshModifier.activeModifiers);
            }
            var parentSurf = transform.parent.GetComponent<NavMeshSurfaceOther>();
            modifiers.RemoveAll(x => x is NavMeshSurfaceOther);

            foreach (var m in modifiers)
            {
                if ((m_LayerMask & (1 << m.gameObject.layer)) == 0)
                    continue;
                if (!m.AffectsAgentType(m_AgentTypeID))
                    continue;
                var markup = new NavMeshBuildMarkup()
                {
                    root = m.transform,
                    overrideArea = m.overrideArea,
                    area = m.modifiedArea,
                    ignoreFromBuild = m.ignoreFromBuild
                };
                markups.Add(markup);
            }

            if (m_CollectObjects == CollectObjects.All)
            {
                NavMeshBuilder.CollectSources(null, m_LayerMask, m_UseGeometry, m_DefaultArea, markups, sources);
            }
            else if (m_CollectObjects == CollectObjects.Children)
            {
                NavMeshBuilder.CollectSources(transform, m_LayerMask, m_UseGeometry, m_DefaultArea, markups, sources);
            }
            else if (m_CollectObjects == CollectObjects.Volume)
            {
                Matrix4x4 localToWorld = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                var worldBounds = GetWorldBounds(localToWorld, new Bounds(m_Center, m_Size));
                NavMeshBuilder.CollectSources(worldBounds, m_LayerMask, m_UseGeometry, m_DefaultArea, markups, sources);
            }

            if (m_IgnoreNavMeshAgent)
                sources.RemoveAll((x) => (x.component != null && x.component.gameObject.GetComponent<NavMeshAgent>() != null));

            if (m_IgnoreNavMeshObstacle)
                sources.RemoveAll((x) => (x.component != null && x.component.gameObject.GetComponent<NavMeshObstacle>() != null));

            AppendModifierVolumes(ref sources);

            return sources;
        }

        static Vector3 Abs(Vector3 v)
        {
            return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        }

        static Bounds GetWorldBounds(Matrix4x4 mat, Bounds bounds)
        {
            var absAxisX = Abs(mat.MultiplyVector(Vector3.right));
            var absAxisY = Abs(mat.MultiplyVector(Vector3.up));
            var absAxisZ = Abs(mat.MultiplyVector(Vector3.forward));
            var worldPosition = mat.MultiplyPoint(bounds.center);
            var worldSize = absAxisX * bounds.size.x + absAxisY * bounds.size.y + absAxisZ * bounds.size.z;
            return new Bounds(worldPosition, worldSize);
        }

        Bounds CalculateWorldBounds(List<NavMeshBuildSource> sources)
        {
            // Use the unscaled matrix for the NavMeshSurfaceOther
            Matrix4x4 worldToLocal = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            worldToLocal = worldToLocal.inverse;

            var result = new Bounds();
            foreach (var src in sources)
            {
                switch (src.shape)
                {
                    case NavMeshBuildSourceShape.Mesh:
                        {
                            var m = src.sourceObject as Mesh;
                            result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, m.bounds));
                            break;
                        }
                    case NavMeshBuildSourceShape.Terrain:
                        {
                            // Terrain pivot is lower/left corner - shift bounds accordingly
                            var t = src.sourceObject as TerrainData;
                            result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, new Bounds(0.5f * t.size, t.size)));
                            break;
                        }
                    case NavMeshBuildSourceShape.Box:
                    case NavMeshBuildSourceShape.Sphere:
                    case NavMeshBuildSourceShape.Capsule:
                    case NavMeshBuildSourceShape.ModifierBox:
                        result.Encapsulate(GetWorldBounds(worldToLocal * src.transform, new Bounds(Vector3.zero, src.size)));
                        break;
                }
            }
            // Inflate the bounds a bit to avoid clipping co-planar sources
            result.Expand(0.1f);
            return result;
        }

        bool HasTransformChanged()
        {
            if (m_LastPosition != transform.position) return true;
            if (m_LastRotation != transform.rotation) return true;
            return false;
        }

        void UpdateDataIfTransformChanged()
        {
            if (HasTransformChanged())
            {
                RemoveData();
                AddData();
            }
        }

#if UNITY_EDITOR
        bool UnshareNavMeshAsset()
        {
            // Nothing to unshare
            if (m_NavMeshData == null)
                return false;

            // Prefab parent owns the asset reference
            var prefabType = UnityEditor.PrefabUtility.GetPrefabType(this);
            if (prefabType == UnityEditor.PrefabType.Prefab)
                return false;

            // An instance can share asset reference only with its prefab parent
            var prefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(this) as NavMeshSurfaceBase;
            if (prefab != null && prefab.navMeshData == navMeshData)
                return false;

            // Don't allow referencing an asset that's assigned to another surface
            for (var i = 0; i < s_NavMeshSurfaces.Count; ++i)
            {
                var surface = s_NavMeshSurfaces[i];
                if (surface != this && surface.m_NavMeshData == m_NavMeshData)
                    return true;
            }

            // Asset is not referenced by known surfaces
            return false;
        }

        void OnValidate()
        {
            if (UnshareNavMeshAsset())
            {
                Debug.LogWarning("Duplicating NavMeshSurfaceOther does not duplicate the referenced navmesh data", this);
                m_NavMeshData = null;
            }

            var settings = NavMesh.GetSettingsByID(m_AgentTypeID);
            if (settings.agentTypeID != -1)
            {
                // When unchecking the override control, revert to automatic value.
                const float kMinVoxelSize = 0.01f;
                if (!m_OverrideVoxelSize)
                    m_VoxelSize = settings.agentRadius / 3.0f;
                if (m_VoxelSize < kMinVoxelSize)
                    m_VoxelSize = kMinVoxelSize;

                // When unchecking the override control, revert to default value.
                const int kMinTileSize = 16;
                const int kMaxTileSize = 1024;
                const int kDefaultTileSize = 256;

                if (!m_OverrideTileSize)
                    m_TileSize = kDefaultTileSize;
                // Make sure tilesize is in sane range.
                if (m_TileSize < kMinTileSize)
                    m_TileSize = kMinTileSize;
                if (m_TileSize > kMaxTileSize)
                    m_TileSize = kMaxTileSize;
            }
        }
#endif
    }

    public class NavMeshSurfaceOther : NavMeshSurfaceBase, INavMeshModifier
    {
        //from INavMeshModifier
        public MonoBehaviour monoBehaviour { get { return this; } }
        public int modifiedArea { get { return 1; } }//notWalkable, this is for other navmeshstuff though, it will be ignored by this component
        public bool ignoreFromBuild { get { return false; } }
        public bool overrideArea { get { return true; } }
        public bool AffectsAgentType(int agentTypeID) { return true; }

        private static readonly Dictionary<NavMeshAgent, AreaOverlap> trackedAgents = new Dictionary<NavMeshAgent, AreaOverlap>();


        protected override void OnEnable()
        {
            base.OnEnable();
            if (!NavMeshModifier.activeModifiers.Contains(this))
                NavMeshModifier.activeModifiers.Add(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            NavMeshModifier.activeModifiers.Remove(this);
        }

        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("ModularNavMesh");
        }

        private void Update()
        {
            if(Application.isPlaying)
            {
                foreach (var agent in trackedAgents.Keys)
                {
                    var activeColor = Color.cyan;
                    if (trackedAgents[agent].activeArea == 3) activeColor = Color.magenta;
                    if (trackedAgents[agent].activeArea == 4) activeColor = Color.green;
                    var secondaryColor = Color.cyan;
                    if (trackedAgents[agent].secondaryArea == 3) secondaryColor = Color.magenta;
                    if (trackedAgents[agent].secondaryArea == 4) secondaryColor = Color.green;

                    DebugShape.DrawSphere(agent.transform.position + Vector3.up, 1, activeColor, .5f);
                    DebugShape.DrawSphere(agent.transform.position, 1, secondaryColor, .5f);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            //if this is called multiple times on the same agent, the activeArea will already be this  one so no changes would be made
            var agent = other.GetComponentInParent<NavMeshAgent>();
            if(agent)
            {
                if(trackedAgents.ContainsKey(agent))
                { 
                    if(trackedAgents[agent].activeArea != defaultArea)
                    {
                        trackedAgents[agent].secondaryArea = trackedAgents[agent].activeArea;
                        trackedAgents[agent].activeArea = this.defaultArea;
                    }                
                }
                else
                {
                    trackedAgents.Add(agent, new AreaOverlap(defaultArea, 0));
                }
                agent.areaMask = 1 << trackedAgents[agent].activeArea;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var agent = other.GetComponentInParent<NavMeshAgent>();
            if (agent && trackedAgents.ContainsKey(agent))
            {
                var tracked = trackedAgents[agent];
                if (trackedAgents.ContainsKey(agent))
                {
                    if (tracked.activeArea == this.defaultArea)
                    {
                        tracked.activeArea = tracked.secondaryArea;
                        tracked.secondaryArea = 0;
                    }
                    if (tracked.secondaryArea == this.defaultArea)
                    {
                        tracked.secondaryArea = 0;
                    }
                }

                agent.areaMask = 1 << trackedAgents[agent].activeArea;
            }


            //if the agent has been destroyed but its in the dictionary
            //OR
            //if the agent isnt on any navmeshsurfaceothers(both its overlap areas are the default area)
            //THEN
            //remove it from the dictionary.
            if (trackedAgents.ContainsKey(agent) && (!agent || (trackedAgents[agent].secondaryArea == 0 && trackedAgents[agent].activeArea == 0)))
                trackedAgents.Remove(agent);
        }

        //if two Enters or two Exits are called on the same frame, then the agent is waarping to positions, or moving fast enough essentially be warping positions, or the triggers are too close
        //in this case you have to just try the layers and see which works.

        private class AreaOverlap
        {
            public int activeArea;
            public int secondaryArea;

            public AreaOverlap(int activeArea, int secondaryArea)
            {
                this.activeArea = activeArea;
                this.secondaryArea = secondaryArea;
            }
        }
    }
}