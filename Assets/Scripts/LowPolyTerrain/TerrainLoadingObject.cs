using System;
using System.Collections.Generic;
using System.Linq;
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
        public int renderDistanceY;
        public int chunksPerFrame;

        public Dictionary<TerrainChunkIndex, TerrainChunk> loadedChunks;

        TerrainChunkIndex currentTerrainChunkIndex;

        void Awake()
        {
            loadedChunks = new Dictionary<TerrainChunkIndex, TerrainChunk>(new TerrainChunkIndexComparer());
        }

        void Start()
        {
            InitializeTerrain();
            TerrainChunkIndex.LoadInitialChunks(loadingObject.transform.position, renderDistance, renderDistanceY, this);
            currentTerrainChunkIndex = TerrainChunkIndex.FromVector(loadingObject.transform.position);
        }

        void InitializeTerrain()
        {
            TerrainChunk.prefab = terrainChunkPrefab;
            TerrainChunk.parent = transform;
            TerrainChunkMeshGenerator.Init(surfaceLevelGeneratorShader, marchingCubesGeneratorShader, alterPointsShader, prepareRelevantCubesShader);
            TerrainChunkLoadingManager.Init();
        }

        void Update()
        {
            TerrainChunkLoadingManager.PhaseOne(chunksPerFrame);
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
            TerrainChunkIndex[] loadedChunkIndices = loadedChunks.Keys.ToArray();
            foreach (TerrainChunkIndex loadedChunkIndex in loadedChunkIndices)
            {
                TerrainChunk loadedChunk = loadedChunks[loadedChunkIndex];
                if (!loadedChunk.index.InRange(currentTerrainChunkIndex, renderDistance, renderDistanceY))
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

                    loadedChunks.Remove(loadedChunkIndex);
                    LoadChunk(newChunk);
                }
            }
        }

        public void LoadChunk(TerrainChunkIndex indexToLoad)
        {
            TerrainChunk chunkToAdd = new TerrainChunk(indexToLoad);
            LoadChunk(chunkToAdd);
        }

        void LoadChunk(TerrainChunk chunkToLoad)
        {
            TerrainChunkLoadingManager.chunksToLoad.Add(chunkToLoad);
            try
            {
                loadedChunks.Add(chunkToLoad.index, chunkToLoad);
            }
            catch (ArgumentException)
            {
                loadedChunks[chunkToLoad.index] = chunkToLoad;
            }
        }

        void OnDestroy()
        {
            TerrainChunkLoadingManager.ReleaseCached();
        }
    }
}