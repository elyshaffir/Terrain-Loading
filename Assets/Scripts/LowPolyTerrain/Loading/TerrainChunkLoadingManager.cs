using System;
using System.Collections.Generic;
using UnityEngine;
using static LowPolyTerrain.Chunk.TerrainChunkIndex;

namespace LowPolyTerrain.Chunk
{
    class TerrainChunkLoadingManager
    {
        public static List<TerrainChunk> chunksToLoad;
        public static List<TerrainChunk> chunksWithPoints;

        static Dictionary<TerrainChunkIndex, ComputeBuffer> cachedChunks;

        public static void Init()
        {
            chunksToLoad = new List<TerrainChunk>();
            chunksWithPoints = new List<TerrainChunk>();
            cachedChunks = new Dictionary<TerrainChunkIndex, ComputeBuffer>(new TerrainChunkIndexComparer());
        }

        public static void PhaseOne(int renderDistance)
        {
            List<TerrainChunk> currentChunks = new List<TerrainChunk>();
            int chunksPerFrame = CalculateChunksPerFrame(renderDistance);
            for (int i = 0, c = 0; i < chunksToLoad.Count && c < chunksPerFrame; i++, c++)
            {
                TerrainChunk chunkToLoad = chunksToLoad[i];
                chunkToLoad.PhaseOne();
                currentChunks.Add(chunkToLoad);
                chunksToLoad.RemoveAt(i);
            }
            foreach (TerrainChunk chunkToLoad in currentChunks)
            {
                if (chunkToLoad.PhaseTwo())
                {
                    chunksWithPoints.Add(chunkToLoad);
                }
            }
            foreach (TerrainChunk relevantChunk in chunksWithPoints)
            {
                relevantChunk.PhaseThree();
            }
        }

        static int CalculateChunksPerFrame(int renderDistance)
        {
            return (int)Math.Ceiling(TerrainChunk.ChunkSizeInCubes.magnitude / renderDistance * TerrainChunkIndex.GetRenderDistanceY(renderDistance) * renderDistance);
        }

        public static void PhaseTwo()
        {
            foreach (TerrainChunk relevantChunk in chunksWithPoints)
            {
                relevantChunk.PhaseFour();
            }
            chunksWithPoints.Clear();
        }

        public static void CacheChunk(TerrainChunkIndex index, ComputeBuffer alterationsBuffer)
        {
            try
            {
                cachedChunks.Add(index, alterationsBuffer);
                // Since this means the chunk was already cached,
                // the reference to the points buffer is also in the dictionary already                
            }
            catch (ArgumentException) { }
        }

        public static ComputeBuffer GetCachedChunk(TerrainChunkIndex index)
        {
            try
            {
                return cachedChunks[index];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public static void ReleaseCached()
        {
            foreach (ComputeBuffer alterationsBuffer in cachedChunks.Values)
            {
                alterationsBuffer.Release();
            }
        }
    }
}