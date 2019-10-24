using System;
using System.Collections.Generic;
using UnityEngine;
using static TerrainChunkIndex;

public class TerrainManipulatingObject : MonoBehaviour
{
    /*
    NEW ALTERATION ALGORITM
    -----------------------
    1. Predict the positions of the to-be-altered points using the sphere's position, radius and the chunk-resolution (for now it is always one)    
    2. As the prediction process is happenning, create the alteration dictionary using the points and their corresponding chunks
        - a point's chunk can be determined by: Mathf.CeilToInt(position / ChunkSize) or something along those lines.
    3. Add the alteration dictionary to TerrainAlterationManager.

    HAS NOT BEEN DONE BECAUSE
    -------------------------
    1. It would result in the same number of times looping the points array in the generator, because we need to apply the changes to the mesh and you can't
       get a dictionary from the compute shaders.
    2. The problem in the performance is half - looping the points array and half - generating the mesh.
    3. Just putting the looping over the points array in seperate threads (even on really large chunks) didn't result in much improvement.

    IT SHOULD STILL BE DONE BECAUSE
    -------------------------------
    Later on, manipulation will not be done with only spheres, and its healthy to keep the options open and allow terrain manipulation by point location.
     */
    public GameObject loadingGroupObject;

    private const float RESIZE_SPEED = 55f;
    private const float MIN_SCALE = 1f;
    private const float MAX_SCALE = 15f;
    private const int MAX_RANGE = 100;
    private const float ALTERING_POWER = 1f;


    private GameObject sphereTool;

    List<Vector3> alteredPoints;

    void Awake()
    {
        alteredPoints = new List<Vector3>();
    }

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

    private void AlterTerrain1(float alteringPower)
    {
        AlterTerrain(alteringPower,
                new HashSet<TerrainChunkIndex>(new TerrainChunkIndexComparer()) { TerrainChunkIndex.FromVector(sphereTool.transform.position) },
                new HashSet<TerrainChunkIndex>(new TerrainChunkIndexComparer()));
    }

    private void AlterTerrain(float alteringPower)
    {
        // This method assumes that the sphereTool is a sphere        
        Dictionary<Vector3, float> alterations = new Dictionary<Vector3, float>();
        alteredPoints = new List<Vector3>();
        Vector3 radiusScale = sphereTool.transform.localScale / 2;
        for (float x = -radiusScale.x; x <= radiusScale.x; x++)
        {
            int currentX = Mathf.FloorToInt(sphereTool.transform.position.x + x);
            for (float y = -radiusScale.y; y <= radiusScale.y; y++)
            {
                int currentY = Mathf.FloorToInt(sphereTool.transform.position.y + y);
                for (float r = 1; r <= radiusScale.z; r++)
                {
                    int currentZ = Mathf.FloorToInt(sphereTool.transform.position.z + r);
                    Vector3 currentPoint = new Vector3(currentX, currentY, currentZ);
                    if (Vector3.Distance(currentPoint, sphereTool.transform.position) <= radiusScale.z)
                    {
                        int oppositeZ = Mathf.FloorToInt(sphereTool.transform.position.z - r);
                        Vector3 oppositePoint = new Vector3(currentX, currentY, oppositeZ);
                        alterations.Add(currentPoint, alteringPower);
                        alterations.Add(oppositePoint, alteringPower);
                        alteredPoints.Add(currentPoint);
                        alteredPoints.Add(oppositePoint);
                    }
                    else
                    {
                        break;
                    }
                }
                Vector3 pointZero = new Vector3(currentX, currentY, Mathf.FloorToInt(sphereTool.transform.position.z));
                if (Vector3.Distance(pointZero, sphereTool.transform.position) <= radiusScale.z)
                {
                    alterations.Add(pointZero, alteringPower);
                    alteredPoints.Add(pointZero);
                }
            }
        }
        HashSet<TerrainChunkIndex> alteredChunks = TerrainChunkAlterationManager.AddAlterations(alterations);
        foreach (TerrainChunkBehaviour terrainChunk in loadingGroupObject.GetComponentsInChildren<TerrainChunkBehaviour>())
        {
            if (alteredChunks.Contains(terrainChunk.chunk.index))
            {
                terrainChunk.Alter();
            }
        }
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

    public void OnDrawGizmos()
    {
        foreach (Vector3 point in alteredPoints)
        {
            Gizmos.DrawWireSphere(point, .1f);
        }
    }
}
