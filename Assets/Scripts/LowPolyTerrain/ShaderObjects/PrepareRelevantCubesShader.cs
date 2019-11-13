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
            cubesToMarchBuffer = new ComputeBuffer(generator.constraint.GetVolume(), sizeof(uint) * 3, ComputeBufferType.Append);
            cubesToMarchBuffer.SetCounterValue(0);
            cubesToMarchCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);

            SetBuffer("relevantCubeCorners", relevantCubeCornersBuffer, false);
            SetBuffer("cubesToMarch", cubesToMarchBuffer, false); // Make sure this buffer is not released before MarchingCubesShader is executed!
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
            generator.marchingCubesShader.SetCubesToMarch(cubesToMarchBuffer, cubesToMarchCount[0]);

            bool relevant = cubesToMarchCount[0] > 0 && cubesToMarchCount[0] != generator.constraint.GetVolume();
            generator.surfaceLevelShader.SetRelevant(relevant);
            if (!relevant)
            {
                AddBuffer(cubesToMarchBuffer);
            }
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