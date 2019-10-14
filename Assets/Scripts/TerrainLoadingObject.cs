using System;
using System.Collections.Generic;
using UnityEngine;
using static TerrainChunkIndex;

public class TerrainLoadingObject : MonoBehaviour
{
    /*    
    - Change terrain loading to more efficient and possibly to where the player looks
    -- Test the option of dispatching from different threads (on C#)
    -- Also make sure the chunks don't refresh ever - very inefficient and will mess with the terrain editor     
    - Adding an option to change how many points are in a chunk (without making it larger) would be nice.
    - Organize the code / documents (imports, namespaces etc.) to prepare for importing to other projects.    
     */
    public GameObject loadingObject;
    public GameObject terrainChunkPrefab;
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

    void Update()
    {
        List<TerrainChunkIndex> indicesToUpdate = TerrainChunkIndex.GetChunksToUpdate(
            loadingObject.transform.position,
            renderDistance);
        RemoveChunks(new HashSet<TerrainChunkIndex>(indicesToUpdate, new TerrainChunkIndexComparer()));
        List<TerrainChunkIndex> indicesToLoad = TerrainChunkIndex.GetChunksToLoad(
            loadingObject.transform.position,
            renderDistance,
            indicesToUpdate,
            loadedChunks);
        LoadChunks(indicesToLoad);
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
            chunkToAdd.Create(GenerateConstraintScale());
            loadedChunks.Add(chunkToAdd);
        }
    }

    private void RemoveChunks(HashSet<TerrainChunkIndex> indicesToUpdate)
    {
        for (int i = 0; i < loadedChunks.Count; i++)
        {
            TerrainChunk loadedChunk = loadedChunks[i];
            if (!indicesToUpdate.Contains(loadedChunk.index))
            {
                loadedChunk.Destroy();
                loadedChunks.Remove(loadedChunk);
            }
        }
    }

    private Vector3Int GenerateConstraintScale()
    {
        return new Vector3Int(1, 1, 1);
    }
}
