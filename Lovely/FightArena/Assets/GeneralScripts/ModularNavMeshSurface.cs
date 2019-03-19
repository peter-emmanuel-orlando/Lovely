using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


namespace ModularNavMesh
{
    [ExecuteInEditMode]
    [DefaultExecutionOrder(-102)]
    public class ModularNavMeshSurface : ModularNavMeshSurfaceBase, INavMeshModifier
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

        protected override void Update()
        {
            base.Update();
            if (Application.isPlaying)
            {
                foreach (var agent in trackedAgents.Keys)
                {

                    agent.gameObject.DisplayTextComponent("agent active area:" + Convert.ToString(agent.areaMask, 2) + "\n" + "active area:" + trackedAgents[agent].activeArea + " count:" + trackedAgents[agent].activeArea.overlapCount + "\n" + "secondary area:" + trackedAgents[agent].secondaryArea + " count:" + trackedAgents[agent].secondaryArea.overlapCount + "\n", this);
                    var activeColor = Color.cyan;
                    if (trackedAgents[agent].activeArea == 3) activeColor = Color.magenta;
                    if (trackedAgents[agent].activeArea == 4) activeColor = Color.green;
                    var secondaryColor = Color.cyan;
                    if (trackedAgents[agent].secondaryArea == 3) secondaryColor = Color.magenta;
                    if (trackedAgents[agent].secondaryArea == 4) secondaryColor = Color.green;

                    DebugShape.DrawSphere(agent.transform.position + Vector3.up, 1, activeColor);
                    DebugShape.DrawSphere(agent.transform.position, 1, secondaryColor);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            //if this is called multiple times on the same agent, the activeArea will already be this  one so no changes would be made
            var agent = other.GetComponentInParent<NavMeshAgent>();
            if (agent)
            {
                if (trackedAgents.ContainsKey(agent))
                {
                    var trackedAgent = trackedAgents[agent];
                    if (trackedAgent.activeArea != DefaultArea)
                    {
                        if(trackedAgent.secondaryArea == DefaultArea)
                        {
                            var tmp = trackedAgent.activeArea;
                            trackedAgent.activeArea = trackedAgent.secondaryArea;
                            trackedAgent.secondaryArea = tmp;
                        }
                        else
                        {
                            trackedAgent.secondaryArea = trackedAgent.activeArea;
                            trackedAgent.activeArea = new AreaTracking(this.DefaultArea);
                        }
                    }
                }
                else
                {
                    trackedAgents.Add(agent, new AreaOverlap(new AreaTracking(DefaultArea), new AreaTracking(0)));
                }

                trackedAgents[agent].activeArea.overlapCount++;
                agent.areaMask = 1 << trackedAgents[agent].activeArea.area;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var agent = other.GetComponentInParent<NavMeshAgent>();
            if (agent && trackedAgents.ContainsKey(agent))
            {
                var trackedAgent = trackedAgents[agent];
                if (trackedAgents.ContainsKey(agent))
                {
                    if (trackedAgent.secondaryArea == this.DefaultArea)
                    {
                        trackedAgent.secondaryArea.overlapCount--;
                        if(trackedAgent.secondaryArea.overlapCount <= 0)
                            trackedAgent.secondaryArea = new AreaTracking(0);
                    }
                    if (trackedAgent.activeArea == this.DefaultArea)
                    {
                        if(trackedAgent.activeArea.overlapCount > 0)
                        {
                            trackedAgent.activeArea.overlapCount--;
                        }

                        if(trackedAgent.activeArea.overlapCount <= 0)
                        {
                            trackedAgent.activeArea = trackedAgent.secondaryArea;
                            trackedAgent.secondaryArea = new AreaTracking(0);
                        }
                    }
                }

                agent.areaMask = 1 << trackedAgents[agent].activeArea.area;
            }


            //if the agent has been destroyed but its in the dictionary
            //OR
            //if the agent isnt on any navmeshsurfaceothers(both its overlap areas are the default area)
            //THEN
            //remove it from the dictionary.
            if (trackedAgents.ContainsKey(agent) && (!agent || (trackedAgents[agent].secondaryArea.area == 0 && trackedAgents[agent].activeArea.area == 0)))
                trackedAgents.Remove(agent);
        }

        //if two Enters or two Exits are called on the same frame, then the agent is waarping to positions, or moving fast enough essentially be warping positions, or the triggers are too close
        //in this case you have to just try the layers and see which works.

        private class AreaOverlap
        {
            public AreaTracking activeArea;
            public AreaTracking secondaryArea;

            public AreaOverlap(AreaTracking activeArea, AreaTracking secondaryArea)
            {
                this.activeArea = activeArea;
                this.secondaryArea = secondaryArea;
            }
        }
        private class AreaTracking
        {
            public int area;
            public int overlapCount;

            public static implicit operator int(AreaTracking areaTracking)
            {
                return areaTracking.area;
            }

            public AreaTracking(int area, int overlapCount = 0)
            {
                this.area = area;
                this.overlapCount = overlapCount;
            }

            public override string ToString()
            {
                return "NavMeshArea(" + area + ")";
            }
        }
    }


    [ExecuteInEditMode]
    [DefaultExecutionOrder(-102)]
    public abstract class ModularNavMeshSurfaceBase : MonoBehaviour
    {
        [SerializeField]
        NavMeshData data;

        [SerializeField]
        bool getAreaAutomatically = false;
        [SerializeField]
        bool rebuild = false;

        [SerializeField]
        int defaultArea = 3;// NavMesh.GetAreaFromName("WalkableA");
        public int DefaultArea { get { return defaultArea; } }

        [SerializeField]
        List<Bounds> triggerBounds = new List<Bounds>();

        [ShowOnly]
        [SerializeField]
        List<BoxCollider> triggers = new List<BoxCollider>();

        NavMeshDataInstance instance;
        Vector3 margin = Vector3.one * 2;

        protected virtual void OnEnable()
        {
            this.gameObject.layer = LayerMask.NameToLayer("ModularNavMesh");
            if (instance.valid)
                NavMesh.RemoveNavMeshData(instance);
            if (data == null)
                data = new NavMeshData();
            SetUpTriggers();
            UpdateNavMesh();
            UnityEditor.AI.NavMeshVisualizationSettings.showNavigation++;
            if(!Application.isPlaying)
                NavMesh.onPreUpdate += UpdateNavMesh;
        }

        protected virtual void OnDisable()
        {
            if (instance.valid)
                NavMesh.RemoveNavMeshData(instance);
            instance = new NavMeshDataInstance();
            UnityEditor.AI.NavMeshVisualizationSettings.showNavigation--;
            if (!Application.isPlaying)
                NavMesh.onPreUpdate -= UpdateNavMesh;
        }

        protected virtual void OnValidate()
        {
            var cols = GetComponents<BoxCollider>();
            if (cols.Length != triggers.Count)
            {
                Debug.LogWarning("DO NOT CHANGE THE SIZE OF THE COLLIDER ARRAY IN THE INSPECTOR, BEHAVIOR WILL BE UNDEFINED");
                triggers.Clear();
                triggers.AddRange(cols);
                SetUpTriggers();
            }
        }

        List<NavMeshBuildSource> GetSources()
        {
            var sources = new List<NavMeshBuildSource>();

            foreach (var t in triggers)
            {
                var addIn = new List<NavMeshBuildSource>();
                NavMeshBuilder.CollectSources(t.bounds, ~(1 << LayerMask.NameToLayer("ModularNavMesh")), NavMeshCollectGeometry.PhysicsColliders, 2, new List<NavMeshBuildMarkup>(), addIn);
                //var v = sources.RemoveAll(x => x.sourceObject is Mesh && !((Mesh)x.sourceObject).isReadable);
                sources.AddRange(addIn);

                var m = new Matrix4x4();
                m.SetTRS( t.bounds.center, t.transform.rotation, Vector3.one);
                var newModifier = new NavMeshBuildSource()
                {
                    transform = m,
                    area = DefaultArea,
                    component = this,
                    shape = NavMeshBuildSourceShape.ModifierBox,
                    size = t.bounds.size + margin
                };
                sources.Add(newModifier);
            }

            return sources;
        }

        void SetUpTriggers()
        {
            if (triggers.Count < triggerBounds.Count)
            {
                for (int i = triggers.Count; i < triggerBounds.Count; i++)
                {
                    var newCol = gameObject.AddComponent<BoxCollider>();
                    triggers.Add(newCol);
                    newCol.hideFlags = HideFlags.NotEditable;
                    UnityEditorInternal.InternalEditorUtility.SetIsInspectorExpanded(newCol, false);
                    newCol.isTrigger = true;
                }
            }
            else if (triggers.Count > triggerBounds.Count)
            {
                for (int i = triggerBounds.Count; i < triggers.Count; i++)
                {
                    var col = triggers[i];
                    triggers.RemoveAt(i);

                    if (Application.isPlaying)
                        Destroy(col);
                    else
                        DestroyImmediate(col);
                }
            }

            for (int i = 0; i < triggerBounds.Count; i++)
            {
                var bound = triggerBounds[i];
                if (bound.center.IsNaN())
                {
                    if (i > 0)
                    {
                        bound.center = triggerBounds[i - 1].center;
                        bound.size = triggerBounds[i - 1].size;
                    }
                    else
                    {
                        bound.center = Vector3.zero;
                        bound.size = Vector3.one * 2;
                    }
                }


                var trigger = triggers[i];
                trigger.center = bound.center;
                trigger.size = bound.size;
            }
        }

        int GetLayer()
        {
            var usedLayers = 0;
            foreach (var trigger in triggers)
            {
                var othersModulars = Physics.OverlapBox(trigger.bounds.center, trigger.bounds.extents, trigger.transform.rotation, ~LayerMask.NameToLayer("ModularNavMesh"), QueryTriggerInteraction.Collide);
                foreach (var other in othersModulars)
                {
                    var m = other.GetComponent<ModularNavMeshSurface>();
                    if (m && m != this) usedLayers |= m.DefaultArea;
                }
            }
            for (int i = NavMesh.GetAreaFromName("WalkableA"); i <= NavMesh.GetAreaFromName("WalkableD"); i++)
            {
                if ((usedLayers & 1 << i) == 0)
                    return i;
            }
            return 1;
        }

        protected virtual void OnDrawGizmos()
        {
            if (!isActiveAndEnabled) return;
            
            foreach (var col in GetComponents<Collider>())
            {
                Gizmos.color = Color.yellow;
                //DebugShape.DrawSphere
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }

            foreach (var bound in triggerBounds)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.TransformPoint(bound.center), bound.size);
            }
        }

        protected virtual void Update()
        {
            if (getAreaAutomatically)
                defaultArea = GetLayer();
            if (rebuild || getAreaAutomatically)
            {
                SetUpTriggers();
                UpdateNavMesh();
            }
            getAreaAutomatically = false;
            rebuild = false;
        }

        private void UpdateNavMesh()
        {
            if (!isActiveAndEnabled) return;

            SetUpTriggers();
            var sources = GetSources();

            var b = new Bounds();
            foreach (var bound in triggerBounds)
                b.Encapsulate(bound);
            b.Expand(margin);

            data.position = transform.position;
            data.rotation = transform.rotation;

            var v = NavMeshBuilder.UpdateNavMeshData(data, NavMesh.GetSettingsByID(0), sources, b);

            NavMesh.RemoveNavMeshData(instance);
            instance = NavMesh.AddNavMeshData(data);
        }

        protected virtual void OnDestroy()
        {
            foreach (var col in triggers)
            {
                if(col != null)
                {
                    /*
                    if (Application.isPlaying)
                        Destroy(col);
                    else
                        DestroyImmediate(col);
                    */
                }
            }
            triggers.Clear();
        }

    }

}
