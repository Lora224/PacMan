using UnityEngine;




public class CherryController : MonoBehaviour


{
    [SerializeField] private float moveSpeed = 3f;
    private Vector3 targetPosition;
    private Vector3 startPosition;
    private float lerpTime = 0f;
    private static float spawnTimer = 0f;
    private static float spawnInterval = 10f;

    private void Start()
    {
        // Set random starting position outside camera view
        Camera mainCamera = Camera.main;
        float height = mainCamera.orthographicSize;
        float width = height * mainCamera.aspect;

        // Randomly choose a side to spawn from
        int side = Random.Range(0, 4);
        switch (side)
        {
            case 0: // Top
                startPosition = new Vector3(Random.Range(-width, width), height + 1, 0);
                break;
            case 1: // Right
                startPosition = new Vector3(width + 1, Random.Range(-height, height), 0);
                break;
            case 2: // Bottom
                startPosition = new Vector3(Random.Range(-width, width), -height - 1, 0);
                break;
            case 3: // Left
                startPosition = new Vector3(-width - 1, Random.Range(-height, height), 0);
                break;
        }

        transform.position = startPosition;

        // Set target to opposite side through center
        Vector3 center = Vector3.zero;
        Vector3 directionToCenter = (center - startPosition).normalized;
        targetPosition = startPosition + directionToCenter * ((width + height) * 2);
    }

    private void Update()
    {
        lerpTime += Time.deltaTime * moveSpeed;
        transform.position = Vector3.Lerp(startPosition, targetPosition, lerpTime);

        // Destroy if outside camera view
        Vector3 viewportPoint = Camera.main.WorldToViewportPoint(transform.position);
        if (viewportPoint.x < -0.1f || viewportPoint.x > 1.1f ||
            viewportPoint.y < -0.1f || viewportPoint.y > 1.1f)
        {
            Destroy(gameObject);
        }
    }

    public static void UpdateSpawnTimer()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnCherry();
        }
    }

    private static void SpawnCherry()
    {
        // This should be called from your game manager
        GameObject cherryPrefab = Resources.Load<GameObject>("Cherry");
        if (cherryPrefab != null)
        {
            Instantiate(cherryPrefab);
        }
    }
}