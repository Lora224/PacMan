using UnityEngine;

public class CherryController : MonoBehaviour
{
    public GameObject cherryPrefab;  
    private float spawnInterval = 10f;  // Time interval between cherry spawns
    private Vector2 spawnPosition;
    private void Start()
    {

        InvokeRepeating("SpawnCherry", spawnInterval, spawnInterval);
    }

    private void SpawnCherry()
    {
        // Get a random spawn position outside the camera view
        spawnPosition = GetRandomOffScreenPosition();

        // Instantiate the cherry at the random position
        GameObject cherry = Instantiate(cherryPrefab, spawnPosition, Quaternion.identity);

        // Add CherryMovement component to handle movement
        CherryMovement movement = cherry.AddComponent<CherryMovement>();
        movement.spawnPosition = spawnPosition;
        Vector2 center = new Vector2(-4, 0);
        movement.SetTarget(center);  // Set the target to the center of the level
    }

    private Vector2 GetRandomOffScreenPosition()
    {
        Camera cam = Camera.main;
        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        // Choose a random side of the screen to spawn from (left, right, top, bottom)
        int randomSide = Random.Range(0, 4);
        Vector2 position = Vector2.zero;

        switch (randomSide)
        {
            case 0:  // Left side
                position = new Vector2(-camWidth / 2 - 1, Random.Range(-camHeight / 2, camHeight / 2));
                break;
            case 1:  // Right side
                position = new Vector2(camWidth / 2 + 1, Random.Range(-camHeight / 2, camHeight / 2));
                break;
            case 2:  // Top side
                position = new Vector2(Random.Range(-camWidth / 2, camWidth / 2), camHeight / 2 + 1);
                break;
            case 3:  // Bottom side
                position = new Vector2(Random.Range(-camWidth / 2, camWidth / 2), -camHeight / 2 - 1);
                break;
        }

        return position;
    }
}
