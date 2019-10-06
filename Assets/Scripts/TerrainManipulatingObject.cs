using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManipulatingObject : MonoBehaviour
{
    /*
    The collider needs to be updated for it to start reacting
    The position needs to be global
     */

    public GameObject loadingGroupObject;
    public int maxRange = 1000;

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
            AlterTerrain(GetAlterPower());
        }
        else if (Input.GetMouseButton(1))
        {
            AlterTerrain(-GetAlterPower());
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

    private float GetAlterPower()
    {
        return 1f; // Perhaps change to other calculation / variable
    }

    private void AlterTerrain(float power)
    /*
        - Change power to how close the point is to the center of the sphere - Not a must
        - A static class of cross-chunk triangles, updating whenever the player moves and builds - No Need
        - Make sure that reloading a chunk doesn't reset the builds
        -- This can be done with a whole class holding the build data from all chunks (and perhaps all static TerrainChunk data in general)
        --- This class can also handle cross-chunk triangles.
        ---- From this class chunks will be saved to files (because there is only a need to save the changes to a chunk, not all of it)
    */
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
    // The problem lies here (Thank god), because when it returns "true" always it works properly
    // The only problems that occure are when you go out and in of an altered chunk
    // The bigger the tool the more crackes emerge!
    // Optimization is needed badly
    {
        // Assuming the sphere is not larger than the chunk size and that there is only horizontal chunks        
        return TerrainChunkIndex.FromVector(sphereTool.transform.position).IsAdjacent(chunk.chunk.index);
    }
}
