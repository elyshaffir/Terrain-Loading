using System.Collections.Generic;
using LowPolyTerrain.MeshGeneration;
using UnityEngine;

namespace LowPolyTerrain.Chunk
{
    class TerrainChunk
    {
        public static Vector3Int ChunkSizeInCubes = new Vector3Int(14, 14, 14);
        public static Vector3Int ChunkSizeInPoints = TerrainChunk.ChunkSizeInCubes + Vector3Int.one;

        public static GameObject prefab;
        public static Transform parent;
        public readonly TerrainChunkIndex index;

        TerrainChunkMeshGenerator meshGenerator;
        GameObject terrainObject;

        bool cached;

        public TerrainChunk(TerrainChunkIndex index)
        {
            this.index = index;
            meshGenerator = new TerrainChunkMeshGenerator(index);
            this.terrainObject = MonoBehaviour.Instantiate(prefab, index.ToPosition(), Quaternion.identity, parent);
            terrainObject.GetComponent<TerrainChunkBehaviour>().chunk = this;
            terrainObject.GetComponent<MeshFilter>().mesh = meshGenerator.mesh;
            terrainObject.GetComponent<MeshCollider>().sharedMesh = meshGenerator.mesh;
            this.cached = false;
        }

        public void PhaseOne()
        {
            meshGenerator.surfaceLevelShader.SetBuffers();
            meshGenerator.surfaceLevelShader.Dispatch();
        }

        public bool PhaseTwo()
        {
            meshGenerator.surfaceLevelShader.GetData();
            return meshGenerator.surfaceLevelShader.IsRelevant();
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
            terrainObject.GetComponent<MeshCollider>().sharedMesh = meshGenerator.mesh;
        }

        public void Alter(Vector3 spherePosition, float sphereRadius, float power, HashSet<TerrainChunkIndex> additionalIndices)
        {
            meshGenerator.Alter(spherePosition, sphereRadius, power, additionalIndices, this);
            terrainObject.GetComponent<MeshCollider>().sharedMesh = meshGenerator.mesh;
        }

        public void Destroy()
        {
            if (!cached)
            {
                meshGenerator.surfaceLevelShader.Release();
                meshGenerator.alterPointsShader.alterationsBuffer.Release();
                MonoBehaviour.Destroy(terrainObject);
            }
        }

        public void Cache()
        {
            cached = true;
            TerrainChunkLoadingManager.CacheChunk(index, meshGenerator.alterPointsShader.alterationsBuffer);
            meshGenerator.surfaceLevelShader.Release();
            MonoBehaviour.Destroy(terrainObject);
        }
    }
}