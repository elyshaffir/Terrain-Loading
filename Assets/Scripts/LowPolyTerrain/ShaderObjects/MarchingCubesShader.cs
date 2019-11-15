using ComputeShading;
using LowPolyTerrain.Chunk;
using LowPolyTerrain.MeshGeneration;
using LowPolyTerrain.MeshGeneration.DataStructures;
using UnityEngine;
using static ComputeShading.ComputeShaderProperty;

namespace LowPolyTerrain.ShaderObjects
{
    class MarchingCubesShader : ComputeShaderObject
    {
        public static ComputeShader marchingCubesGeneratorShader;

        readonly TerrainChunkMeshGenerator generator;

        ComputeBuffer triangleBuffer;
        ComputeBuffer triangleCountBuffer;
        ComputeBuffer cubesToMarchBuffer;

        uint cubesToMarchCount;

        public MarchingCubesShader(TerrainChunkMeshGenerator generator) :
            base(marchingCubesGeneratorShader,
                marchingCubesGeneratorShader.FindKernel("MarchCubes"))
        {
            this.generator = generator;
        }

        public void SetCubesToMarch(ComputeBuffer cubesToMarchBuffer, uint cubesToMarchCount)
        {
            this.cubesToMarchBuffer = cubesToMarchBuffer;
            this.cubesToMarchCount = cubesToMarchCount;
        }

        protected override ComputeShaderProperty[] GetProperties()
        {
            return new ComputeShaderProperty[] {
                new ComputeShaderIntProperty("numPointsX", generator.constraint.scale.x * TerrainChunk.ChunkSizeInPoints.x),
                new ComputeShaderIntProperty("numPointsY", generator.constraint.scale.y * TerrainChunk.ChunkSizeInPoints.y),
                new ComputeShaderFloatProperty("isoLevel", SurfaceLevelShader.isoLevel)
            };
        }

        public override void SetBuffers()
        {
            triangleBuffer = new ComputeBuffer(
                generator.constraint.GetVolume(), Triangle.StructSize, ComputeBufferType.Append);
            triangleBuffer.SetCounterValue(0);
            triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);

            SetBuffer("triangles", triangleBuffer);
            SetBuffer("points", generator.surfaceLevelShader.pointsBuffer, false);
            SetBuffer("cubesToMarch", cubesToMarchBuffer);
            AddBuffer(triangleCountBuffer);
        }

        public override void Dispatch()
        {
            Dispatch(
                Mathf.CeilToInt((cubesToMarchCount) / 5f),
                1,
                1,
                GetProperties());
        }

        public override void GetData()
        {
            ComputeBuffer.CopyCount(triangleBuffer, triangleCountBuffer, 0);
            int[] triangleCount = new int[1] { 0 };
            triangleCountBuffer.GetData(triangleCount);
            generator.triangles = new Triangle[triangleCount[0]];
            triangleBuffer.GetData(generator.triangles);
        }
    }
}