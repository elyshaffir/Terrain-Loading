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
        public SurfaceLevelShader surfaceLevelShader;
        public MarchingCubesShader marchingCubesShader;
        public GetPointsToAlterShader getPointsToAlterShader;

        TerrainChunkIndex index;

        public static void Init(ComputeShader surfaceLevelGeneratorShader, ComputeShader marchingCubesGeneratorShader, ComputeShader getPointsToAlterShader, ComputeShader prepareRelevantCubesShader)
        {
            SurfaceLevelShader.surfaceLevelGeneratorShader = surfaceLevelGeneratorShader;
            MarchingCubesShader.marchingCubesGeneratorShader = marchingCubesGeneratorShader;
            GetPointsToAlterShader.getPointsToAlterShader = getPointsToAlterShader;
            PrepareRelevantCubesShader.prepareRelevantCubesShader = prepareRelevantCubesShader;

            SurfaceLevelShader.seed = 12;//Random.Range(-1000000f, 1000000f);
            SurfaceLevelShader.isoLevel = -3.5f;
        }

        public TerrainChunkMeshGenerator(TerrainChunkIndex index)
        {
            this.index = index;
            constraint = new TerrainChunkConstraint(index.ToPosition());

            surfaceLevelShader = new SurfaceLevelShader(this);
            marchingCubesShader = new MarchingCubesShader(this);
            getPointsToAlterShader = new GetPointsToAlterShader(this, TerrainChunkLoadingManager.GetCachedChunk(index));

            mesh = new Mesh();
        }

        public Dictionary<Vector3, float> Alter(Vector3 spherePosition, float sphereRadius, float power, HashSet<TerrainChunkIndex> additionalIndices, TerrainChunk chunk)
        {
            Dictionary<Vector3, float> alterations = new Dictionary<Vector3, float>(new Vector3Comparer());
            getPointsToAlterShader.Execute(sphereRadius, spherePosition, index, additionalIndices);
            // The alterations need to be brought to the CPU any way because the saving of alterations on the machine (to a file) cannot be done from the CPU
            // foreach (int indexToAlter in pointsToAlter)
            // {
            //     // points[indexToAlter].surfaceLevel += power;                
            //     alterations[points[indexToAlter].position + constraint.position] = points[indexToAlter].surfaceLevel;
            //     // As last resort, there can be an array of onEdges for each generator for GetAdjacentToManipulate
            //     // GetAdjacentToManipulate can also be calculated in the shader given the current chunk and in a simmilar way to the way relevantCubes is calculated (array[i] = 1)
            //     index.GetAdjacentToManipulate(points[indexToAlter].onEdges, additionalIndices); // DONE!
            // }            
            if (surfaceLevelShader.IsRelevant())
            {
                TerrainChunkLoadingManager.chunksWithPoints.Add(chunk);
            }
            return alterations;
        }

        public bool ApplyAlterations()
        {
            Dictionary<Vector3, float> alterations = TerrainChunkAlterationManager.alterations[index];
            if (alterations.Count == 0)
            {
                return false;
            }
            // Dictionary<Vector3, int> pointIndices = new Dictionary<Vector3, int>();
            // for (int i = 0; i < points.Length; i++)
            // {
            //     pointIndices.Add(points[i].position, i);
            // }
            // foreach (KeyValuePair<Vector3, float> alteration in alterations)
            // {
            //     points[pointIndices[alteration.Key]].surfaceLevel = alteration.Value;
            // }
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