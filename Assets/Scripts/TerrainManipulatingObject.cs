using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManipulatingObject : MonoBehaviour
{
    /*    
    -Terrain Manipulation is working, but with the following limitations:
        1. Not the most efficient (Some chunks are being tested for alteration even though they don't get altered at all)
        2. The bigger the sphere the more cracks emerge, probably because it manipulates more points it really shouldn't touch.
        -- To solve 1 & 2 a re-write of IsChunkInRange is needed, where it takes into account the sphere radius and location better.
        3. when reloading the altered chunks, all of the alterations are reset.
        -- This can be done with a whole class holding the build data from all chunks (and perhaps all static TerrainChunk data in general)        
        --- From this class chunks will be saved to files (because there is only a need to save the changes to a chunk, not all of it)        
        4. When loading alterations, it is very slow.
     */
    /*
    Can use this for snow effect (the snow could also re-grow with the wind or somehing)
     */
    public GameObject loadingGroupObject;
    public int maxRange = 1000;
    public float power = 1f;

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
        foreach (TerrainChunkBehaviour terrainChunk in loadingGroupObject.GetComponentsInChildren<TerrainChunkBehaviour>())
        {
            if (IsChunkInRange(terrainChunk))
            {
                terrainChunk.Alter(
                    sphereTool.transform.position,
                    sphereTool.transform.localScale.x,
                    power);
            }
        }
    }

    private bool IsChunkInRange(TerrainChunkBehaviour chunk)
    {
        // Assuming the sphere is not larger than the chunk size and that there is only horizontal chunks        
        return TerrainChunkIndex.FromVector(sphereTool.transform.position).IsAdjacent(chunk.chunk.index);
    }
}
