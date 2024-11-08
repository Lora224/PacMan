using UnityEngine;

public class CherryMovement : MonoBehaviour
{
    public float moveSpeed = 1f;  // Speed at which the cherry moves
    private Vector2 targetPosition;  // The target position for lerping
    public Vector2 spawnPosition;

    public void SetTarget(Vector2 target)
    {
        targetPosition = target;  // Set the target position (center of the screen)
    }

    private void Update()
    {
        // Move the cherry towards the target position using linear interpolation
        transform.position = Vector2.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // If the cherry is close enough to the target, move it further past the screen
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {

            targetPosition = GetOutOfScreenPosition(spawnPosition);
        }

        // Destroy the cherry if it moves out of the camera view
        if (IsOutOfCameraView())
        {
            Destroy(gameObject);
        }
    }

    private Vector2 GetOutOfScreenPosition(Vector2 spawnPosition)
    {
        Camera cam = Camera.main;
        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        Vector2 screenCenter = new Vector2(-4,0);
        Vector2 direction = (screenCenter - spawnPosition).normalized;
        Vector2 outOfScreenPosition = spawnPosition + direction * Mathf.Max(camWidth, camHeight) * 2;
        return outOfScreenPosition;
    }


    private bool IsOutOfCameraView()
    {
        Vector3 screenPoint = Camera.main.WorldToViewportPoint(transform.position);
        return screenPoint.x < 0 || screenPoint.x > 1 || screenPoint.y < 0 || screenPoint.y > 1;
    }
}
