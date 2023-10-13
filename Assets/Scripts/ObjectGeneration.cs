using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGenerationEmpty : MonoBehaviour
{
    public virtual void Generate(Texture2D texture, GameObject island, IslandData islandData, int biomeType)
    {
        print("Generating...");
    }
}
public class ObjectGeneration : ObjectGenerationEmpty
{
    WorldGeneration worldGenerationManager;

    public void Awake()
    {
        worldGenerationManager = GetComponent<WorldGeneration>();
    }

    //Generates a new perlin noise for the object
    public override void Generate(Texture2D texture, GameObject island, IslandData islandData, int biomeType)
    {
        GameObject Parent = new GameObject(islandData.islanDataInformation[biomeType].objectsToSpawn[0].name);

        float offsetXTrees = worldGenerationManager.seed * 10;
        float offsetZTrees = worldGenerationManager.seed * 10;

        for (int z = 0; z < worldGenerationManager.length; z++)
        {
            for (int x = 0; x < worldGenerationManager.width; x++)
            {
                //uses the same perlin noise as the island to get the height value its supposed to place the objects on.
                //also creates new perlin noise for the new objects
                var perlinnoiseIsland = texture.GetPixel(x * 7, z * 7).g;
                var perlinnoisePlace = Mathf.PerlinNoise((x + offsetXTrees) / worldGenerationManager.scale, (z + offsetZTrees) / worldGenerationManager.scale);
                var placeValue = perlinnoisePlace;

                var perlinnoise = Mathf.PerlinNoise((x + worldGenerationManager.seed) / worldGenerationManager.scale, (z + worldGenerationManager.seed) / worldGenerationManager.scale);
                var height = (perlinnoise - perlinnoiseIsland) * worldGenerationManager.amplitude; ;

                if (placeValue > islandData.islanDataInformation[biomeType].spawnfrequency && perlinnoise - perlinnoiseIsland > islandData.islanDataInformation[biomeType].aboveFloat && perlinnoise - perlinnoiseIsland < islandData.islanDataInformation[biomeType].belowFloat)
                {
                    var randomise = Random.Range(0, 100);

                    if (randomise < islandData.islanDataInformation[biomeType].spawnChance)
                    {
                        var randomizedObject = Random.Range(0, islandData.islanDataInformation[biomeType].objectsToSpawn.Count);
                        Instantiate(islandData.islanDataInformation[biomeType].objectsToSpawn[randomizedObject], new Vector3(x, height, z), Quaternion.identity, Parent.transform);
                    }
                }


            }
        }

        Parent.transform.SetParent(island.transform);
    }
}