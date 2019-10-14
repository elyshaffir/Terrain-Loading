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

    public static List<TerrainChunkIndex> GetChunksToUpdate(Vector3 loadingChunkPosition, int renderDistance)
    {
        TerrainChunkIndex loadingObjectIndex = FromVector(loadingChunkPosition);
        List<TerrainChunkIndex> chunksToUpdate = new List<TerrainChunkIndex>();
        for (int x = loadingObjectIndex.x + renderDistance - 1; x > loadingObjectIndex.x - renderDistance - 1; x--)
        {
            for (int z = loadingObjectIndex.z + renderDistance - 1; z > loadingObjectIndex.z - renderDistance - 1; z--)
            {
                chunksToUpdate.Add(new TerrainChunkIndex(x, z));
            }
        }
        return chunksToUpdate;
    }

    public static List<TerrainChunkIndex> GetChunksToLoad(Vector3 loadingChunkPosition,
        int renderDistance,
        List<TerrainChunkIndex> chunksToLoad,
        List<TerrainChunk> loadedChunks)
    {
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

    public void GetAdjacentToManipulate(Vector2 onEdges, HashSet<TerrainChunkIndex> additionalIndices)
    {
        if (onEdges.x != 0)
        {
            additionalIndices.Add(new TerrainChunkIndex(x + Math.Sign(onEdges.x), z));
        }

        if (onEdges.y != 0)
        {
            additionalIndices.Add(new TerrainChunkIndex(x, z + Math.Sign(onEdges.y)));
            if (onEdges.x != 0)
            {
                additionalIndices.Add(new TerrainChunkIndex(x + Math.Sign(onEdges.x), z + Math.Sign(onEdges.y)));
            }
        }
    }

    public bool InRange(TerrainChunkIndex loadingObjectIndex, int renderDistance)
    {
        return Math.Abs(loadingObjectIndex.x - x) <= renderDistance &&
            Math.Abs(loadingObjectIndex.z - z) <= renderDistance;
    }

    public static TerrainChunkIndex FromVector(Vector3 v)
    {
        return new TerrainChunkIndex(Mathf.FloorToInt(v.x / TerrainChunk.ChunkSize.x), Mathf.FloorToInt(v.z / TerrainChunk.ChunkSize.z));
    }

    public Vector3 ToPosition()
    {
        return new Vector3(x * TerrainChunk.ChunkSize.x, 0, z * TerrainChunk.ChunkSize.z);
    }

    public bool Equals(TerrainChunkIndex index)
    {
        return x == index.x && z == index.z;
    }

    public bool IsAdjacent(TerrainChunkIndex index, int distance) // This includes the case where both are equal
    {
        return Mathf.Abs(x - index.x) + Mathf.Abs(z - index.z) <= distance;
    }

    public static int GetTerrainY(Vector3 position)
    {
        return Mathf.RoundToInt(position.y / TerrainChunk.ChunkSize.y);
    }

    public class TerrainChunkIndexComparer : IEqualityComparer<TerrainChunkIndex>
    {
        public bool Equals(TerrainChunkIndex x, TerrainChunkIndex y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(TerrainChunkIndex obj)
        {
            if (obj == null) return 0;
            return obj.x.GetHashCode() ^ obj.z.GetHashCode();
        }
    }
}