using System.Collections.Generic;
using LowPolyTerrain.Chunk;
using UnityEngine;
using static LowPolyTerrain.Chunk.TerrainChunkIndex;

namespace LowPolyTerrain
{
    class TerrainManipulatingObject : MonoBehaviour
    {
#pragma warning disable 649
        public GameObject loadingGroupObject;

        const float RESIZE_SPEED = 55f;
        const float MIN_SCALE = .1f;
        const float MAX_SCALE = 7f;
        const int MAX_RANGE = 100;
        const float ALTERING_POWER = 1f;

        GameObject sphereTool;

        void Start()
        {
            sphereTool = transform.Find("TerrainManipulatingSphere").gameObject;
        }

        void Update()
        {
            MoveTerrainSphere();
            HandleTerrainControls();
        }

        void MoveTerrainSphere()
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, MAX_RANGE))
            {
                sphereTool.GetComponent<SmoothMover>().ChageTargetPosition(hit.point);
            }
        }

        void HandleTerrainControls()
        {
            ResizeTerrainSphere();
            if (Input.GetMouseButtonUp(0))
            {
                AlterTerrain(ALTERING_POWER);
            }
            else if (Input.GetMouseButton(1))
            {
                AlterTerrain(-ALTERING_POWER);
            }
        }

        void ResizeTerrainSphere()
        {
            float scaleFactor = -Input.mouseScrollDelta.y * Time.deltaTime * RESIZE_SPEED;
            sphereTool.transform.localScale += new Vector3(scaleFactor, scaleFactor, scaleFactor);
            if (sphereTool.transform.localScale.x < MIN_SCALE)
            {
                scaleFactor = sphereTool.transform.localScale.x - MIN_SCALE;
                sphereTool.transform.localScale -= new Vector3(scaleFactor, scaleFactor, scaleFactor);
            }
            else if (sphereTool.transform.localScale.x > MAX_SCALE)
            {
                scaleFactor = sphereTool.transform.localScale.x - MAX_SCALE;
                sphereTool.transform.localScale -= new Vector3(scaleFactor, scaleFactor, scaleFactor);
            }
        }

        void AlterTerrain(float alteringPower)
        {
            AlterTerrain(alteringPower,
                    new HashSet<TerrainChunkIndex>(new TerrainChunkIndexComparer()) { TerrainChunkIndex.FromVector(sphereTool.transform.position) },
                    new HashSet<TerrainChunkIndex>(new TerrainChunkIndexComparer()));
        }

        void AlterTerrain(float alteringPower, HashSet<TerrainChunkIndex> indices, HashSet<TerrainChunkIndex> chunksDone)
        {
            foreach (TerrainChunkBehaviour terrainChunk in loadingGroupObject.GetComponentsInChildren<TerrainChunkBehaviour>())
            {
                foreach (TerrainChunkIndex index in indices)
                {
                    if (terrainChunk.chunk.index.Equals(index) && !chunksDone.Contains(terrainChunk.chunk.index))
                    {
                        HashSet<TerrainChunkIndex> newIndices = new HashSet<TerrainChunkIndex>(new TerrainChunkIndexComparer());
                        terrainChunk.Alter(
                            sphereTool.transform.position,
                            sphereTool.transform.localScale.x / 2,
                            alteringPower,
                            newIndices);
                        chunksDone.Add(terrainChunk.chunk.index);
                        AlterTerrain(alteringPower, newIndices, chunksDone);
                    }
                }
            }
        }
    }

}