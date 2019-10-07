﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainLoadingObject : MonoBehaviour
{
    /*        
    - Adding an option to change how many points are in a chunk (without making it larger) would be nice.
    - Change terrain loading to more efficient and possibly to where the player looks
    -- Test the option of dispatching from different threads (on C#)
    -- Also make sure the chunks don't refresh ever - very inefficient and will mess with the terrain editor
    - Implement terrain editor    
    - Organize the code / documents (imports, namespaces etc.) to prepare for importing to other projects.
     */
    public GameObject loadingObject;
    public GameObject terrainChunkPrefab;
    public ComputeShader surfaceLevelGeneratorShader;
    public ComputeShader marchingCubesGeneratorShader;
    public int renderDistance;

    private List<TerrainChunk> loadedChunks;
    private int lastTerrainY;

    void Awake()
    {
        loadedChunks = new List<TerrainChunk>();
        lastTerrainY = TerrainChunkIndex.GetTerrainY(loadingObject.transform.position);
    }

    void Start()
    {
        InitializeTerrain();
    }

    private void InitializeTerrain()
    {
        TerrainChunk.prefab = terrainChunkPrefab;
        TerrainChunk.parent = transform;
        TerrainChunkMeshGenerator.Init(surfaceLevelGeneratorShader, marchingCubesGeneratorShader);
        TerrainChunkAlterationManager.Init();
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

    private void UpdateChunks(List<TerrainChunkIndex> indicesToUpdate, bool recreate)
    {
        List<TerrainChunk> newLoadedChunks = new List<TerrainChunk>();
        foreach (TerrainChunkIndex indexToUpdate in indicesToUpdate)
        {
            foreach (TerrainChunk loadedChunk in loadedChunks)
            {
                if (loadedChunk.index.Equals(indexToUpdate))
                {
                    if (recreate && false) // Notice this is disabled
                    {
                        loadedChunk.Create(GenerateConstraintScale(), GenerateConstraintY());
                        lastTerrainY = TerrainChunkIndex.GetTerrainY(loadingObject.transform.position);
                    }
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
        return Mathf.RoundToInt(loadingObject.transform.position.y) - renderDistance * TerrainChunk.ChunkSize.y;
    }

    void Update()
    {
        List<TerrainChunkIndex> indicesToUpdate = TerrainChunkIndex.GetChunksToUpdate(
            loadingObject.transform.position,
            renderDistance);
        UpdateChunks(indicesToUpdate, TerrainChunkIndex.GetTerrainY(loadingObject.transform.position) != lastTerrainY);
        List<TerrainChunkIndex> indicesToLoad = TerrainChunkIndex.GetChunksToLoad(
            loadingObject.transform.position,
            renderDistance,
            indicesToUpdate,
            loadedChunks);
        LoadChunks(indicesToLoad);
    }
}