using System;
using System.Collections.Generic;
using Comparers;
using LowPolyTerrain.Chunk;
using UnityEngine;
using static LowPolyTerrain.Chunk.TerrainChunkIndex;

namespace LowPolyTerrain.MeshGeneration
{
    class TerrainChunkAlterationManager
    {
        public static Dictionary<TerrainChunkIndex, Dictionary<Vector3, float>> alterations;

        public static void Init()
        {
            alterations = new Dictionary<TerrainChunkIndex, Dictionary<Vector3, float>>(new TerrainChunkIndexComparer());
        }

        public static void CreateChunk(TerrainChunkIndex index)
        {
            try
            {
                alterations.Add(index, new Dictionary<Vector3, float>(new Vector3Comparer()));
            }
            catch (ArgumentException) { }
        }

        public static void AddAlterations(TerrainChunkIndex index, Dictionary<Vector3, float> newAlterations)
        {
            Dictionary<Vector3, float> currentAlterations = alterations[index];
            foreach (KeyValuePair<Vector3, float> alteration in newAlterations)
            {
                try
                {
                    currentAlterations.Add(alteration.Key, alteration.Value);
                }
                catch (ArgumentException)
                {
                    currentAlterations[alteration.Key] = alteration.Value;
                }
            }
        }
    }
}