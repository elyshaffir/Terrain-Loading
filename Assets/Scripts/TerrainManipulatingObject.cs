using System.Collections.Generic;
using UnityEngine;
using static TerrainChunkIndex;

public class TerrainManipulatingObject : MonoBehaviour
{
    /*
    - Altering terrain on a REALLY large area is somewhat slow    

    NEW ALTERATION ALGORITM
    -----------------------
    1. Predict the positions of the to-be-altered points using the sphere's position, radius and the chunk-resolution (for now it is always one)    
    2. As the prediction process is happenning, create the alteration dictionary using the points and their corresponding chunks
        - a point's chunk can be determined by: Mathf.CeilToInt(position / ChunkSize) or something along those lines.
    3. Add the alteration dictionary to TerrainAlterationManager.
     */
    public GameObject loadingGroupObject;

    private const float RESIZE_SPEED = 55f;
    private const float MIN_SCALE = .1f;
    private const float MAX_SCALE = 7f;
    private const int MAX_RANGE = 100;
    private const float ALTERING_POWER = 1f;


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
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, MAX_RANGE))
        {
            sphereTool.GetComponent<SmoothMover>().ChageTargetPosition(hit.point);
        }
    }

    private void HandleTerrainControls()
    {
        ResizeTerrainSphere();
        if (Input.GetMouseButton(0))
        {
            AlterTerrain(ALTERING_POWER);
        }
        else if (Input.GetMouseButton(1))
        {
            AlterTerrain(-ALTERING_POWER);
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

    private void AlterTerrain(float alteringPower)
    {
        AlterTerrain(alteringPower,
                new HashSet<TerrainChunkIndex>(new TerrainChunkIndexComparer()) { TerrainChunkIndex.FromVector(sphereTool.transform.position) },
                new HashSet<TerrainChunkIndex>(new TerrainChunkIndexComparer()));
    }

    private void AlterTerrain(float alteringPower, HashSet<TerrainChunkIndex> indices, HashSet<TerrainChunkIndex> chunksDone)
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
