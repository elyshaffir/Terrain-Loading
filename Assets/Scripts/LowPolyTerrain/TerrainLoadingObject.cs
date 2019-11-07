using System.Collections.Generic;
using LowPolyTerrain.Chunk;
using LowPolyTerrain.MeshGeneration;
using UnityEngine;

namespace LowPolyTerrain
{
    class TerrainLoadingObject : MonoBehaviour
    {
#pragma warning disable 649
        public GameObject loadingObject;
        public GameObject terrainChunkPrefab;
        public ComputeShader surfaceLevelGeneratorShader;
        public ComputeShader marchingCubesGeneratorShader;
        public ComputeShader getPointsToAlterShader;
        public ComputeShader prepareRelevantCubesShader;
        public int renderDistance;

        List<TerrainChunk> loadedChunks;
        TerrainChunkIndex currentTerrainChunkIndex;

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

        void InitializeTerrain()
        {
            TerrainChunk.prefab = terrainChunkPrefab;
            TerrainChunk.parent = transform;
            TerrainChunkMeshGenerator.Init(surfaceLevelGeneratorShader, marchingCubesGeneratorShader, getPointsToAlterShader, prepareRelevantCubesShader);
            TerrainChunkLoadingManager.Init();
            TerrainChunkAlterationManager.Init();
        }

        void InitializeChunks()
        {
            List<TerrainChunkIndex> indicesToUpdate = TerrainChunkIndex.GetChunksToUpdate(
                        loadingObject.transform.position,
                        renderDistance);
            LoadChunks(indicesToUpdate);
        }

        void Update()
        {
            TerrainChunkLoadingManager.PhaseOne(renderDistance);
            TerrainChunkIndex newTerrainChunkIndex = TerrainChunkIndex.FromVector(loadingObject.transform.position);
            TerrainChunkIndex distance = newTerrainChunkIndex - currentTerrainChunkIndex;
            if (!distance.Equals(TerrainChunkIndex.zero))
            {
                currentTerrainChunkIndex = newTerrainChunkIndex;
                ManageChunks(distance);
            }
            TerrainChunkLoadingManager.PhaseTwo();
        }

        void ManageChunks(TerrainChunkIndex distance)
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
                    TerrainChunkIndex gridPosInverted = initialGridPos.Invert(axisToInvert);
                    gridPosInverted -= axisToInvert;
                    TerrainChunkIndex newIndex = currentTerrainChunkIndex + gridPosInverted;
                    TerrainChunk newChunk = new TerrainChunk(newIndex);

                    loadedChunks[i] = newChunk;
                    TerrainChunkLoadingManager.chunksToLoad.Add(newChunk);
                }
            }
        }

        void LoadChunks(List<TerrainChunkIndex> indicesToLoad)
        {
            foreach (TerrainChunkIndex indexToLoad in indicesToLoad)
            {
                LoadChunk(indexToLoad);
            }
        }

        void LoadChunk(TerrainChunkIndex indexToLoad)
        {
            TerrainChunk chunkToAdd = new TerrainChunk(indexToLoad);
            TerrainChunkLoadingManager.chunksToLoad.Add(chunkToAdd);
            loadedChunks.Add(chunkToAdd);
        }
    }
}