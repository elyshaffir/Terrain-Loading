using System;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunkIndex
{
    private readonly int x;
    private readonly int y;
    private readonly int z;

    public TerrainChunkIndex(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public static List<TerrainChunkIndex> GetChunksToUpdate(Vector3 loadingChunkPosition, int renderDistance)
    {
        TerrainChunkIndex loadingObjectIndex = FromVector(loadingChunkPosition);
        List<TerrainChunkIndex> chunksToUpdate = new List<TerrainChunkIndex>();
        for (int x = loadingObjectIndex.x + renderDistance - 1; x > loadingObjectIndex.x - renderDistance - 1; x--)
        {
            for (int y = loadingObjectIndex.y + renderDistance - 1; y > loadingObjectIndex.y - renderDistance - 1; y--)
            {
                for (int z = loadingObjectIndex.z + renderDistance - 1; z > loadingObjectIndex.z - renderDistance - 1; z--)
                {
                    chunksToUpdate.Add(new TerrainChunkIndex(x, y, z));
                }
            }
        }
        return chunksToUpdate;
    }

    public static List<TerrainChunkIndex> GetChunksToLoad(Vector3 loadingChunkPosition,
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

    public void GetAdjacentToManipulate(Vector3 onEdges, HashSet<TerrainChunkIndex> additionalIndices)
    {
        if (onEdges.x != 0)
        {
            additionalIndices.Add(new TerrainChunkIndex(x + Math.Sign(onEdges.x), y, z));
        }

        if (onEdges.y != 0)
        {
            additionalIndices.Add(new TerrainChunkIndex(x, y + Math.Sign(onEdges.y), z));
            if (onEdges.x != 0)
            {
                additionalIndices.Add(new TerrainChunkIndex(x + Math.Sign(onEdges.x), y + Math.Sign(onEdges.y), z));
            }
        }

        if (onEdges.z != 0)
        {
            additionalIndices.Add(new TerrainChunkIndex(x, y, z + Math.Sign(onEdges.z)));
            if (onEdges.y != 0)
            {
                additionalIndices.Add(new TerrainChunkIndex(x, y + Math.Sign(onEdges.y), z + Math.Sign(onEdges.z)));
                if (onEdges.x != 0)
                {
                    additionalIndices.Add(new TerrainChunkIndex(x + Math.Sign(onEdges.x), y + Math.Sign(onEdges.y), z + Math.Sign(onEdges.z)));
                }
            }
        }
    }

    public static TerrainChunkIndex FromVector(Vector3 v)
    {
        return new TerrainChunkIndex(
            Mathf.FloorToInt(v.x / TerrainChunk.ChunkSize.x),
            Mathf.FloorToInt(v.y / TerrainChunk.ChunkSize.y),
            Mathf.FloorToInt(v.z / TerrainChunk.ChunkSize.z)
        );
    }

    public Vector3 ToPosition()
    {
        return new Vector3(
            x * TerrainChunk.ChunkSize.x,
            y * TerrainChunk.ChunkSize.y,
            z * TerrainChunk.ChunkSize.z
        );
    }

    public bool Equals(TerrainChunkIndex index)
    {
        if (index == null)
        {
            return false;
        }
        return x == index.x && y == index.y && z == index.z;
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
            return obj.x.GetHashCode() ^ obj.y.GetHashCode() << 2 ^ obj.z.GetHashCode() >> 2;
        }
    }
}