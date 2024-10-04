using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public static int[,] levelMap = {
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
        {0,0,0,0,0,2,5,4,3,4,4,3,0,3},
        {0,0,0,0,0,2,5,4,4,0,0,0,0,0},
        {0,0,0,0,0,2,5,4,4,0,3,4,4,0},
        {2,2,2,2,2,1,5,3,3,0,4,0,0,0},
        {0,0,0,0,0,0,5,0,0,0,4,0,0,0},
    };

    public static int rows = levelMap.GetLength(0);
    public static int cols = levelMap.GetLength(1);
    public static int[,] fullMap = new int[rows * 2, cols * 2];
    public GameObject innerWallTile;
    public GameObject outerWallTile;
    public GameObject innerCornerTile;
    public GameObject outerCornerTile;
    public GameObject pelletTile;
    public GameObject powerPelletTile;
    private Vector3 origin = new Vector3(-10.5f, 7, 0);
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
        MirrorLevelMap();
        //mirror Level Map
        // Loop through each row and column of the levelMap
        for (int row = 0; row < fullMap.GetLength(0); row++)
        {
            for (int col = 0; col < fullMap.GetLength(1); col++)
            {
                // Adjusting the position to ensure there's no gap between tiles
                Vector3 position = new Vector3(col * tileSize, -row * tileSize, 0)+origin;
                int tileType = fullMap[row, col];

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

        bool hasWallLeft = (col > 0 && (levelMap[row, col - 1] == 2 || levelMap[row, col - 1] == 1 || levelMap[row, col - 1] == 4 || levelMap[row, col - 1] == 3|| levelMap[row, col - 1] == 7));
        bool hasWallRight = (col < levelMap.GetLength(1) - 1 && (levelMap[row, col + 1] == 2 || levelMap[row, col + 1] == 1 || levelMap[row, col + 1] == 4 || levelMap[row, col + 1] == 3|| levelMap[row, col + 1] == 7));
        bool hasWallUp = (row > 0 && (levelMap[row - 1, col] == 2 || levelMap[row - 1, col] == 1 || levelMap[row - 1, col] == 4 || levelMap[row - 1, col] == 3|| levelMap[row - 1, col] == 7));
        bool hasWallDown = (row < levelMap.GetLength(0) - 1 && (levelMap[row + 1, col] == 2 || levelMap[row + 1, col] == 1 || levelMap[row + 1, col] == 4 || levelMap[row + 1, col] == 3|| levelMap[row + 1, col] == 7));
        bool rightBorder = (col == levelMap.GetLength(1) - 1);
        bool leftBorder = (col == 0);
        bool topBorder = (row == 0);
        bool bottomBorder = (row == levelMap.GetLength(0) - 1);
        if((hasWallUp || topBorder) && (hasWallRight || rightBorder) && (hasWallDown || bottomBorder)&& (hasWallLeft || leftBorder))//walls in 4 directions
            return Quaternion.identity;

        // Handle cases where walls exist in three or more directions
        if ((hasWallUp || topBorder) && (hasWallRight || rightBorder) && (hasWallDown || bottomBorder))
            return Quaternion.Euler(0, 0, 90); // Rotate for three-sided connection
        if ((hasWallLeft || leftBorder) && (hasWallUp || topBorder) && (hasWallRight || rightBorder))
            return Quaternion.Euler(0, 0, 0);// Rotate for three-sided connection
        if ((hasWallLeft || leftBorder) && (hasWallDown || bottomBorder) && (hasWallRight || rightBorder))
            return Quaternion.Euler(0, 0, 0);// Rotate for three-sided connection
        if ((hasWallLeft || leftBorder) && (hasWallUp || topBorder) && (hasWallDown || bottomBorder))
            return Quaternion.Euler(0, 0, 270); // Rotate for three-sided connection

        // Handle isolated walls
        if (hasWallLeft && hasWallRight && !hasWallUp && !hasWallDown)
            return Quaternion.identity; // Horizontal wall
        if (hasWallUp && hasWallDown && !hasWallLeft && !hasWallRight)
            return Quaternion.Euler(0, 0, 90); // Vertical wall



        return Quaternion.identity; // Default rotation for other walls
    }

    Quaternion DetermineCornerRotation(int row, int col)
    {

        bool hasWallLeft = (col > 0 && (levelMap[row, col - 1] == 2 || levelMap[row, col - 1] == 1 || levelMap[row, col - 1] == 4 || levelMap[row, col - 1] == 3));
        bool hasWallRight = (col < levelMap.GetLength(1) - 1 && (levelMap[row, col + 1] == 2 || levelMap[row, col + 1] == 1 || levelMap[row, col + 1] == 4 || levelMap[row, col + 1] == 3));
        bool hasWallUp = (row > 0 && (levelMap[row - 1, col] == 2 || levelMap[row - 1, col] == 1 || levelMap[row - 1, col] == 4 || levelMap[row - 1, col] == 3));
        bool hasWallDown = (row < levelMap.GetLength(0) - 1 && (levelMap[row + 1, col] == 2 || levelMap[row + 1, col] == 1 || levelMap[row + 1, col] == 4 || levelMap[row + 1, col] == 3));
        bool rightBorder = (col == levelMap.GetLength(1) - 1);
        bool leftBorder = (col == 0);
        bool topBorder = (row == 0);
        bool bottomBorder = (row == levelMap.GetLength(0) - 1);
        if ((hasWallUp || topBorder) && (hasWallRight || rightBorder) && (hasWallDown || bottomBorder) && (hasWallLeft || leftBorder))//walls in 4 directions (T shape)
        {
            if (row > 0 && row < levelMap.GetLength(0) - 1 && col < levelMap.GetLength(1) - 1 && col > 0) {
                if ((levelMap[row + 1, col] == 1 || levelMap[row +1, col] == 3) && levelMap[row - 1, col - 1] != 1 && levelMap[row - 1, col - 1] != 2 && levelMap[row - 1, col - 1] != 3 && levelMap[row - 1, col - 1] != 4)
                {
                    return Quaternion.Euler(0, 0, 180);
                }
                if ( (levelMap[row + 1, col] == 1 || levelMap[row + 1, col] == 3) && levelMap[row - 1, col + 1] != 1 && levelMap[row - 1, col + 1] != 2 && levelMap[row - 1, col + 1] != 3 && levelMap[row - 1, col + 1] != 4)
                {
                    return Quaternion.Euler(0, 0, 90);
                }
                if ( (levelMap[row - 1, col] == 1 || levelMap[row - 1, col] == 3) && levelMap[row +1, col + 1] != 1 && levelMap[row + 1, col + 1] != 2 && levelMap[row + 1, col + 1] != 3 && levelMap[row + 1, col + 1] != 4)
                {
                    return Quaternion.Euler(0, 0, 0);
                }
                if ( (levelMap[row - 1, col] == 1 || levelMap[row - 1, col] == 3) && levelMap[row + 1, col -1] != 1 && levelMap[row + 1, col - 1] != 2 && levelMap[row + 1, col - 1] != 3 && levelMap[row + 1, col - 1] != 4)
                {
                    return Quaternion.Euler(0, 0, 270);
                  
                }
                if ( (levelMap[row, col + 1] == 1 || levelMap[row, col + 1] == 3) && levelMap[row + 1, col - 1] != 1 && levelMap[row + 1, col - 1] != 2 && levelMap[row + 1, col - 1] != 3 && levelMap[row+ 1, col - 1] != 4)
                {
                    return Quaternion.Euler(0, 0, 270);
                }
                if ((levelMap[row, col - 1] == 1 || levelMap[row, col - 1] == 3) && levelMap[row + 1, col + 1] != 1 && levelMap[row+ 1, col + 1] != 2 && levelMap[row + 1, col + 1] != 3 && levelMap[row+ 1, col + 1] != 4)
                {
                    return Quaternion.Euler(0, 0, 0);
                }
                if ((levelMap[row, col + 1] == 1 || levelMap[row, col + 1] == 3) && levelMap[row - 1, col - 1] != 1 && levelMap[row - 1, col - 1] != 2 && levelMap[row - 1, col - 1] != 3 && levelMap[row - 1, col - 1] != 4)
                {
                    return Quaternion.Euler(0, 0, 180);
                }
            }
        }// Handle specific corner cases
        if (hasWallLeft && hasWallUp && !hasWallRight && !hasWallDown)
              return Quaternion.Euler(0, 0, 180);// Top-left corner
        if (hasWallUp && hasWallRight && !hasWallLeft && !hasWallDown)
            return Quaternion.Euler(0, 0, 90); // Top-right corner
        if (hasWallRight && hasWallDown && !hasWallLeft && !hasWallUp)
            return Quaternion.Euler(0, 0, 0); // Bottom-right corner
        if (hasWallDown && hasWallLeft && !hasWallRight && !hasWallUp)
            return Quaternion.Euler(0, 0, 270); // Bottom-left corner

        // Handle T-shaped intersections or intersections with three walls
        if (hasWallLeft && hasWallUp && hasWallRight)
            return Quaternion.Euler(0, 0, 90); // T-shape from left, top, and right
        if (hasWallUp && hasWallRight && hasWallDown)
            return Quaternion.Euler(0, 0, 180); // T-shape from top, right, and bottom
        if (hasWallRight && hasWallDown && hasWallLeft)
            return Quaternion.Euler(0, 0, 90); // T-shape from right, bottom, and left
        if (hasWallDown && hasWallLeft && hasWallUp)
            return Quaternion.Euler(0, 0, 0); // T-shape from bottom, left, and top

        return Quaternion.identity; // Default for isolated corners
    }

    void MirrorLevelMap()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                fullMap[row, col] = levelMap[row, col]; // Top-left quadrant (original)
            }
        }

        // Mirror horizontally (right side)
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                fullMap[row, cols * 2 - col - 1] = levelMap[row, col]; // Top-right quadrant
            }
        }

        // Mirror vertically (bottom side)
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                fullMap[rows * 2 - row - 1, col] = levelMap[row, col]; // Bottom-left quadrant
            }
        }

        // Mirror both horizontally and vertically (bottom-right)
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                fullMap[rows * 2 - row - 1, cols * 2 - col - 1] = levelMap[row, col]; // Bottom-right quadrant
            }
        }

        // Optionally, replace the original levelMap with the fullMap or use fullMap for further processing
        levelMap = fullMap;

        // Debug print the full map for verification (optional)
        for (int row = 0; row < fullMap.GetLength(0); row++)
        {
            string line = "";
            for (int col = 0; col < fullMap.GetLength(1); col++)
            {
                line += fullMap[row, col].ToString() + " ";
            }
            Debug.Log(line);
        }
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
