using System;
using System.Collections.Generic;
using UnityEngine;

public class LoadingObject : MonoBehaviour
{
    public GameObject prefab;
    public ComputeShader surfaceLevelGeneratorShader;
    public ComputeShader marchingCubesGeneratorShader;
    public int renderDistance = 1;

    private List<TerrainChunk> loadedChunks;

    void Awake()
    {
        loadedChunks = new List<TerrainChunk>();
    }

    void Start()
    {
        InitializeTerrain();
    }

    private void InitializeTerrain()
    {
        TerrainChunk.prefab = prefab;
        TerrainChunk.surfaceLevelGeneratorShader = surfaceLevelGeneratorShader;
        TerrainChunk.marchingCubesGeneratorShader = marchingCubesGeneratorShader;
    }

    private void InitializeChunk(TerrainChunkIndex index)
    {
        TerrainChunk chunkToAdd = new TerrainChunk(index);
        chunkToAdd.GenerateMesh();
        loadedChunks.Add(chunkToAdd);
    }

    void Update()
    {
        List<TerrainChunkIndex> indicesToLoad = TerrainChunkIndex.GetChunksToLoad(
            transform.position,
            renderDistance,
            loadedChunks);
        foreach (TerrainChunkIndex index in indicesToLoad)
        {
            InitializeChunk(index);
        }
        // ControlChunkSize();        
    }

    private void ControlChunkSize()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            UpdateChunk(new Vector3Int(-1, 0, 0));
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            UpdateChunk(new Vector3Int(1, 0, 0));
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            UpdateChunk(new Vector3Int(0, -1, 0));
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            UpdateChunk(new Vector3Int(0, 1, 0));
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            UpdateChunk(new Vector3Int(0, 0, -1));
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            UpdateChunk(new Vector3Int(0, 0, 1));
        }
    }

    private void UpdateChunk(Vector3Int scale)
    {
        loadedChunks[0].Update(scale);
        loadedChunks[0].GenerateMesh();
        loadedChunks[0].DebugFunciton();
    }

    void OnDrawGizmos()
    {
        try
        {
            foreach (TerrainChunk loadedChunk in loadedChunks)
            {
                // Gizmos.DrawWireCube(loadedChunk.GetScale() / 2 + loadedChunk.index.ToPosition(), loadedChunk.GetScale());
            }
        }
        catch (NullReferenceException) { }
    }
}
