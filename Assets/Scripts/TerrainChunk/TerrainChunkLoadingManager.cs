using System.Collections.Generic;

public class TerrainChunkLoadingManager
{
    public static List<TerrainChunk> chunksToLoad;

    public static void Init()
    {
        chunksToLoad = new List<TerrainChunk>();
    }

    public static void Update()
    {
        foreach (TerrainChunk chunkToLoad in chunksToLoad)
        {
            chunkToLoad.PhaseOne();
        }
        List<TerrainChunk> relevantChunks = new List<TerrainChunk>();
        foreach (TerrainChunk chunkToLoad in chunksToLoad)
        {
            if (chunkToLoad.PhaseTwo())
            {
                relevantChunks.Add(chunkToLoad);
            }
        }
        chunksToLoad.Clear();
        foreach (TerrainChunk relevantChunk in relevantChunks)
        {
            relevantChunk.PhaseThree();
        }
        foreach (TerrainChunk relevantChunk in relevantChunks)
        {
            relevantChunk.PhaseFour();
        }
    }
}