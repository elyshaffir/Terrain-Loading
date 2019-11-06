using System;
using System.Collections.Generic;

namespace LowPolyTerrain.Chunk
{
    class TerrainChunkLoadingManager
    {
        public static List<TerrainChunk> chunksToLoad;
        public static List<TerrainChunk> chunksWithPoints;

        public static void Init()
        {
            chunksToLoad = new List<TerrainChunk>();
            chunksWithPoints = new List<TerrainChunk>();
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
    }
}