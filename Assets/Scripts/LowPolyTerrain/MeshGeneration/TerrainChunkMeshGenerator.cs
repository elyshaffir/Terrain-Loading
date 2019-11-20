using System.Collections.Generic;
using Comparers;
using LowPolyTerrain.Chunk;
using LowPolyTerrain.MeshGeneration.DataStructures;
using LowPolyTerrain.ShaderObjects;
using UnityEngine;

namespace LowPolyTerrain.MeshGeneration
{
    class TerrainChunkMeshGenerator
    {
        public Mesh mesh;
        public readonly TerrainChunkConstraint constraint;
        public Triangle[] triangles;
        public SurfaceLevelGeneratorShader surfaceLevelShader;
        public MarchingCubesShader marchingCubesShader;
        public AlterPointsShader alterPointsShader;

        TerrainChunkIndex index;

        public static void Init(ComputeShader surfaceLevelGeneratorShader, ComputeShader marchingCubesGeneratorShader, ComputeShader alterPointsShader, ComputeShader prepareRelevantCubesShader)
        {
            SurfaceLevelGeneratorShader.surfaceLevelGeneratorShader = surfaceLevelGeneratorShader;
            MarchingCubesShader.marchingCubesGeneratorShader = marchingCubesGeneratorShader;
            AlterPointsShader.alterPointsShader = alterPointsShader;
            PrepareRelevantCubesShader.prepareRelevantCubesShader = prepareRelevantCubesShader;

            SurfaceLevelGeneratorShader.seed = Random.Range(-1000000f, 1000000f);
            SurfaceLevelGeneratorShader.isoLevel = -3.5f;
        }

        public TerrainChunkMeshGenerator(TerrainChunkIndex index)
        {
            this.index = index;
            constraint = new TerrainChunkConstraint(index.ToPosition());

            surfaceLevelShader = new SurfaceLevelGeneratorShader(this);
            marchingCubesShader = new MarchingCubesShader(this);
            alterPointsShader = new AlterPointsShader(this, TerrainChunkLoadingManager.GetCachedChunk(index));

            mesh = new Mesh();
        }

        public void Alter(Vector3 spherePosition, float sphereRadius, float power, HashSet<TerrainChunkIndex> additionalIndices, TerrainChunk chunk)
        {
            alterPointsShader.Execute(spherePosition, sphereRadius, power, index, additionalIndices);
            if (surfaceLevelShader.IsRelevant())
            {
                TerrainChunkLoadingManager.chunksWithPoints.Add(chunk);
            }
        }

        public void CreateMesh()
        {
            mesh.Clear();
            List<Vector3> meshVertices = new List<Vector3>();
            List<int> meshTriangles = new List<int>();
            for (int i = 0; i < triangles.Length; i++)
            {
                Triangle t = triangles[i];
                meshVertices.Add(t.vertexC);
                meshVertices.Add(t.vertexB);
                meshVertices.Add(t.vertexA);
                meshTriangles.Add(i * 3);
                meshTriangles.Add(i * 3 + 1);
                meshTriangles.Add(i * 3 + 2);
            }
            mesh.vertices = meshVertices.ToArray();
            mesh.triangles = meshTriangles.ToArray();
            mesh.RecalculateNormals();
        }
    }
}