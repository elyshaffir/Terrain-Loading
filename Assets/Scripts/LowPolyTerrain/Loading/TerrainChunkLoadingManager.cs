using System;
using System.Collections.Generic;

namespace LowPolyTerrain.Chunk
{
    class TerrainChunkLoadingManager
    {
        public static List<TerrainChunk> chunksToLoad;

        static List<TerrainChunk> relevantChunks; // Remember to change GenerateWithPoints() to use this

        public static void Init()
        {
            chunksToLoad = new List<TerrainChunk>();
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
            relevantChunks = new List<TerrainChunk>();
            foreach (TerrainChunk chunkToLoad in currentChunks)
            {
                if (chunkToLoad.PhaseTwo())
                {
                    relevantChunks.Add(chunkToLoad);
                }
            }
            foreach (TerrainChunk relevantChunk in relevantChunks)
            {
                relevantChunk.PhaseThree();
            }
        }

        static int CalculateChunksPerFrame(int renderDistance)
        {
            return (int)Math.Ceiling(TerrainChunk.ChunkSize.magnitude / renderDistance * TerrainChunkIndex.GetRenderDistanceY(renderDistance) * renderDistance);
        }

        public static void PhaseTwo()
        {
            foreach (TerrainChunk relevantChunk in relevantChunks)
            {
                relevantChunk.PhaseFour();
            }
        }
    }
}