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
        public Point[] points;
        public SurfaceLevelShader surfaceLevelShader;
        public MarchingCubesShader marchingCubesShader;

        TerrainChunkIndex index;

        public static void Init(ComputeShader surfaceLevelGeneratorShader, ComputeShader marchingCubesGeneratorShader)
        {
            SurfaceLevelShader.surfaceLevelGeneratorShader = surfaceLevelGeneratorShader;
            SurfaceLevelShader.seed = Random.Range(-1000000f, 1000000f);
            MarchingCubesShader.marchingCubesGeneratorShader = marchingCubesGeneratorShader;
        }

        public TerrainChunkMeshGenerator(TerrainChunkIndex index)
        {
            this.index = index;
            constraint = new TerrainChunkConstraint(index.ToPosition());

            surfaceLevelShader = new SurfaceLevelShader(this);
            marchingCubesShader = new MarchingCubesShader(this);

            mesh = new Mesh();
        }

        public Dictionary<Vector3, float> Alter(Vector3 spherePosition, float sphereRadius, float power, HashSet<TerrainChunkIndex> additionalIndices, TerrainChunk chunk)
        {
            Dictionary<Vector3, float> alterations = new Dictionary<Vector3, float>(new Vector3Comparer());
            for (int i = 0; i < points.Length; i++)
            {
                if (Vector3.Distance(points[i].position, spherePosition) <= sphereRadius)
                {
                    points[i].surfaceLevel += power;
                    alterations[points[i].position] = points[i].surfaceLevel;

                    index.GetAdjacentToManipulate(points[i].onEdges, additionalIndices);
                }
            }
            TerrainChunkLoadingManager.chunksWithPoints.Add(chunk);
            return alterations;
        }

        public bool ApplyAlterations()
        {
            Dictionary<Vector3, float> alterations = TerrainChunkAlterationManager.alterations[index];
            if (alterations.Count == 0)
            {
                return false;
            }
            Dictionary<Vector3, int> pointIndices = new Dictionary<Vector3, int>();
            for (int i = 0; i < points.Length; i++)
            {
                pointIndices.Add(points[i].position, i);
            }
            foreach (KeyValuePair<Vector3, float> alteration in alterations)
            {
                points[pointIndices[alteration.Key]].surfaceLevel = alteration.Value;
            }
            return true;

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