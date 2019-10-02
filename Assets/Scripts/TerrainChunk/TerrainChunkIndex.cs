using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunkIndex
{
    private readonly int x;
    private readonly int z;

    public TerrainChunkIndex(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public static List<TerrainChunkIndex> GetChunksToLoad(Vector3 loadingChunkPosition, int renderDistance, List<TerrainChunk> loadedChunks)
    {
        return GetChunksToLoad(FromVector(loadingChunkPosition), renderDistance, loadedChunks);
    }

    public static List<TerrainChunkIndex> GetChunksToLoad(TerrainChunkIndex loadingObjectIndex, int renderDistance, List<TerrainChunk> loadedChunks)
    {
        List<TerrainChunkIndex> chunksToLoad = new List<TerrainChunkIndex>();
        for (int x = loadingObjectIndex.x + renderDistance; x > loadingObjectIndex.x - renderDistance; x--)
        {
            for (int z = loadingObjectIndex.z + renderDistance; z > loadingObjectIndex.z - renderDistance; z--)
            {
                chunksToLoad.Add(new TerrainChunkIndex(x, z));
            }
        }
        List<TerrainChunkIndex> loadedIndices = new List<TerrainChunkIndex>();
        foreach (TerrainChunk loadedChunk in loadedChunks)
        {
            loadedIndices.Add(loadedChunk.index);
        }
        List<TerrainChunkIndex> indicesToRemove = new List<TerrainChunkIndex>();
        foreach (TerrainChunkIndex loadedIndex in loadedIndices)
        {
            foreach (TerrainChunkIndex indexToLoad in chunksToLoad)
            {
                if (indexToLoad.Equals(loadedIndex))
                {
                    indicesToRemove.Add(indexToLoad);
                }
            }
        }
        foreach (TerrainChunkIndex indexToRemove in indicesToRemove)
        {
            chunksToLoad.Remove(indexToRemove);
        }
        return chunksToLoad;
    }

    public bool InRange(TerrainChunkIndex loadingObjectIndex, int renderDistance)
    {
        return Math.Abs(loadingObjectIndex.x - x) <= renderDistance &&
            Math.Abs(loadingObjectIndex.z - z) <= renderDistance;
    }

    public static TerrainChunkIndex FromVector(Vector3 v)
    {
        return new TerrainChunkIndex(Mathf.RoundToInt(v.x / TerrainChunk.ChunkSize), Mathf.RoundToInt(v.z / TerrainChunk.ChunkSize));
    }

    public Vector3 ToPosition()
    {
        return new Vector3(x - 1, 0, z - 1) * (TerrainChunk.ChunkSize - 1);
    }

    public bool Equals(TerrainChunkIndex index)
    {
        return x == index.x && z == index.z;
    }
}