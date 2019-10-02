using System;
using System.Collections.Generic;
using UnityEngine;

public class LoadingObject : MonoBehaviour
{
    public GameObject prefab;
    public ComputeShader surfaceLevelGeneratorShader;
    public ComputeShader marchingCubesGeneratorShader;
    public int renderDistance;

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
        TerrainChunkMeshGenerator.Init(surfaceLevelGeneratorShader, marchingCubesGeneratorShader);
    }

    private void InitializeChunk(TerrainChunkIndex index)
    {
        TerrainChunk chunkToAdd = new TerrainChunk(index);
        chunkToAdd.Update(new Vector3Int(1, renderDistance * 2 + 1, 1), transform.position.y - renderDistance * TerrainChunk.ChunkSize);
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
    }

    void OnDrawGizmos()
    {
        try
        {
            foreach (TerrainChunk loadedChunk in loadedChunks)
            {
                // Gizmos.DrawWireCube(loadedChunk.GetScale() / 2 + loadedChunk.constraint.position, loadedChunk.GetScale());
            }
        }
        catch (NullReferenceException) { }
    }
}
