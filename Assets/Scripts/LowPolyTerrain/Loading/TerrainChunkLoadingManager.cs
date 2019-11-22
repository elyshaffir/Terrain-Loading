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

        public static void PhaseOne(int chunksPerFrame)
        {
            for (int i = 0, c = 0; i < chunksToLoad.Count && c < chunksPerFrame; i++, c++)
            {
                TerrainChunk chunkToLoad = chunksToLoad[i];
                chunkToLoad.PhaseOne();
                chunksToLoad.RemoveAt(i);

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