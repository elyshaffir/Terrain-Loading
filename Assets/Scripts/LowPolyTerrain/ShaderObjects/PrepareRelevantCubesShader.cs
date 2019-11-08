using ComputeShading;
using LowPolyTerrain.Chunk;
using LowPolyTerrain.MeshGeneration;
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

        public PrepareRelevantCubesShader(TerrainChunkMeshGenerator generator, ComputeBuffer relevantCubeCornersBuffer) :
            base(prepareRelevantCubesShader,
                prepareRelevantCubesShader.FindKernel("PrepareRelevantCubes"))
        {
            this.generator = generator;
            this.relevantCubeCornersBuffer = relevantCubeCornersBuffer;
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
            cubesToMarchBuffer = new ComputeBuffer(generator.points.Length, sizeof(uint) * 3, ComputeBufferType.Append);
            cubesToMarchBuffer.SetCounterValue(0);
            cubesToMarchCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);

            SetBuffer("relevantCubeCorners", relevantCubeCornersBuffer, false);
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
            Vector3Int[] cubesToMarch = new Vector3Int[cubesToMarchCount[0]];
            cubesToMarchBuffer.GetData(cubesToMarch);
            generator.marchingCubesShader.SetCubesToMarch(cubesToMarch);
            generator.surfaceLevelShader.SetRelevant(cubesToMarch.Length > 0 && cubesToMarch.Length != generator.points.Length);
        }

        public void Execute()
        {
            SetBuffers();
            Dispatch();
            GetData();
            Release();
        }
    }
}