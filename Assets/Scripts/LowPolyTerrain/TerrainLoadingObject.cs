using System;
using System.Collections.Generic;
using LowPolyTerrain.Chunk;
using LowPolyTerrain.MeshGeneration;
using UnityEngine;
using static LowPolyTerrain.Chunk.TerrainChunkIndex;

namespace LowPolyTerrain
{
    class TerrainLoadingObject : MonoBehaviour
    {
#pragma warning disable 649
        public GameObject loadingObject;
        public GameObject terrainChunkPrefab;
        public ComputeShader surfaceLevelGeneratorShader;
        public ComputeShader marchingCubesGeneratorShader;
        public ComputeShader alterPointsShader;
        public ComputeShader prepareRelevantCubesShader;
        public int renderDistance;

        public Dictionary<TerrainChunkIndex, TerrainChunk> loadedChunksSorted;

        List<TerrainChunk> loadedChunks;
        TerrainChunkIndex currentTerrainChunkIndex;

        void Awake()
        {
            loadedChunks = new List<TerrainChunk>();
            loadedChunksSorted = new Dictionary<TerrainChunkIndex, TerrainChunk>(new TerrainChunkIndexComparer());
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
            TerrainChunkMeshGenerator.Init(surfaceLevelGeneratorShader, marchingCubesGeneratorShader, alterPointsShader, prepareRelevantCubesShader);
            TerrainChunkLoadingManager.Init();
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
            int c = loadedChunks.Count;
            for (int i = 0; i < c; i++)
            {
                TerrainChunk loadedChunk = loadedChunks[i];
                if (!loadedChunk.index.InRange(currentTerrainChunkIndex, renderDistance))
                {
                    loadedChunk.Cache();
                    TerrainChunkLoadingManager.chunksToLoad.Remove(loadedChunk);

                    TerrainChunkIndex initialTerrainChunkIndex = currentTerrainChunkIndex - distance;
                    TerrainChunkIndex initialGridPos = loadedChunk.index - initialTerrainChunkIndex;
                    TerrainChunkIndex axisToInvert = distance.Sign().Abs();
                    TerrainChunkIndex gridPosInverted = initialGridPos.Invert(axisToInvert);
                    gridPosInverted -= axisToInvert;
                    TerrainChunkIndex newIndex = currentTerrainChunkIndex + gridPosInverted;
                    TerrainChunk newChunk = new TerrainChunk(newIndex);

                    loadedChunks[i] = newChunk;
                    LoadChunk(newChunk);
                }
            }
        }

        void LoadChunks(List<TerrainChunkIndex> indicesToLoad)
        {
            foreach (TerrainChunkIndex indexToLoad in indicesToLoad)
            {
                TerrainChunk chunkToAdd = new TerrainChunk(indexToLoad);
                loadedChunks.Add(chunkToAdd);
                LoadChunk(chunkToAdd);
            }
        }

        void LoadChunk(TerrainChunk chunkToLoad)
        {
            TerrainChunkLoadingManager.chunksToLoad.Add(chunkToLoad);
            try
            {
                loadedChunksSorted.Add(chunkToLoad.index, chunkToLoad);
            }
            catch (ArgumentException)
            {
                loadedChunksSorted[chunkToLoad.index] = chunkToLoad;
            }
        }

        void OnDestroy()
        {
            TerrainChunkLoadingManager.ReleaseCached();
        }
    }
}