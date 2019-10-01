using System;
using System.Collections.Generic;
using UnityEngine;

public class LoadingObject : MonoBehaviour
{
    public GameObject prefab;
    public ComputeShader surfaceLevelGeneratorShader;
    public ComputeShader marchingCubesGeneratorShader;
    public int renderDistance = 2;

    private List<TerrainChunk> loadedChunks;

    void Awake()
    {
        loadedChunks = new List<TerrainChunk>();
    }

    void Start()
    {
        TerrainChunk.prefab = prefab;
        TerrainChunk.surfaceLevelGeneratorShader = surfaceLevelGeneratorShader;
        TerrainChunk.marchingCubesGeneratorShader = marchingCubesGeneratorShader;
        loadedChunks.Add(new TerrainChunk(new TerrainChunkIndex(0, 0)));
        loadedChunks[0].GenerateMesh();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            UpdateChunk(new Vector3Int(-1, 0, 0));
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            UpdateChunk(new Vector3Int(0, -1, 0));
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            UpdateChunk(new Vector3Int(0, 0, -1));
        }
        if (Input.GetKeyDown(KeyCode.Q)) // Streaches
        {
            UpdateChunk(new Vector3Int(1, 0, 0));
        }
        if (Input.GetKeyDown(KeyCode.W)) // Heightens and shortes (more than half)
        {
            UpdateChunk(new Vector3Int(0, 1, 0));
        }
        if (Input.GetKeyDown(KeyCode.E)) // Halfs
        {
            UpdateChunk(new Vector3Int(0, 0, 1));
        }
    }

    private void UpdateChunk(Vector3Int scale)
    {
        loadedChunks[0].Update(scale);
        loadedChunks[0].GenerateMesh();
    }

    void OnDrawGizmos()
    {
        try
        {
            Gizmos.DrawWireCube(loadedChunks[0].GetScale() / 2, loadedChunks[0].GetScale());
        }
        catch (NullReferenceException) { }
    }
}
