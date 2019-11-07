using ComputeShading;
using LowPolyTerrain.Chunk;
using LowPolyTerrain.MeshGeneration;
using Unity.Mathematics;
using UnityEngine;
using static ComputeShading.ComputeShaderProperty;

namespace LowPolyTerrain.ShaderObjects
{
    class PrepareRelevantCubesShader : ComputeShaderObject
    {
        public static ComputeShader prepareRelevantCubesShader;

        readonly TerrainChunkMeshGenerator generator;

        ComputeBuffer relevantCubeCornersBuffer;
        ComputeBuffer cubesToMarchBuffer;
        ComputeBuffer cubesToMarchCountBuffer;

        uint[] relevantCubeCorners;

        public PrepareRelevantCubesShader(TerrainChunkMeshGenerator generator) :
            base(prepareRelevantCubesShader,
                prepareRelevantCubesShader.FindKernel("PrepareRelevantCubes"))
        {
            this.generator = generator;
        }

        void SetRelevantCubeCorners(uint[] relevantCubeCorners)
        {
            this.relevantCubeCorners = relevantCubeCorners;
        }

        protected override ComputeShaderProperty[] GetProperties()
        {
            return new ComputeShaderProperty[] {
                new ComputeShaderIntProperty("numPointsX", generator.constraint.scale.x * TerrainChunk.ChunkSizeInPoints.x),
                new ComputeShaderIntProperty("numPointsY", generator.constraint.scale.y * TerrainChunk.ChunkSizeInPoints.y)
            };
        }

        public override void SetBuffers()
        {
            relevantCubeCornersBuffer = new ComputeBuffer(generator.points.Length, sizeof(uint));
            relevantCubeCornersBuffer.SetData(relevantCubeCorners);
            cubesToMarchBuffer = new ComputeBuffer(generator.points.Length, sizeof(uint) * 3, ComputeBufferType.Append);
            cubesToMarchBuffer.SetCounterValue(0);
            cubesToMarchCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);

            SetBuffer("relevantCubeCorners", relevantCubeCornersBuffer);
            SetBuffer("cubesToMarch", cubesToMarchBuffer);
            AddBuffer(cubesToMarchCountBuffer);
        }

        public override void Dispatch()
        {
            Dispatch(
                generator.constraint.scale.x * TerrainChunk.ChunkSizeInCubes.x / 7,
                generator.constraint.scale.y * TerrainChunk.ChunkSizeInCubes.y / 7,
                generator.constraint.scale.z * TerrainChunk.ChunkSizeInCubes.z / 7,
                GetProperties());
        }

        public override void GetData()
        {
            ComputeBuffer.CopyCount(cubesToMarchBuffer, cubesToMarchCountBuffer, 0);
            uint[] cubesToMarchCount = new uint[1] { 0 };
            cubesToMarchCountBuffer.GetData(cubesToMarchCount);
            uint3[] cubesToMarch = new uint3[cubesToMarchCount[0]];
            cubesToMarchBuffer.GetData(cubesToMarch);
            generator.marchingCubesShader.SetCubesToMarch(cubesToMarch);
        }

        public void Execute(uint[] relevantCubeCorners)
        {
            SetRelevantCubeCorners(relevantCubeCorners);
            SetBuffers();
            Dispatch();
            GetData();
            Release();
        }
    }
}