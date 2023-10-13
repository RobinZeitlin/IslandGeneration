using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Data", menuName = "Island/BiomeData", order = 1)]
public class IslandData : ScriptableObject
{
    [SerializeField]
    public Gradient colorGradient;
    public List<IslanDataInformation> islanDataInformation = new List<IslanDataInformation>();    
    public float amplitude;
    public float scale;
}

[Serializable]
public class IslanDataInformation
{
    [SerializeField]
    public List<GameObject> objectsToSpawn = new List<GameObject>();

    [Header("Object spawn demands")]
    [Range(0f, 1f)]
    [SerializeField]
    public float aboveFloat;

    [Range(0f, 1f)]
    [SerializeField]
    public float belowFloat;

    [Range(0f, 1f)]
    [SerializeField]
    public float spawnfrequency;

    [Range(0f, 100f)]
    [SerializeField]
    public float spawnChance;
}
