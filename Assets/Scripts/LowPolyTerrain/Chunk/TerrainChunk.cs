using System.Collections.Generic;
using LowPolyTerrain.MeshGeneration;
using UnityEngine;

namespace LowPolyTerrain.Chunk
{
    class TerrainChunk
    {
        public static Vector3Int ChunkSize = new Vector3Int(50, 15, 50);
        public static GameObject prefab;
        public static Transform parent;
        public readonly TerrainChunkIndex index;

        TerrainChunkMeshGenerator meshGenerator;
        GameObject terrainObject;

        public TerrainChunk(TerrainChunkIndex index)
        {
            this.index = index;
            meshGenerator = new TerrainChunkMeshGenerator(index);
            this.terrainObject = MonoBehaviour.Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);
            terrainObject.GetComponent<TerrainChunkBehaviour>().chunk = this;
            terrainObject.GetComponent<MeshFilter>().mesh = meshGenerator.mesh;
            terrainObject.GetComponent<MeshCollider>().sharedMesh = meshGenerator.mesh;
            TerrainChunkAlterationManager.CreateChunk(index);
        }

        public void PhaseOne()
        {
            meshGenerator.surfaceLevelShader.SetBuffers();
            meshGenerator.surfaceLevelShader.Dispatch();
        }

        public bool PhaseTwo()
        {
            meshGenerator.surfaceLevelShader.GetData();
            meshGenerator.surfaceLevelShader.Release();
            return meshGenerator.ApplyAlterations() || meshGenerator.surfaceLevelShader.IsRelevant();
        }

        public void PhaseThree()
        {
            meshGenerator.marchingCubesShader.SetBuffers();
            meshGenerator.marchingCubesShader.Dispatch();
        }

        public void PhaseFour()
        {
            meshGenerator.marchingCubesShader.GetData();
            meshGenerator.marchingCubesShader.Release();
            meshGenerator.CreateMesh();
            terrainObject.GetComponent<MeshCollider>().sharedMesh = meshGenerator.mesh;
        }

        public void Alter(Vector3 spherePosition, float sphereRadius, float power, HashSet<TerrainChunkIndex> additionalIndices)
        {
            TerrainChunkAlterationManager.AddAlterations(index, meshGenerator.Alter(spherePosition, sphereRadius, power, additionalIndices, this));
            terrainObject.GetComponent<MeshCollider>().sharedMesh = meshGenerator.mesh;
        }

        public void Destroy()
        {
            MonoBehaviour.Destroy(terrainObject);
        }
    }
}