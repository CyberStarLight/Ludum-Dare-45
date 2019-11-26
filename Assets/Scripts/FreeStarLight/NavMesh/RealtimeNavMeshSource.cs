using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class RealtimeNavMeshSource : SmartBeheviour
{
    public static IndexedDictionary<RealtimeNavMeshSource, NavMeshBuildSource> AllSourceObjects = new IndexedDictionary<RealtimeNavMeshSource, NavMeshBuildSource>();
    public static List<NavMeshBuildSource> AllSources { get { return AllSourceObjects.Values; } }
    public static bool ItemsListChanged { get; private set; }

    public MeshFilter MeshFilterSource;
    public MeshRenderer MeshRenderer;
    public BoxCollider BoxSource;
    public BoxCollider AreaModifier;
    public Terrain TerrainSource;

    public string AreaName = "Walkable";
    public bool isStatic = true;
    
    private Vector3 lastPosition;
    private Vector3 lastScale;
    private Vector3 lastRotation;
    private Mesh mesh;
    private Bounds bounds;

    public override void Awake()
    {
        base.Awake();

        UpdateSource(true);
        ItemsListChanged = true;
    }

    public override void OnDestroy()
    {
        AllSourceObjects.Remove(this);
        ItemsListChanged = true;

        base.OnDestroy();
    }

    public static bool UpdateAllSources(Bounds? onlyUpdateInsideBounds = null)
    {
        bool hasChanges = false;

        var items = AllSourceObjects.Keys.ToArray();
        foreach (var item in items)
        {
            //Skip item if it's out of bounds
            if (onlyUpdateInsideBounds.HasValue)
            {
                if (!item.isStatic)
                {
                    bool hasLeftZone = item.UpdateBounds(onlyUpdateInsideBounds.Value);
                    if (!hasChanges && hasLeftZone)
                        hasChanges = true;
                }

                if (!onlyUpdateInsideBounds.Value.Intersects(item.bounds))
                    continue;
            }

            bool itemChanged = item.UpdateSource();

            if (!hasChanges && itemChanged)
                hasChanges = true;
        }

        if (ItemsListChanged)
        {
            ItemsListChanged = false;
            return true;
        }
        else
        {
            return hasChanges;
        }
    }
    
    public bool UpdateBounds(Bounds targetBounds)
    {
        var oldBounds = bounds;

        if (MeshRenderer != null)
            bounds = MeshRenderer.bounds;
        else if (BoxSource != null)
            bounds = BoxSource.bounds;
        else if (AreaModifier != null)
            bounds = AreaModifier.bounds;
        else if (TerrainSource != null)
            bounds = new Bounds(TerrainSource.terrainData.bounds.center + TerrainSource.transform.position, TerrainSource.terrainData.bounds.size);

        if(oldBounds.Intersects(targetBounds) && !bounds.Intersects(targetBounds))
        {
            UpdateSource();
            return true;
        }

        return false;
    }

    public bool UpdateSource(bool forceUpdate = false)
    {
        if (isStatic && !forceUpdate)
            return false;

        if (
            transform.position == lastPosition &&
            transform.eulerAngles == lastRotation &&
            transform.lossyScale == lastScale
            )
            return false;

        print("UpdateSource(): found changes");

        lastPosition = transform.position;
        lastRotation = transform.eulerAngles;
        lastScale = transform.lossyScale;
        
        var shapeType = GetShapeType();
        if(!shapeType.HasValue)
        {
            Debug.LogError("RealtimeNavMeshSource - UpdateSource(): invalid source");
            return false;
        }

        Object sourceObject = null;
        Matrix4x4 sourceTransform;
        Vector3 sourceSize = Vector3.zero;
        switch (shapeType.Value)
        {
            case NavMeshBuildSourceShape.Mesh:
                sourceObject = MeshFilterSource.sharedMesh;
                sourceTransform = MeshFilterSource.gameObject.transform.localToWorldMatrix;
                bounds = MeshRenderer.bounds;
                break;
            case NavMeshBuildSourceShape.Terrain:
                sourceObject = TerrainSource.terrainData;
                sourceTransform = Matrix4x4.TRS(TerrainSource.transform.position, Quaternion.identity, Vector3.one);
                bounds = new Bounds(TerrainSource.terrainData.bounds.center + TerrainSource.transform.position, TerrainSource.terrainData.bounds.size);
                break;
            case NavMeshBuildSourceShape.Box:
                sourceTransform = BoxSource.transform.localToWorldMatrix;
                sourceSize = BoxSource.bounds.size;
                bounds = BoxSource.bounds;
                break;
            case NavMeshBuildSourceShape.ModifierBox:
                sourceTransform = AreaModifier.transform.localToWorldMatrix;
                sourceSize = AreaModifier.bounds.size;
                bounds = AreaModifier.bounds;
                break;
            default:
                Debug.LogError("RealtimeNavMeshSource - UpdateSource(): invalid source");
                return false;
        }

        var compiledSource = new NavMeshBuildSource()
        {
            shape = shapeType.Value,
            sourceObject = sourceObject,
            transform = sourceTransform,
            size = sourceSize,
            area = NavMesh.GetAreaFromName(AreaName)
        };
        
        AllSourceObjects[this] = compiledSource;
        
        return true;
    }
    
    public NavMeshBuildSourceShape? GetShapeType()
    {
        if (MeshFilterSource != null)
            return NavMeshBuildSourceShape.Mesh;
        else if (BoxSource != null)
            return NavMeshBuildSourceShape.Box;
        else if (AreaModifier != null)
            return NavMeshBuildSourceShape.ModifierBox;
        else if (TerrainSource != null)
            return NavMeshBuildSourceShape.Terrain;
        else
            return null;
    }
    
}
