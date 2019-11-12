using System.Collections.Generic;
using ComputeShading;
using LowPolyTerrain.Chunk;
using LowPolyTerrain.MeshGeneration;
using UnityEngine;
using static ComputeShading.ComputeShaderProperty;

namespace LowPolyTerrain.ShaderObjects
{
    class GetPointsToAlterShader : ComputeShaderObject
    {
        public static ComputeShader getPointsToAlterShader;

        readonly TerrainChunkMeshGenerator generator;

        ComputeBuffer relevantCubeCornersBuffer;
        ComputeBuffer onEdgesBuffer;
        PrepareRelevantCubesShader prepareRelevantCubesShader;

        float sphereRadius;
        Vector3 spherePosition;
        int[] onEdges;

        public GetPointsToAlterShader(TerrainChunkMeshGenerator generator) :
            base(getPointsToAlterShader,
                getPointsToAlterShader.FindKernel("GetPointsToAlter"))
        {
            this.generator = generator;
        }

        void SetSphere(float sphereRadius, Vector3 spherePosition)
        {
            this.sphereRadius = sphereRadius;
            this.spherePosition = spherePosition;
        }

        protected override ComputeShaderProperty[] GetProperties()
        {
            return new ComputeShaderProperty[] {
                new ComputeShaderIntProperty("numPointsX", generator.constraint.scale.x * TerrainChunk.ChunkSizeInCubes.x),
                new ComputeShaderIntProperty("numPointsY", generator.constraint.scale.y * TerrainChunk.ChunkSizeInCubes.y),
                new ComputeShaderIntProperty("numPointsZ", generator.constraint.scale.z * TerrainChunk.ChunkSizeInPoints.z),
                new ComputeShaderFloatProperty("power", 1f),
                new ComputeShaderVector3Property("chunkPosition", generator.constraint.position),
                new ComputeShaderFloatProperty("sphereRadius", sphereRadius),
                new ComputeShaderVector3Property("spherePosition", spherePosition)
            };
        }

        public override void SetBuffers()
        {
            relevantCubeCornersBuffer = new ComputeBuffer(generator.constraint.GetVolume(), sizeof(uint)); // if the initial value is not set to 0 it might pose a problem
            onEdgesBuffer = new ComputeBuffer(6, sizeof(int));
            SetBuffer("points", generator.surfaceLevelShader.pointsBuffer, false);
            SetBuffer("relevantCubeCorners", relevantCubeCornersBuffer);
            SetBuffer("onEdges", onEdgesBuffer);
        }

        public override void Dispatch()
        {
            Dispatch(
                generator.constraint.scale.x * TerrainChunk.ChunkSizeInCubes.x / 5,
                generator.constraint.scale.y * TerrainChunk.ChunkSizeInCubes.y / 5,
                generator.constraint.scale.z * TerrainChunk.ChunkSizeInCubes.z / 5,
                GetProperties());
        }

        public override void GetData()
        {
            prepareRelevantCubesShader = new PrepareRelevantCubesShader(generator, relevantCubeCornersBuffer);
            // prepareRelevantCubesShader.Execute();

            // Create a new version of PrepareRelevantCubesShader which adds
            // the new relevant cubes to the old ones calculated at generation
            // -------- OR
            // dont dispose of cubesToMarch and it's counter and use the same shader!
            onEdges = new int[6];
            onEdgesBuffer.GetData(onEdges);
        }

        public override void Release()
        {
            base.Release();
            prepareRelevantCubesShader.Release();
        }

        public void Execute(float sphereRadius, Vector3 spherePosition, TerrainChunkIndex index, HashSet<TerrainChunkIndex> additionalIndices)
        {
            SetSphere(sphereRadius, spherePosition);
            SetBuffers();
            Dispatch();
            GetData();
            Release();
            index.GetEdgeChunks(onEdges, additionalIndices);
        }
    }
}