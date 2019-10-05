using System;
using System.Collections.Generic;
using UnityEngine;

public class LoadingObject : MonoBehaviour
{
    /*    
    - Change vertical loading to the same way as horizontal loading - Maybe not...
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

    private void UpdateChunks(List<TerrainChunkIndex> indicesToUpdate, bool recreate)
    {
        List<TerrainChunk> newLoadedChunks = new List<TerrainChunk>();
        foreach (TerrainChunkIndex indexToUpdate in indicesToUpdate)
        {
            foreach (TerrainChunk loadedChunk in loadedChunks)
            {
                if (loadedChunk.index.Equals(indexToUpdate))
                {
                    if (recreate)
                    {
                        loadedChunk.Create(GenerateConstraintScale(), GenerateConstraintY());
                        lastTerrainY = TerrainChunkIndex.GetTerrainY(transform.position);
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
        return Mathf.RoundToInt(transform.position.y) - renderDistance * TerrainChunk.ChunkSize.y;
    }

    void Update()
    {
        List<TerrainChunkIndex> indicesToUpdate = TerrainChunkIndex.GetChunksToUpdate(
            transform.position,
            renderDistance);
        UpdateChunks(indicesToUpdate, TerrainChunkIndex.GetTerrainY(transform.position) != lastTerrainY);
        List<TerrainChunkIndex> indicesToLoad = TerrainChunkIndex.GetChunksToLoad(
            transform.position,
            renderDistance,
            indicesToUpdate,
            loadedChunks);
        LoadChunks(indicesToLoad);
    }
}
