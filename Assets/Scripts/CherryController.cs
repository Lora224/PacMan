using UnityEngine;
using System.Collections;

public class CherryController : MonoBehaviour
{
    public GameObject cherryPrefab;
    private float spawnInterval = 10f;  // Time interval between cherry spawns
    //private bool isInvoking = false;
    private void Start()
    {
        StartCoroutine(SpawnCherryCoroutine());
    }

    private void SpawnCherry()
    {
        // Debug log to verify when the cherry spawns
        Debug.Log("Spawning cherry at: " + Time.time);
        // Get a random spawn position outside the camera view
        Vector2 spawnPosition = GetRandomOffScreenPosition();

        // Instantiate the cherry at the random position
        GameObject cherry = Instantiate(cherryPrefab, spawnPosition, Quaternion.identity);

        // Add CherryMovement component to handle movement
        CherryMovement movement = cherry.AddComponent<CherryMovement>();
        Vector2 center = new Vector2(0, 0);  // Set the center of the level
        movement.InitializeMovement(spawnPosition, center);
    }

    private IEnumerator SpawnCherryCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnCherry();
        }
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
