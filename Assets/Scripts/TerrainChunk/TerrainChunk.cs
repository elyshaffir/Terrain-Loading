using UnityEngine;

public class TerrainChunk
{
    public static Vector3Int ChunkSize = new Vector3Int(10, 10, 10);
    public static GameObject prefab;
    public static Transform parent;

    public readonly TerrainChunkIndex index;

    private GameObject terrainObject;
    private TerrainChunkMeshGenerator meshGenerator;

    public TerrainChunk(TerrainChunkIndex index)
    {
        this.index = index;
        meshGenerator = new TerrainChunkMeshGenerator(index);
        this.terrainObject = MonoBehaviour.Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);
        terrainObject.GetComponent<TerrainChunkBehaviour>().chunk = this;
        terrainObject.GetComponent<MeshFilter>().mesh = meshGenerator.mesh;
        terrainObject.GetComponent<MeshCollider>().sharedMesh = meshGenerator.mesh;
    }

    public void Create(Vector3Int scale, float newConstraintY)
    {
        meshGenerator.Update(scale, newConstraintY);
        terrainObject.GetComponent<MeshCollider>().sharedMesh = meshGenerator.mesh;
    }

    public void Alter(Vector3 spherePosition, float sphereRadius, float power)
    {
        meshGenerator.Alter(spherePosition, sphereRadius, power);
        terrainObject.GetComponent<MeshCollider>().sharedMesh = meshGenerator.mesh;
    }

    public void Destroy()
    {
        MonoBehaviour.Destroy(terrainObject);
    }

    public Vector3 GetScale()
    {
        return meshGenerator.constraint.scale * ChunkSize;
    }
}