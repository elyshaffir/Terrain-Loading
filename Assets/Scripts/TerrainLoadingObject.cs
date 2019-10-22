using System.Collections.Generic;
using UnityEngine;

public class TerrainLoadingObject : MonoBehaviour
{
    /*
    - Altering terrain on a REALLY large area is somewhat slow
    - Make chunk size more customizable
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
        InitializeChunks();
        currentTerrainChunkIndex = TerrainChunkIndex.FromVector(loadingObject.transform.position);
    }

    private void InitializeTerrain()
    {
        TerrainChunk.prefab = terrainChunkPrefab;
        TerrainChunk.parent = transform;
        TerrainChunkMeshGenerator.Init(surfaceLevelGeneratorShader, marchingCubesGeneratorShader);
        TerrainChunkLoadingManager.Init();
        TerrainChunkAlterationManager.Init();
    }

    private void InitializeChunks()
    {
        List<TerrainChunkIndex> indicesToUpdate = TerrainChunkIndex.GetChunksToUpdate(
                    loadingObject.transform.position,
                    renderDistance);
        LoadChunks(indicesToUpdate);
    }

    void Update()
    {
        TerrainChunkLoadingManager.PhaseOne();
        TerrainChunkIndex newTerrainChunkIndex = TerrainChunkIndex.FromVector(loadingObject.transform.position);
        TerrainChunkIndex distance = newTerrainChunkIndex - currentTerrainChunkIndex;
        if (!distance.Equals(TerrainChunkIndex.zero))
        {
            currentTerrainChunkIndex = newTerrainChunkIndex;
            ManageChunks(distance);
        }
        TerrainChunkLoadingManager.PhaseTwo();
    }

    private void ManageChunks(TerrainChunkIndex distance)
    {
        // If the chunk is out of range, invert it around currentTerrainChunkIndex        
        int c = loadedChunks.Count;
        for (int i = 0; i < c; i++)
        {
            TerrainChunk loadedChunk = loadedChunks[i];
            if (!loadedChunk.index.InRange(currentTerrainChunkIndex, renderDistance))
            {
                loadedChunk.Destroy();
                TerrainChunkLoadingManager.chunksToLoad.Remove(loadedChunk);

                TerrainChunkIndex initialTerrainChunkIndex = currentTerrainChunkIndex - distance;
                TerrainChunkIndex initialGridPos = loadedChunk.index - initialTerrainChunkIndex;
                TerrainChunkIndex axisToInvert = distance.Sign().Abs(); // if the value is not 0, need to invert on that axis
                TerrainChunkIndex gridPosInverted = initialGridPos * new TerrainChunkIndex(
                    (axisToInvert.x == 0) ? 1 : -1,
                    (axisToInvert.y == 0) ? 1 : -1,
                    (axisToInvert.z == 0) ? 1 : -1
                );
                gridPosInverted -= axisToInvert;
                TerrainChunkIndex newIndex = currentTerrainChunkIndex + gridPosInverted;
                TerrainChunk newChunk = new TerrainChunk(newIndex);

                loadedChunks[i] = newChunk;
                TerrainChunkLoadingManager.chunksToLoad.Add(newChunk); // Change to Queue                
            }
        }
    }

    private void LoadChunks(List<TerrainChunkIndex> indicesToLoad)
    {
        foreach (TerrainChunkIndex indexToLoad in indicesToLoad)
        {
            LoadChunk(indexToLoad);
        }
    }

    private void LoadChunk(TerrainChunkIndex indexToLoad)
    {
        TerrainChunk chunkToAdd = new TerrainChunk(indexToLoad);
        TerrainChunkLoadingManager.chunksToLoad.Add(chunkToAdd); // Change to Queue
        loadedChunks.Add(chunkToAdd);
    }
}
