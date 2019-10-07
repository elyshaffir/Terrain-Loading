using System;
using System.Collections.Generic;
using UnityEngine;
using static TerrainChunkIndex;

public class TerrainChunkAlterationManager
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

    public class Vector3Comparer : IEqualityComparer<Vector3>
    {
        public bool Equals(Vector3 v1, Vector3 v2)
        {
            return v1.x == v2.x && v1.y == v2.y && v1.z == v2.z;
        }

        public int GetHashCode(Vector3 obj)
        {
            return obj.x.GetHashCode() ^ obj.y.GetHashCode() << 2 ^ obj.z.GetHashCode() >> 2;
        }
    }
}