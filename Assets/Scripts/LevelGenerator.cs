using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    int[,] levelMap = {
        {1,2,2,2,2,2,2,2,2,2,2,2,2,7},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,4},
        {2,6,4,0,0,4,5,4,0,0,0,4,5,4},
        {2,5,3,4,4,3,5,3,4,4,4,3,5,3},
        {2,5,5,5,5,5,5,5,5,5,5,5,5,5},
        {2,5,3,4,4,3,5,3,3,5,3,4,4,4},
        {2,5,3,4,4,3,5,4,4,5,3,4,4,3},
        {2,5,5,5,5,5,5,4,4,5,5,5,5,4},
        {1,2,2,2,2,1,5,4,3,4,4,3,0,4},
        {1,1,1,1,1,2,5,4,3,4,4,3,0,3},
        {1,1,1,1,1,2,5,4,4,0,0,0,0,0},
        {1,1,1,1,1,2,5,4,4,0,3,4,4,0},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0},
    };

    public GameObject innerWallTile;
    public GameObject outerWallTile;
    public GameObject innerCornerTile;
    public GameObject outerCornerTile;
    public GameObject pelletTile;
    public GameObject powerPelletTile;

    private float tileSize = 0.5f;  // Ensure the size is uniform for proper tile placement.
    private Vector3 cameraCenter;

    void Start()
    {
        // Set the center of the level to match the center of the camera's view
        cameraCenter = Camera.main.transform.position;

        // Delete existing level
        DeleteOldLevel();

        // Generate the new level
        GenerateLevel();
    }

    void DeleteOldLevel()
    {
        GameObject[] levelObjects = GameObject.FindGameObjectsWithTag("OldLevel");
        foreach (GameObject obj in levelObjects)
        {
            Destroy(obj);
        }
    }

    void GenerateLevel()
    {
        // Loop through each row and column of the levelMap
        for (int row = 0; row < levelMap.GetLength(0); row++)
        {
            for (int col = 0; col < levelMap.GetLength(1); col++)
            {
                // Adjusting the position to ensure there's no gap between tiles
                Vector3 position = new Vector3(row * tileSize, col * tileSize, 0)+ cameraCenter;
                int tileType = levelMap[row, col];

                InstantiateTile(tileType, position, row, col);
            }
        }

        // Call additional functions for mirroring the level
      /*  MirrorLevelHorizontally();
        MirrorLevelVertically();
        MirrorLevelBoth();
      */
    }

    void InstantiateTile(int tileType, Vector3 position, int row, int col)
    {
        switch (tileType)
        {
            case 0: // Empty space, do nothing
                break;
            case 1: // Outer Corner
                Instantiate(outerCornerTile, position, DetermineCornerRotation(row, col));
                break;
            case 2: // Outer Wall
                Instantiate(outerWallTile, position, DetermineWallRotation(row, col));
                break;
            case 3: // Inner Corner
                Instantiate(innerCornerTile, position, DetermineCornerRotation(row, col));
                break;
            case 4: // Inner Wall
                Instantiate(innerWallTile, position, DetermineWallRotation(row, col));
                break;
            case 5: // Pellet
                Instantiate(pelletTile, position, Quaternion.identity);
                break;
            case 6: // Power Pellet
                Instantiate(powerPelletTile, position, Quaternion.identity);
                break;
            case 7: // Special Outer Corner
                Instantiate(outerCornerTile, position, Quaternion.identity);
                break;
        }
    }

    // determine rotation for walls based on neighbors
    Quaternion DetermineWallRotation(int row, int col)
    {
        bool hasWallLeft = (col > 0 && ((levelMap[row, col - 1] == 2|| levelMap[row, col - 1] == 1)|| (levelMap[row, col - 1] == 4 || levelMap[row, col - 1] == 3)));
        bool hasWallRight = (col < levelMap.GetLength(1) - 1 &&( (levelMap[row, col + 1] == 2|| levelMap[row, col + 1] == 1)||(col < levelMap.GetLength(1) - 1 && (levelMap[row, col + 1] == 4|| levelMap[row, col + 1] == 3))));
        bool hasWallUp = (row > 0 &&((levelMap[row - 1, col] == 2||levelMap[row-1,col]==1)|| (levelMap[row - 1, col] == 4 || levelMap[row - 1, col] == 3)));
        bool hasWallDown = (row < levelMap.GetLength(0) - 1 && ((levelMap[row + 1, col] == 2|| levelMap[row + 1, col] == 1)||(levelMap[row + 1, col] == 4 || levelMap[row + 1, col] == 3)));

        // If wall is connected horizontally, no rotation (default)
        if (hasWallLeft || hasWallRight)
            return Quaternion.identity;

        // If wall is connected vertically, rotate 90 degrees
        if (hasWallUp || hasWallDown)
            return Quaternion.Euler(0, 0, 90);

        return Quaternion.identity;  // Default rotation for isolated walls
    }

    Quaternion DetermineCornerRotation(int row, int col)
    {
        bool hasWallLeft = (col > 0 && levelMap[row, col - 1] == 2);
        bool hasWallRight = (col < levelMap.GetLength(1) - 1 && levelMap[row, col + 1] == 2);
        bool hasWallUp = (row > 0 && levelMap[row - 1, col] == 2);
        bool hasWallDown = (row < levelMap.GetLength(0) - 1 && levelMap[row + 1, col] == 2);

        if (hasWallLeft && hasWallUp) return Quaternion.Euler(0, 0, 0);  // No rotation
        if (hasWallUp && hasWallRight) return Quaternion.Euler(0, 0, 90);  // 90 degrees
        if (hasWallRight && hasWallDown) return Quaternion.Euler(0, 0, 180);  // 180 degrees
        if (hasWallDown && hasWallLeft) return Quaternion.Euler(0, 0, 270);  // 270 degrees

        return Quaternion.identity;  // Default rotation
    }
    /*
    void MirrorLevelHorizontally()
    {
        for (int row = 0; row < levelMap.GetLength(0); row++)
        {
            for (int col = 0; col < levelMap.GetLength(1); col++)
            {
                Vector3 mirroredPosition = new Vector3(-col * tileSize, -row * tileSize, 0) + cameraCenter * 2;  // Mirroring on X-axis
                int tileType = levelMap[row, col];
                InstantiateTile(tileType, mirroredPosition, row, col);
            }
        }
    }

    void MirrorLevelVertically()
    {
        for (int row = 0; row < levelMap.GetLength(0); row++)
        {
            for (int col = 0; col < levelMap.GetLength(1); col++)
            {
                Vector3 mirroredPosition = new Vector3(col * tileSize, row * tileSize, 0) + cameraCenter * 2;  // Mirroring on Y-axis
                int tileType = levelMap[row, col];
                InstantiateTile(tileType, mirroredPosition, row, col);
            }
        }
    }

    void MirrorLevelBoth()
    {
        for (int row = 0; row < levelMap.GetLength(0); row++)
        {
            for (int col = 0; col < levelMap.GetLength(1); col++)
            {
                Vector3 mirroredPosition = new Vector3(-col * tileSize, row * tileSize, 0) + cameraCenter * 2;  // Mirroring on both axes
                int tileType = levelMap[row, col];
                InstantiateTile(tileType, mirroredPosition, row, col);
            }
        }
    }
        */
}
