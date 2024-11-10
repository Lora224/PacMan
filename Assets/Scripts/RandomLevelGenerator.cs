using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomLevelGenerator : MonoBehaviour
{
    public static int[,] levelMap = new int[15, 14];
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

    // Coordinates for the center area to preserve
    private readonly List<int[]> centerArea = new List<int[]>
    {
        new int[] { 6, 5 }, new int[] { 6, 6 }, new int[] { 6, 7 }, new int[] { 6, 8 },
        new int[] { 7, 5 }, new int[] { 7, 6 }, new int[] { 7, 7 }, new int[] { 7, 8 },
        new int[] { 8, 5 }, new int[] { 8, 6 }, new int[] { 8, 7 }, new int[] { 8, 8 }
    };

    private int powerPelletCount = 0;

    void Start()
    {
        cameraCenter = Camera.main.transform.position;
        DeleteOldLevel();
        GenerateRandomLevelMap();
        MirrorLevelMap();
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

    void GenerateRandomLevelMap()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                // Ensure outer walls are set on the boundaries
                if (row == 0 || row == rows - 1 || col == 0 || col == cols - 1)
                {
                    levelMap[row, col] = 2; // Outer wall
                }
                else if (IsBottomRightCorner(row, col))
                {
                    levelMap[row, col] = 4; // 1/4 inner wall block in the bottom-right corner
                }
                else
                {
                    // Randomly assign values inside the outer walls
                    bool isInCenterArea = false;
                    foreach (var position in centerArea)
                    {
                        if (row == position[0] && col == position[1])
                        {
                            isInCenterArea = true;
                            levelMap[row, col] = 0; // Ensure the center is empty (or adjust as needed)
                            break;
                        }
                    }

                    if (!isInCenterArea)
                    {
                        // Randomize inner map generation while ensuring power pellet count doesn't exceed 5
                        if (powerPelletCount < 5 && Random.Range(0, 100) < 5) // 5% chance to place a power pellet
                        {
                            levelMap[row, col] = 6; // Power pellet
                            powerPelletCount++;
                        }
                        else
                        {
                            levelMap[row, col] = Random.Range(0, 6); // Randomly assign other tile types
                        }
                    }
                }
            }
        }
    }

    bool IsBottomRightCorner(int row, int col)
    {
        return row == rows - 1 && col == cols - 1;
    }

    void GenerateLevel()
    {
        for (int row = 0; row < fullMap.GetLength(0); row++)
        {
            for (int col = 0; col < fullMap.GetLength(1); col++)
            {
                Vector3 position = new Vector3(col * tileSize, -row * tileSize, 0) + origin;
                int tileType = fullMap[row, col];
                InstantiateTile(tileType, position, row, col);
            }
        }
    }

    void InstantiateTile(int tileType, Vector3 position, int row, int col)
    {
        switch (tileType)
        {
            case 0: break; // Empty space, do nothing
            case 1: Instantiate(outerCornerTile, position, DetermineCornerRotation(row, col)); break;
            case 2: Instantiate(outerWallTile, position, DetermineWallRotation(row, col)); break;
            case 3: Instantiate(innerCornerTile, position, DetermineCornerRotation(row, col)); break;
            case 4: Instantiate(innerWallTile, position, DetermineWallRotation(row, col)); break;
            case 5: Instantiate(pelletTile, position, Quaternion.identity); break;
            case 6: Instantiate(powerPelletTile, position, Quaternion.identity); break;
            case 7: Instantiate(outerCornerTile, position, Quaternion.identity); break;
        }
    }

    Quaternion DetermineWallRotation(int row, int col)
    {
        bool hasWallLeft = (col > 0 && IsWallTile(levelMap[row, col - 1]));
        bool hasWallRight = (col < levelMap.GetLength(1) - 1 && IsWallTile(levelMap[row, col + 1]));
        bool hasWallUp = (row > 0 && IsWallTile(levelMap[row - 1, col]));
        bool hasWallDown = (row < levelMap.GetLength(0) - 1 && IsWallTile(levelMap[row + 1, col]));
        bool rightBorder = (col == levelMap.GetLength(1) - 1);
        bool leftBorder = (col == 0);
        bool topBorder = (row == 0);
        bool bottomBorder = (row == levelMap.GetLength(0) - 1);

        if ((hasWallUp || topBorder) && (hasWallRight || rightBorder) && (hasWallDown || bottomBorder) && (hasWallLeft || leftBorder))
            return Quaternion.identity;

        if ((hasWallUp || topBorder) && (hasWallRight || rightBorder) && (hasWallDown || bottomBorder))
            return Quaternion.Euler(0, 0, 90);
        if ((hasWallLeft || leftBorder) && (hasWallUp || topBorder) && (hasWallRight || rightBorder))
            return Quaternion.Euler(0, 0, 0);
        if ((hasWallLeft || leftBorder) && (hasWallDown || bottomBorder) && (hasWallRight || rightBorder))
            return Quaternion.Euler(0, 0, 0);
        if ((hasWallLeft || leftBorder) && (hasWallUp || topBorder) && (hasWallDown || bottomBorder))
            return Quaternion.Euler(0, 0, 270);

        if (hasWallLeft && hasWallRight && !hasWallUp && !hasWallDown)
            return Quaternion.identity;
        if (hasWallUp && hasWallDown && !hasWallLeft && !hasWallRight)
            return Quaternion.Euler(0, 0, 90);

        return Quaternion.identity;
    }

    Quaternion DetermineCornerRotation(int row, int col)
    {
        bool hasWallLeft = (col > 0 && IsWallTile(levelMap[row, col - 1]));
        bool hasWallRight = (col < levelMap.GetLength(1) - 1 && IsWallTile(levelMap[row, col + 1]));
        bool hasWallUp = (row > 0 && IsWallTile(levelMap[row - 1, col]));
        bool hasWallDown = (row < levelMap.GetLength(0) - 1 && IsWallTile(levelMap[row + 1, col]));

        if (hasWallLeft && hasWallUp && !hasWallRight && !hasWallDown)
            return Quaternion.Euler(0, 0, 180);
        if (hasWallUp && hasWallRight && !hasWallLeft && !hasWallDown)
            return Quaternion.Euler(0, 0, 90);
        if (hasWallRight && hasWallDown && !hasWallLeft && !hasWallUp)
            return Quaternion.Euler(0, 0, 0);
        if (hasWallDown && hasWallLeft && !hasWallRight && !hasWallUp)
            return Quaternion.Euler(0, 0, 270);

        return Quaternion.identity;
    }

    bool IsWallTile(int tileType)
    {
        return tileType == 1 || tileType == 2 || tileType == 3 || tileType == 4 || tileType == 7;
    }

    void MirrorLevelMap()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                fullMap[row, col] = levelMap[row, col];
            }
        }

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                fullMap[row, cols * 2 - col - 1] = levelMap[row, col];
            }
        }

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                fullMap[rows * 2 - row - 1, col] = levelMap[row, col];
            }
        }

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                fullMap[rows * 2 - row - 1, cols * 2 - col - 1] = levelMap[row, col];
            }
        }

        levelMap = fullMap;
    }
}
