using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;



public class GetWorldPosition : MonoBehaviour
{
    public Tilemap tilemap;  
    // Start is called before the first frame update
    void Start()
    {

        Vector3Int gridPosition1 = new Vector3Int(-21, 13, 0);
        Vector3Int gridPosition2 = new Vector3Int(-16, 13, 0);
        Vector3Int gridPosition3 = new Vector3Int(-16, 9, 0);
        Vector3Int gridPosition4 = new Vector3Int(-21, 9, 0);


        // Convert the grid position to world position
        Vector3 worldPosition1 = tilemap.CellToWorld(gridPosition1);
        Vector3 worldPosition2 = tilemap.CellToWorld(gridPosition2);
        Vector3 worldPosition3 = tilemap.CellToWorld(gridPosition3);
        Vector3 worldPosition4 = tilemap.CellToWorld(gridPosition4);
        // Log the world position to the console
        Debug.Log("World Position of Tile: " + worldPosition1);
        Debug.Log("World Position of Tile: " + worldPosition2);
        Debug.Log("World Position of Tile: " + worldPosition3);
        Debug.Log("World Position of Tile: " + worldPosition4);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

