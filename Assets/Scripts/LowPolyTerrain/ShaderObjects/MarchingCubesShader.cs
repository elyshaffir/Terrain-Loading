using ComputeShading;
using LowPolyTerrain.Chunk;
using LowPolyTerrain.MeshGeneration;
using LowPolyTerrain.MeshGeneration.DataStructures;
using Unity.Mathematics;
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

        uint3[] cubesToMarch;

        public MarchingCubesShader(TerrainChunkMeshGenerator generator) :
            base(marchingCubesGeneratorShader,
                marchingCubesGeneratorShader.FindKernel("MarchCubes"))
        {
            this.generator = generator;
        }

        public void SetCubesToMarch(uint3[] cubesToMarch)
        {
            this.cubesToMarch = cubesToMarch;
        }

        protected override ComputeShaderProperty[] GetProperties()
        {
            return new ComputeShaderProperty[] {
                new ComputeShaderIntProperty("numPointsX", generator.constraint.scale.x * TerrainChunk.ChunkSizeInPoints.x),
                new ComputeShaderIntProperty("numPointsY", generator.constraint.scale.y * TerrainChunk.ChunkSizeInPoints.y),
                new ComputeShaderFloatProperty("isoLevel", TerrainChunkMeshGenerator.IsoLevel)
            };
        }

        public override void SetBuffers()
        {
            ComputeBuffer inputPoints = new ComputeBuffer(generator.points.Length, Point.StructSize);
            inputPoints.SetData(generator.points);

            ComputeBuffer cubesToMarchBuffer = new ComputeBuffer(TerrainChunk.ChunkSizeInPoints.x * TerrainChunk.ChunkSizeInPoints.y * TerrainChunk.ChunkSizeInPoints.z, sizeof(uint) * 3);
            cubesToMarchBuffer.SetData(cubesToMarch);

            triangleBuffer = new ComputeBuffer(
                generator.constraint.GetVolume(), Triangle.StructSize, ComputeBufferType.Append);
            triangleBuffer.SetCounterValue(0);
            triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);

            SetBuffer("triangles", triangleBuffer);
            SetBuffer("points", inputPoints);
            SetBuffer("cubesToMarch", cubesToMarchBuffer);
            AddBuffer(triangleCountBuffer);
        }

        public override void Dispatch()
        {
            Dispatch(
                cubesToMarch.Length,
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