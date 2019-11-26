using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using NavMeshBuilder = UnityEngine.AI.NavMeshBuilder;

public class RealtimeNavMeshZone : SmartBeheviour
{
    [Header("NavMesh Zone")]
    public Transform FollowTarget;
    public BoxCollider ZoneArea;
    
    public AgentTarget[] TargetAgents;

    [Header("Timing")]
    public float UpdateInterval = 1f;

    //Private fields
    private AgentNavmesh[] AgentNavMeshes = new AgentNavmesh[0];
    private float lastUpdate = 0f;

    public override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        AgentNavMeshes = new AgentNavmesh[TargetAgents.Length];
        for (int i = 0; i < AgentNavMeshes.Length; i++)
        {
            AgentNavMeshes[i] = new AgentNavmesh(TargetAgents[i].ID);
        }

        _updateNavMesh();
    }
    
    void Update()
    {
        if(FollowTarget != null)
            transform.position = FollowTarget.position;

        if (Time.time - lastUpdate >= UpdateInterval)
        {
            _updateNavMesh();
            lastUpdate = Time.time;
        }
    }
    
    private void _updateNavMesh()
    {
        //If we have no target agents to build navmeshs for then return
        if (AgentNavMeshes.Length == 0)
            return;

        bool hasChanges = RealtimeNavMeshSource.UpdateAllSources(ZoneArea.bounds);
        if (hasChanges)
        {
            print("_updateNavMesh(): hasChanges");

            foreach (var agentNavMesh in AgentNavMeshes)
            {
                print("_updateNavMesh(): updating agent (" + agentNavMesh.AgentId + ")");
                var defaultBuildSettings = NavMesh.GetSettingsByIndex(agentNavMesh.AgentId);
                print("defaultBuildSettings agentId: " + defaultBuildSettings.agentTypeID);
                

                NavMeshBuilder.UpdateNavMeshData(agentNavMesh.Data, defaultBuildSettings, RealtimeNavMeshSource.AllSources, ZoneArea.bounds);
            }
        }
    }

    public override void OnDestroy()
    {
        //Remove all navmeshes that were created for each agent type
        for (int i = 0; i < AgentNavMeshes.Length; i++)
        {
            AgentNavMeshes[i].Instance.Remove();
        }

        base.OnDestroy();
    }
    
    [System.Serializable]
    public struct AgentTarget
    {
        public string Name;
        public int ID;
    }

    public class AgentNavmesh
    {
        public NavMeshData Data;
        public NavMeshDataInstance Instance;
        public int AgentId;

        public AgentNavmesh(int _AgentId)
        {
            Data = new NavMeshData();
            Instance = NavMesh.AddNavMeshData(Data);
            AgentId = _AgentId;
        }
    }
}
