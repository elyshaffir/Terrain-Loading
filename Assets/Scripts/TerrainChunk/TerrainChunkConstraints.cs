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

    public void UpdateScale(Vector3Int scale)
    {
        this.scale = scale;
        scale = new Vector3Int(Mathf.Max(scale.x, 1), Mathf.Max(scale.y, 1), Mathf.Max(scale.z, 1));
    }

    public int GetVolume()
    {
        return scale.x * TerrainChunk.ChunkSize.x * scale.y * TerrainChunk.ChunkSize.y * scale.z * TerrainChunk.ChunkSize.z;
    }

    public int GetTrianglesVolume()
    {
        return GetVolume();
    }
}