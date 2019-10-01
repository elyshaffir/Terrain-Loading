using UnityEngine;

class TerrainChunkConstraint
{
    public Vector3Int scale { get; private set; } // Scale is by TerrainChunk.ChunkSize
    private readonly Vector3 position;

    public TerrainChunkConstraint(Vector3 position)
    {
        this.scale = Vector3Int.one;
        this.position = position;
    }

    public void Scale(Vector3Int scaleFactor)
    {
        scale += scaleFactor;
        scale = new Vector3Int(Mathf.Max(scale.x, 1), Mathf.Max(scale.y, 1), Mathf.Max(scale.z, 1));
    }

    public int GetVolume()
    {
        return scale.x * TerrainChunk.ChunkSize * scale.y * TerrainChunk.ChunkSize * scale.z * TerrainChunk.ChunkSize;
    }
}