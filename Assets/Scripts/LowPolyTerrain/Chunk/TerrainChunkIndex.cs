using System;
using System.Collections.Generic;
using UnityEngine;

namespace LowPolyTerrain.Chunk
{
    class TerrainChunkIndex
    {

        public static readonly TerrainChunkIndex zero = new TerrainChunkIndex(0, 0, 0);

        readonly int x;
        readonly int y;
        readonly int z;

        TerrainChunkIndex(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static List<TerrainChunkIndex> GetChunksToUpdate(Vector3 loadingChunkPosition, int renderDistance)
        {
            int renderDistanceY = GetRenderDistanceY(renderDistance);
            TerrainChunkIndex loadingObjectIndex = FromVector(loadingChunkPosition);
            List<TerrainChunkIndex> chunksToUpdate = new List<TerrainChunkIndex>();
            for (int x = loadingObjectIndex.x + renderDistance - 1; x > loadingObjectIndex.x - renderDistance - 1; x--)
            {
                for (int y = loadingObjectIndex.y + renderDistanceY - 1; y > loadingObjectIndex.y - renderDistanceY - 1; y--)
                {
                    for (int z = loadingObjectIndex.z + renderDistance - 1; z > loadingObjectIndex.z - renderDistance - 1; z--)
                    {
                        chunksToUpdate.Add(new TerrainChunkIndex(x, y, z));
                    }
                }
            }
            return chunksToUpdate;
        }

        public static int GetRenderDistanceY(int renderDistance)
        {
            return renderDistance / 2;
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
                Mathf.FloorToInt(v.x / TerrainChunk.ChunkSizeInCubes.x),
                Mathf.FloorToInt(v.y / TerrainChunk.ChunkSizeInCubes.y),
                Mathf.FloorToInt(v.z / TerrainChunk.ChunkSizeInCubes.z)
            );
        }

        public Vector3 ToPosition()
        {
            return new Vector3(
                x * TerrainChunk.ChunkSizeInCubes.x,
                y * TerrainChunk.ChunkSizeInCubes.y,
                z * TerrainChunk.ChunkSizeInCubes.z
            );
        }

        public bool InRange(TerrainChunkIndex loadingObjectIndex, int renderDistance)
        {
            int renderDistanceY = GetRenderDistanceY(renderDistance);
            return x <= loadingObjectIndex.x + renderDistance - 1 &&
                x > loadingObjectIndex.x - renderDistance - 1 &&
                y <= loadingObjectIndex.y + renderDistanceY - 1 &&
                y > loadingObjectIndex.y - renderDistanceY - 1 &&
                z <= loadingObjectIndex.z + renderDistance - 1 &&
                z > loadingObjectIndex.z - renderDistance - 1;
        }

        public TerrainChunkIndex Abs()
        {
            return new TerrainChunkIndex(Math.Abs(x), Math.Abs(y), Math.Abs(z));
        }

        public TerrainChunkIndex Sign()
        {
            return new TerrainChunkIndex(Math.Sign(x), Math.Sign(y), Math.Sign(z));
        }

        public static TerrainChunkIndex operator +(TerrainChunkIndex index1, TerrainChunkIndex index2)
        {
            return new TerrainChunkIndex(index1.x + index2.x, index1.y + index2.y, index1.z + index2.z);
        }

        public static TerrainChunkIndex operator -(TerrainChunkIndex index1, TerrainChunkIndex index2)
        {
            return new TerrainChunkIndex(index1.x - index2.x, index1.y - index2.y, index1.z - index2.z);
        }

        public static TerrainChunkIndex operator *(TerrainChunkIndex index1, TerrainChunkIndex index2)
        {
            return new TerrainChunkIndex(index1.x * index2.x, index1.y * index2.y, index1.z * index2.z);
        }

        public TerrainChunkIndex Invert(TerrainChunkIndex axisToInvert)
        {
            return this * new TerrainChunkIndex(
                                (axisToInvert.x == 0) ? 1 : -1,
                                (axisToInvert.y == 0) ? 1 : -1,
                                (axisToInvert.z == 0) ? 1 : -1
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
}