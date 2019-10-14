using UnityEngine;

public class TerrainChunkConstraint
{
    public Vector3Int scale { get; private set; } // Scale is by TerrainChunk.ChunkSize
    public Vector3 position { get; private set; }

    public TerrainChunkConstraint(Vector3 position)
    {
        this.scale = Vector3Int.one;
        this.position = position;
    }

    public int GetVolume()
    {
        return scale.x * TerrainChunk.ChunkSize.x * scale.y * TerrainChunk.ChunkSize.y * scale.z * TerrainChunk.ChunkSize.z;
    }
}