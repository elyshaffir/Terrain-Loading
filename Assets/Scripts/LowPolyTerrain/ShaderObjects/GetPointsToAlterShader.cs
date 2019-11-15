using System;
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

        public ComputeBuffer alterationsBuffer;

        readonly TerrainChunkMeshGenerator generator;

        ComputeBuffer relevantCubeCornersBuffer;
        ComputeBuffer onEdgesBuffer;
        PrepareRelevantCubesShader prepareRelevantCubesShader;

        float sphereRadius;
        Vector3 spherePosition;
        int[] onEdges;

        public GetPointsToAlterShader(TerrainChunkMeshGenerator generator, ComputeBuffer alterationsBuffer) :
            base(getPointsToAlterShader,
                getPointsToAlterShader.FindKernel("GetPointsToAlter"))
        {
            this.generator = generator;

            this.alterationsBuffer = alterationsBuffer;
            ///  
            if (this.alterationsBuffer == null)
            {
                this.alterationsBuffer = new ComputeBuffer(generator.constraint.GetVolume(), sizeof(float));
                float[] alterations = new float[generator.constraint.GetVolume()];
                Array.Clear(alterations, 0, alterations.Length);
                this.alterationsBuffer.SetData(alterations);
            }
            ///
        }

        void SetSphere(float sphereRadius, Vector3 spherePosition)
        {
            this.sphereRadius = sphereRadius;
            this.spherePosition = spherePosition;
        }

        protected override ComputeShaderProperty[] GetProperties()
        {
            return new ComputeShaderProperty[] {
                new ComputeShaderIntProperty("numPointsX", generator.constraint.scale.x * TerrainChunk.ChunkSizeInPoints.x),
                new ComputeShaderIntProperty("numPointsY", generator.constraint.scale.y * TerrainChunk.ChunkSizeInPoints.y),
                new ComputeShaderIntProperty("numPointsZ", generator.constraint.scale.z * TerrainChunk.ChunkSizeInPoints.z),
                new ComputeShaderFloatProperty("power", 1f),
                new ComputeShaderVector3Property("chunkPosition", generator.constraint.position),
                new ComputeShaderFloatProperty("sphereRadius", sphereRadius),
                new ComputeShaderVector3Property("spherePosition", spherePosition),
                new ComputeShaderFloatProperty("isoLevel", SurfaceLevelShader.isoLevel)
            };
        }

        public override void SetBuffers()
        {
            relevantCubeCornersBuffer = new ComputeBuffer(generator.constraint.GetVolume(), sizeof(uint)); // if the initial value is not set to 0 it might pose a problem            

            onEdgesBuffer = new ComputeBuffer(6, sizeof(int));
            int[] onEdgesFill = new int[6]; // This seems to be necessary for some reason
            Array.Clear(onEdgesFill, 0, onEdgesFill.Length);
            onEdgesBuffer.SetData(onEdgesFill);

            SetBuffer("points", generator.surfaceLevelShader.pointsBuffer, false);
            ///
            SetBuffer("alterations", alterationsBuffer, false);
            ///
            SetBuffer("relevantCubeCorners", relevantCubeCornersBuffer);
            SetBuffer("onEdges", onEdgesBuffer);
        }

        public override void Dispatch()
        {
            Dispatch(
                generator.constraint.scale.x * TerrainChunk.ChunkSizeInPoints.x / 5,
                generator.constraint.scale.y * TerrainChunk.ChunkSizeInPoints.y / 5,
                generator.constraint.scale.z * TerrainChunk.ChunkSizeInPoints.z / 5,
                GetProperties());
        }

        public override void GetData()
        {
            // Perhaps use GenerateSurfaceLevel (or a simmilar version to it) that doesnt generate the noise,
            // but just checks for relevant cubes on the already existing array of points (points is already modified here)
            prepareRelevantCubesShader = new PrepareRelevantCubesShader(generator, relevantCubeCornersBuffer);
            prepareRelevantCubesShader.Execute();

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