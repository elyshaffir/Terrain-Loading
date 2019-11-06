using UnityEngine;

namespace LowPolyTerrain.Chunk
{
    class TerrainChunkConstraint
    {
        public readonly Vector3Int scale; // Scale is by TerrainChunk.ChunkSizeInPoints
        public Vector3 position { get; private set; }

        public TerrainChunkConstraint(Vector3 position)
        {
            this.scale = Vector3Int.one;
            this.position = position;
        }

        public int GetVolume()
        {
            return scale.x * TerrainChunk.ChunkSizeInPoints.x * scale.y * TerrainChunk.ChunkSizeInPoints.y * scale.z * TerrainChunk.ChunkSizeInPoints.z;
        }
    }
}