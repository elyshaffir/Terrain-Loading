using UnityEngine;

public class TerrainChunk
{
    public static Vector3Int ChunkSize = new Vector3Int(10, 10, 10);
    public static GameObject prefab;

    public readonly TerrainChunkIndex index;

    private GameObject terrainObject;
    private TerrainChunkMeshGenerator meshGenerator;

    public TerrainChunk(TerrainChunkIndex index)
    {
        this.index = index;
        meshGenerator = new TerrainChunkMeshGenerator(index);
        this.terrainObject = MonoBehaviour.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        terrainObject.GetComponent<MeshFilter>().mesh = meshGenerator.mesh;
    }

    public void Create(Vector3Int scale, float newConstraintY)
    {
        meshGenerator.Update(scale, newConstraintY);
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