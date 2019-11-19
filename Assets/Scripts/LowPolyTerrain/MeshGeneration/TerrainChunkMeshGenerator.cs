using System.Collections.Generic;
using Comparers;
using LowPolyTerrain.Chunk;
using LowPolyTerrain.ShaderObjects;
using UnityEngine;

namespace LowPolyTerrain.MeshGeneration
{
    class TerrainChunkMeshGenerator
    {
        public Mesh mesh;
        public readonly TerrainChunkConstraint constraint;
        public SurfaceLevelShader surfaceLevelShader;
        public MarchingCubesShader marchingCubesShader;
        public AlterPointsShader alterPointsShader;

        TerrainChunkIndex index;

        public static void Init(ComputeShader surfaceLevelGeneratorShader, ComputeShader marchingCubesGeneratorShader, ComputeShader alterPointsShader, ComputeShader prepareRelevantCubesShader)
        {
            SurfaceLevelShader.surfaceLevelGeneratorShader = surfaceLevelGeneratorShader;
            MarchingCubesShader.marchingCubesGeneratorShader = marchingCubesGeneratorShader;
            AlterPointsShader.alterPointsShader = alterPointsShader;
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
            alterPointsShader = new AlterPointsShader(this, TerrainChunkLoadingManager.GetCachedChunk(index));

            mesh = new Mesh();
        }

        public Dictionary<Vector3, float> Alter(Vector3 spherePosition, float sphereRadius, float power, HashSet<TerrainChunkIndex> additionalIndices, TerrainChunk chunk)
        {
            Dictionary<Vector3, float> alterations = new Dictionary<Vector3, float>(new Vector3Comparer());
            alterPointsShader.Execute(spherePosition, sphereRadius, power, index, additionalIndices);
            if (surfaceLevelShader.IsRelevant())
            {
                TerrainChunkLoadingManager.chunksWithPoints.Add(chunk);
            }
            return alterations;
        }
    }
}