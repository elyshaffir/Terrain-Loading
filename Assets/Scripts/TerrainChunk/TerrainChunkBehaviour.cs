using UnityEngine;

public class TerrainChunkBehaviour : MonoBehaviour
{
    public TerrainChunk chunk;

    public void Alter(Vector3 spherePosition, float sphereRadius, float power)
    {
        chunk.Alter(spherePosition, sphereRadius, power);
    }
}
