using ComputeShading;
using LowPolyTerrain.Chunk;
using LowPolyTerrain.MeshGeneration;
using LowPolyTerrain.MeshGeneration.DataStructures;
using UnityEngine;
using static ComputeShading.ComputeShaderProperty;

namespace LowPolyTerrain.ShaderObjects
{
    class GetPointsToAlterShader : ComputeShaderObject
    {
        public static ComputeShader getPointsToAlterShader;

        readonly TerrainChunkMeshGenerator generator;

        ComputeBuffer pointsToAlterBuffer;
        ComputeBuffer pointsToAlterCountBuffer;
        ComputeBuffer inputPoints;

        float sphereRadius;
        Vector3 spherePosition;
        int[] pointsToAlter;

        public GetPointsToAlterShader(TerrainChunkMeshGenerator generator) :
            base(getPointsToAlterShader,
                getPointsToAlterShader.FindKernel("GetPointsToAlter"))
        {
            this.generator = generator;
        }

        public void SetSphere(float sphereRadius, Vector3 spherePosition)
        {
            this.sphereRadius = sphereRadius;
            this.spherePosition = spherePosition;
        }

        protected override ComputeShaderProperty[] GetProperties()
        {
            return new ComputeShaderProperty[] {
                new ComputeShaderIntProperty("numPointsX", generator.constraint.scale.x * TerrainChunk.ChunkSize.x),
                new ComputeShaderIntProperty("numPointsY", generator.constraint.scale.y * TerrainChunk.ChunkSize.y),
                new ComputeShaderIntProperty("numPointsZ", generator.constraint.scale.z * TerrainChunk.ChunkSize.z),
                new ComputeShaderFloatProperty("sphereRadius", sphereRadius),
                new ComputeShaderVector3Property("spherePosition", spherePosition)
            };
        }

        public override void SetBuffers()
        {
            pointsToAlterBuffer = new ComputeBuffer(
                            generator.points.Length, sizeof(int), ComputeBufferType.Append);
            pointsToAlterBuffer.SetCounterValue(0);
            pointsToAlterCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
            inputPoints = new ComputeBuffer(generator.points.Length, Point.StructSize);
            inputPoints.SetData(generator.points);

            SetBuffer("pointsToAlter", pointsToAlterBuffer);
            SetBuffer("points", inputPoints);
            AddBuffer(pointsToAlterCountBuffer);
        }

        public override void Dispatch()
        {
            Dispatch(
                generator.constraint.scale.x * TerrainChunk.ChunkSize.x / 5,
                generator.constraint.scale.y * TerrainChunk.ChunkSize.y / 5,
                generator.constraint.scale.z * TerrainChunk.ChunkSize.z / 5,
                GetProperties());
        }

        public override void GetData()
        {
            ComputeBuffer.CopyCount(pointsToAlterBuffer, pointsToAlterCountBuffer, 0);
            int[] pointsToAlterCount = new int[1] { 0 };
            pointsToAlterCountBuffer.GetData(pointsToAlterCount);
            pointsToAlter = new int[pointsToAlterCount[0]];
            pointsToAlterBuffer.GetData(pointsToAlter);
        }

        public int[] Execute(float sphereRadius, Vector3 spherePosition)
        {
            SetSphere(sphereRadius, spherePosition);
            SetBuffers();
            Dispatch();
            GetData();
            Release();
            return pointsToAlter;
        }
    }
}