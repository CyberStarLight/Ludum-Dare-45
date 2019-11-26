using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyberTriplanar : MonoBehaviour {

    [Header("Material")]
    public Material TriplanarMaterial;

    [Header("Atlas Tiles")]
    public int TopTileX;
    public int TopTileY;
    public int BottomTileX;
    public int BottomTileY;

    [Header("Color")]
    public HSVShift TopHSV;
    public HSVShift BottomHSV;

    [Header("Adjustments")]
    [Range(0.01f, 10f)]
    public float BottomScale = 1f;

    [Range(0.01f, 10f)]
    public float TopScale = 1f;

    [Range(0f, 1f)]
    public float TopSpread = 0.5f;

    [Range(-2f, 2f)]
    public float EnhanceEdge = 0.5f;

    public bool AutoUpdateOn = false;

    private void ForceUpdate()
    {
        print("ForceUpdate()");
        UpdateMesh();
    }

    private void OnValidate()
    {
        if(AutoUpdateOn)
            UpdateMesh();
    }

    public void UpdateMesh()
    {
        MeshFilter meshFilter;
        Mesh mesh;
#if UNITY_EDITOR
        meshFilter = GetComponent<MeshFilter>();
        Mesh meshCopy = Mesh.Instantiate(meshFilter.sharedMesh) as Mesh;
        meshCopy.name = "Edited Mesh (" + GetInstanceID() + ")";
        mesh = meshFilter.mesh = meshCopy;
#else
            //do this in play mode
            meshFilter = GetComponent<MeshFilter>();
            mesh = meshFilter.mesh;
#endif

        //var mesh = Mesh.Instantiate(OriginalMesh) as Mesh;
        mesh.RecalculateNormals(0f);
        

        int verticesLength = mesh.vertices.Length;
        var newUVs = new Vector2[verticesLength];
        var newUVs2 = new Vector2[verticesLength];
        var newColors = new Color[verticesLength];
        var newtangents = new Vector4[verticesLength];
        var newNormals = new Vector3[verticesLength];

        float combinedTopTile = (float)((int)(TopTileX * 100f)) + (TopTileY / 100f);
        Vector2 topTile = new Vector2(combinedTopTile, EnhanceEdge);
        //Vector2 topTile = new Vector2(TopTileX, TopTileY);
        Vector2 bottomTile = new Vector2(BottomTileX, BottomTileY);
        //print(bottomTile);
        //Vector4 bottomSideColor = new Vector4(SideBottomColor.r, SideBottomColor.g, SideBottomColor.b, 1f);

        float topSpread = -3 + (TopSpread * 6);
        Color topColor = new Color(TopHSV.Hue, TopHSV.Saturation, TopHSV.Value, topSpread);

        float combinedScale = (float)((int)(TopScale * 100f)) + (BottomScale / 100f);
        Color bottomColor = new Color(BottomHSV.Hue, BottomHSV.Saturation, BottomHSV.Value, combinedScale);

        for (int i = 0; i < verticesLength; i++)
        {
            //if (i < verticesLength / 2)
            //    newColors[i] = TopColor;
            //else
            //    newColors[i] = TopColor2;

            newUVs[i] = topTile;
            newUVs2[i] = bottomTile;
            newtangents[i] = bottomColor;
            newColors[i] = topColor;
        }

        var renderer = GetComponent<Renderer>();
        //var materials = renderer.sharedMaterials;
        Material[] materials = new Material[mesh.subMeshCount];
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = TriplanarMaterial;
        }
        renderer.materials = materials;
        //renderer.materials = new Material[] { TriplanarMaterial };

        mesh.uv = newUVs;
        mesh.uv2 = newUVs2;
        mesh.colors = newColors;
        mesh.tangents = newtangents;
        //set mesh
        meshFilter.mesh = mesh;

    }
}

[System.Serializable]
public struct HSVShift
{
    [Range(-1f, 1f)]
    public float Hue;

    [Range(-1f, 1f)]
    public float Saturation;

    [Range(-1f, 1f)]
    public float Value;

}