using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TerrainChunkIndex;

public class TerrainManipulatingObject : MonoBehaviour
{
    /*    
    -Terrain Manipulation is working, but with the following limitations:                
        1. When loading alterations, it is very slow.
        2. Altering power doesn't matter
     */
    /*
    Can use this for snow effect (the snow could also re-grow with the wind or somehing)
     */

    public GameObject loadingGroupObject;
    public int maxRange = 1000;
    public float power = .1f;

    private const float RESIZE_SPEED = 55f;

    private const float MIN_SCALE = .1f;
    private const float MAX_SCALE = 7f;


    private GameObject sphereTool;

    void Start()
    {
        sphereTool = transform.Find("TerrainManipulatingSphere").gameObject;
    }

    void Update()
    {
        MoveTerrainSphere();
        HandleTerrainControls();
    }

    private void MoveTerrainSphere()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxRange))
        {
            sphereTool.GetComponent<SmoothMover>().ChageTargetPosition(hit.point);
        }
    }

    private void HandleTerrainControls()
    {
        ResizeTerrainSphere();
        if (Input.GetMouseButton(0))
        {
            AlterTerrain(power);
        }
        else if (Input.GetMouseButton(1))
        {
            AlterTerrain(-power);
        }
    }

    private void ResizeTerrainSphere()
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

    private void AlterTerrain(float power)
    {
        Debug.ClearDeveloperConsole();
        AlterTerrain(power,
                new HashSet<TerrainChunkIndex>(new TerrainChunkIndexComparer()) { TerrainChunkIndex.FromVector(sphereTool.transform.position) },
                new HashSet<TerrainChunkIndex>(new TerrainChunkIndexComparer()));
    }

    private void AlterTerrain(float power, HashSet<TerrainChunkIndex> indices, HashSet<TerrainChunkIndex> chunksDone)
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
                        power,
                        newIndices);
                    chunksDone.Add(terrainChunk.chunk.index);
                    AlterTerrain(power, newIndices, chunksDone);
                }
            }
        }
    }
}
