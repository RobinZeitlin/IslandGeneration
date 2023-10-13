using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEditor.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEditor.AddressableAssets.Settings;
using System;

public class WorldGeneration : MonoBehaviour
{

    public int width { get; private set; }
    public int length { get; private set; }
    public int seed { get; private set; }

    [Header("World Information")]
    public int amountOfIslands;

    [HideInInspector]
    public float scale;
    [HideInInspector]
    public float amplitude;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    [Header("Adressable guideline")]
    List<IslandData> biomeTypes = new();

    List<Texture2D> perlinNoiseIsland = new();

    ObjectGeneration objectGeneration;

    Material terrainMaterial;

    Vector2 oldOffset;

    //Adressables References 

    Action<IslandData> methodToCallBiomes;
    Action<Texture2D> methodToCallPerlinOverlays;

    AssetLabelReference biomeLabel;
    AssetLabelReference perlinNoiseLabel;
    void Start()
    {
        biomeLabel = new AssetLabelReference();
        biomeLabel.labelString = "Biomes";

        perlinNoiseLabel = new AssetLabelReference();
        perlinNoiseLabel.labelString = "PerlinNoise";

        Addressables.LoadAssetsAsync<IslandData>(biomeLabel, methodToCallBiomes).Completed += GetBiomeTypes;
        Addressables.LoadAssetsAsync<Texture2D>(perlinNoiseLabel, methodToCallPerlinOverlays).Completed += GetPerlinOverlays;

        seed = 100;
        width = 200;
        length = 200;
    }

    private void GetPerlinOverlays(AsyncOperationHandle<IList<Texture2D>> assets)
    {
        foreach (var i in assets.Result)
        {
            perlinNoiseIsland.Add(i);
        }
    }

    private void GetBiomeTypes(AsyncOperationHandle<IList<IslandData>> assets)
    {
        foreach (var i in assets.Result)
        {
            biomeTypes.Add(i);
        }
        InitalizeIslands();
    }

    //Start instantiating meshes/islands
    private void InitalizeIslands()
    {
        objectGeneration = GetComponent<ObjectGeneration>();
        terrainMaterial = Resources.Load("TerrainMaterial") as Material;

        for (int i = 0; i < amountOfIslands; i++)
        {
            GameObject newIsland = new GameObject();
            newIsland.name = "Island " + i.ToString();
            AssignWorld(newIsland);
        }
    }

    //Gives the islands the appropriate information from the biome chosen
    public void AssignWorld(GameObject island)
    {
        seed = UnityEngine.Random.Range(0, 1000);

        island.AddComponent<MeshRenderer>().material = terrainMaterial;
        
        meshFilter = island.AddComponent<MeshFilter>();
        meshCollider = island.AddComponent<MeshCollider>();

        RandomiseIslandValues();

        var biomeType = UnityEngine.Random.Range(0, biomeTypes.Count); 
        var perlinint = UnityEngine.Random.Range(0, perlinNoiseIsland.Count);

        amplitude = biomeTypes[biomeType].amplitude;
        scale = biomeTypes[biomeType].scale;

        Mesh terrainMesh = GenerateTerrainMesh(island, perlinint, biomeType);

        if (terrainMesh != null)
            SetWorld(terrainMesh);

        RandomizePositionOfIsland(island);
    }

    public void RandomiseIslandValues()
    {
        amplitude += UnityEngine.Random.Range(amplitude - 1,  amplitude + 1);
        scale += UnityEngine.Random.Range(-1, 1); 
    }

    private void RandomizePositionOfIsland(GameObject island)
    {
        var currOffset = Offset();
        oldOffset = currOffset;
        while(Vector2.Distance(oldOffset, currOffset) >= 200)
        {
            currOffset = Offset();
        }
        oldOffset = currOffset;
        
        island.transform.position += new Vector3(currOffset.x, 0, currOffset.y);
    }

    public Vector2 Offset()
    {
        Vector2 randomizedOffset = new Vector2(UnityEngine.Random.Range(-2000, 2000), UnityEngine.Random.Range(-2000, 2000));
        return randomizedOffset;
    }

    public void SetWorld(Mesh terrain)
    {
        meshFilter.mesh = terrain;
        meshCollider.sharedMesh = terrain;
    }
   

    //Creates the mesh and changes the y position of the vertices based on the perlin noise value created.
    Mesh GenerateTerrainMesh(GameObject island, int perlinInt, int biomeType)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[width * length];
        
        int[] triangles = new int[(width - 1) * (length - 1) * 6];

        int triangleIndex = 0;


        Color[] color = new Color[vertices.Length];

        for (int z = 0; z < length; z++)
        {
            for (int x = 0; x < width; x++)
            {

                //Perlin noise creation and evaluation
                var perlinnoiseIsland = perlinNoiseIsland[perlinInt].GetPixel(x * 7, z * 7).g;
                var perlinnoise = Mathf.PerlinNoise((x + seed) / scale, (z + seed) / scale);
                var height = (perlinnoise - perlinnoiseIsland) * amplitude;
                vertices[z * width + x] = new Vector3(x, height, z);
                //Uses the combination of the two noises and sets color of mesh
                color[z * width + x] = biomeTypes[biomeType].colorGradient.Evaluate(perlinnoise - perlinnoiseIsland);

                //places the triangles accordingly
                if (x < width - 1 && z < length - 1)
                {
                    triangles[triangleIndex] = z * width + x;
                    triangles[triangleIndex + 1] = z * width + x + width;
                    triangles[triangleIndex + 2] = z * width + x + 1;
                    triangles[triangleIndex + 3] = z * width + x + 1;
                    triangles[triangleIndex + 4] = z * width + x + width;
                    triangles[triangleIndex + 5] = z * width + x + width + 1;
                    triangleIndex += 6;
                }
            }
        }
            //Generates all the objects on the island
            for (int i = 0; i < biomeTypes[biomeType].islanDataInformation.Count; i++)
            {
                objectGeneration.Generate(perlinNoiseIsland[perlinInt], island, biomeTypes[biomeType], i);
            }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = color;
        mesh.RecalculateNormals();

        return mesh;
    }
    private void OnDisable()
    {
        Addressables.LoadAssetsAsync<IslandData>(biomeLabel, methodToCallBiomes).Completed -= GetBiomeTypes;
        Addressables.LoadAssetsAsync<Texture2D>(perlinNoiseLabel, methodToCallPerlinOverlays).Completed -= GetPerlinOverlays;
    }
}
