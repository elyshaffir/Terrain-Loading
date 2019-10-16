using System.Collections.Generic;
using UnityEngine;
using static TerrainChunkIndex;

public class TerrainLoadingObject : MonoBehaviour
{
    /*
        NEW LOADING SYSTEM
        ------------------

        - Preformence issues rise when more than one chunk is being generated
        - Look into a static class in which all of the compute-shader-dispatching will happen, that will allow working on multiple chunks at once
        -- And will solve the problem where you can't do many things from a separate thread / job.
        --- This will be probably implemented via a static Queue and an UpdateJobs() method being called every frame.
     */
    /*
    - Organize the code / documents (imports, namespaces etc.) to prepare for importing to other projects.    
     */
    public GameObject loadingObject;
    public GameObject terrainChunkPrefab;
    public ComputeShader surfaceLevelGeneratorShader;
    public ComputeShader marchingCubesGeneratorShader;
    public int renderDistance;

    private List<TerrainChunk> loadedChunks;
    private TerrainChunkIndex currentTerrainChunkIndex;

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
        TerrainChunk.prefab = terrainChunkPrefab;
        TerrainChunk.parent = transform;
        TerrainChunkMeshGenerator.Init(surfaceLevelGeneratorShader, marchingCubesGeneratorShader);
        TerrainChunkLoadingManager.Init();
        TerrainChunkAlterationManager.Init();
    }

    void Update()
    {
        TerrainChunkLoadingManager.PhaseOne();
        TerrainChunkIndex newTerrainChunkIndex = TerrainChunkIndex.FromVector(loadingObject.transform.position);
        if (!newTerrainChunkIndex.Equals(currentTerrainChunkIndex))
        {
            ManageChunks();
            currentTerrainChunkIndex = newTerrainChunkIndex;
        }
        TerrainChunkLoadingManager.PhaseTwo();
    }

    private void ManageChunks()
    {
        List<TerrainChunkIndex> indicesToUpdate = TerrainChunkIndex.GetChunksToUpdate(
                    loadingObject.transform.position,
                    renderDistance);
        RemoveChunks(new HashSet<TerrainChunkIndex>(indicesToUpdate, new TerrainChunkIndexComparer()));
        List<TerrainChunkIndex> indicesToLoad = TerrainChunkIndex.GetChunksToLoad(
            loadingObject.transform.position,
            indicesToUpdate,
            loadedChunks);
        LoadChunks(indicesToLoad);
    }

    private void LoadChunks(List<TerrainChunkIndex> indicesToLoad)
    {
        foreach (TerrainChunkIndex indexToLoad in indicesToLoad)
        {
            TerrainChunk chunkToAdd = new TerrainChunk(indexToLoad);
            TerrainChunkLoadingManager.chunksToLoad.Add(chunkToAdd);
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
                loadedChunks.RemoveAt(i);
                TerrainChunkLoadingManager.chunksToLoad.Remove(loadedChunk);
            }
        }
    }
}
