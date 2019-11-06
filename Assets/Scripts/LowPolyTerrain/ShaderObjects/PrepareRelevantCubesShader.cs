using System.Collections.Generic;
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
                new ComputeShaderIntProperty("numPointsY", generator.constraint.scale.y * TerrainChunk.ChunkSizeInPoints.y),
                new ComputeShaderIntProperty("numPointsZ", generator.constraint.scale.z * TerrainChunk.ChunkSizeInPoints.z)
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

            if (true || generator.constraint.position.Equals(new Vector3(-28, 0, -28)))
            {
                foreach (uint3 marchedCube in cubesToMarch)
                {
                    TerrainLoadingObject.current.marchedCubes.Add(new Vector3(marchedCube.x, marchedCube.y, marchedCube.z) + generator.constraint.position);
                }
            }

            // Changing the value of an array from multi-threaded program is non deterministic            
            // A possible solution is to utilize the fact that the (p.surfaceLevel < isoLevel) conditional when flipped gives accurate results from the other side
            // -- and inaccurate results from the other side (that gets accurate results when the conditional is as usual)
            ///
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