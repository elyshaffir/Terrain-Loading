using System;
using System.Collections.Generic;
using UnityEngine;

public class LoadingObject : MonoBehaviour
{
    /*    
    - Implement terrain editor
    - Implement controller for loading object to see how it looks in-game    
    - Organize the code / documents (imports, namespaces etc.) to prepare for importing to other projects.
     */
    public GameObject terrainChunkPrefab;
    public ComputeShader surfaceLevelGeneratorShader;
    public ComputeShader marchingCubesGeneratorShader;
    public int renderDistance;

    private List<TerrainChunk> loadedChunks;
    private int lastTerrainY;

    void Awake()
    {
        loadedChunks = new List<TerrainChunk>();
        lastTerrainY = TerrainChunkIndex.GetTerrainY(transform.position);
    }

    void Start()
    {
        InitializeTerrain();
    }

    private void InitializeTerrain()
    {
        TerrainChunk.prefab = terrainChunkPrefab;
        TerrainChunkMeshGenerator.Init(surfaceLevelGeneratorShader, marchingCubesGeneratorShader);
    }

    private void LoadChunks(List<TerrainChunkIndex> indicesToLoad)
    {
        foreach (TerrainChunkIndex indexToLoad in indicesToLoad)
        {
            TerrainChunk chunkToAdd = new TerrainChunk(indexToLoad);
            chunkToAdd.Create(GenerateConstraintScale(), GenerateConstraintY());
            loadedChunks.Add(chunkToAdd);
        }
    }

    private void UpdateChunks(List<TerrainChunkIndex> indicesToUpdate)
    {
        List<TerrainChunk> newLoadedChunks = new List<TerrainChunk>();
        foreach (TerrainChunkIndex indexToUpdate in indicesToUpdate)
        {
            foreach (TerrainChunk loadedChunk in loadedChunks)
            {
                if (loadedChunk.index.Equals(indexToUpdate))
                {
                    loadedChunk.Create(GenerateConstraintScale(), GenerateConstraintY());
                    newLoadedChunks.Add(loadedChunk);
                }
            }
        }
        foreach (TerrainChunk loadedChunk in loadedChunks)
        {
            if (!newLoadedChunks.Contains(loadedChunk))
            {
                loadedChunk.Destroy();
            }
        }
        loadedChunks = newLoadedChunks;
    }

    private Vector3Int GenerateConstraintScale()
    {
        return new Vector3Int(1, renderDistance * 2 + 1, 1);
    }

    private float GenerateConstraintY()
    {
        return Mathf.RoundToInt(transform.position.y) - renderDistance * TerrainChunk.ChunkSize;
    }

    void Update()
    {
        List<TerrainChunkIndex> indicesToUpdate = TerrainChunkIndex.GetChunksToUpdate(
            transform.position,
            renderDistance);
        if (TerrainChunkIndex.GetTerrainY(transform.position) != lastTerrainY)
        {
            UpdateChunks(indicesToUpdate);
            lastTerrainY = TerrainChunkIndex.GetTerrainY(transform.position);
        }
        List<TerrainChunkIndex> indicesToLoad = TerrainChunkIndex.GetChunksToLoad(
            transform.position,
            renderDistance,
            indicesToUpdate,
            loadedChunks);
        LoadChunks(indicesToLoad);
    }
}
