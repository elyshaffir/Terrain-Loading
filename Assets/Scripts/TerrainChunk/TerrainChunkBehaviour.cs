﻿using System.Collections.Generic;
using UnityEngine;

public class TerrainChunkBehaviour : MonoBehaviour
{
    public TerrainChunk chunk;

    public void Alter(Vector3 spherePosition, float sphereRadius, float power, HashSet<TerrainChunkIndex> additionalIndices)
    {
        chunk.Alter(spherePosition, sphereRadius, power, additionalIndices);
    }
}
