using UnityEngine;

public class CherryMovement : MonoBehaviour
{
    public float moveSpeed = 1f;  // Speed at which the cherry moves
    private Vector2 startPosition;
    private Vector2 centerPosition;
    private Vector2 endPosition;
    private float journeyLength;
    private float progress = 0f;  
    public void InitializeMovement(Vector2 spawnPosition, Vector2 center)
    {
        startPosition = spawnPosition;
        centerPosition = center;
        endPosition = GetOutOfScreenPosition(center);

        // Calculate total journey length for interpolation
        journeyLength = Vector2.Distance(startPosition, endPosition);
    }

    private void Update()
    {
        float distancePerFrame = moveSpeed * Time.deltaTime / journeyLength;
        progress += distancePerFrame;

        // Lerp from startPosition to endPosition based on progress
        transform.position = Vector2.Lerp(startPosition, endPosition, progress);

        // Destroy the cherry if it has reached or exceeded the end position
        if (progress >= 1f)
        {
            Destroy(gameObject);
        }
    }

    private Vector2 GetOutOfScreenPosition(Vector2 centerPosition)
    {
        Camera cam = Camera.main;
        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        // Calculate the opposite side position for exit
        Vector2 direction = (centerPosition - startPosition).normalized;
        Vector2 outOfScreenPosition = centerPosition + direction * Mathf.Max(camWidth, camHeight) * 1.5f; // Extend beyond screen bounds

        return outOfScreenPosition;
    }
}
