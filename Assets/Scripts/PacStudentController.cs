using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private ParticleSystem movementDust;
    [SerializeField] private AudioClip normalMovementSound;
    [SerializeField] private AudioClip pelletMovementSound;
  //  public bool IsMoving;
    private AudioSource audioSource;
    private Vector2 targetPosition;
    private Vector2 startPosition;
    private float lerpTime = 0f;
    private bool isLerping = false;
    private string lastInput = "";
    private string currentInput = "";
    private Animator animator;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        targetPosition = transform.position;
        startPosition = transform.position;
    }

    private void Update()
    {
        // Get player input
        if (Input.GetKeyDown(KeyCode.W)) lastInput = "UP";
        else if (Input.GetKeyDown(KeyCode.S)) lastInput = "DOWN";
        else if (Input.GetKeyDown(KeyCode.A)) lastInput = "LEFT";
        else if (Input.GetKeyDown(KeyCode.D)) lastInput = "RIGHT";

        // If not currently moving, check for new movement
        if (!isLerping)
        {
            //Debug.Log("Not lerping");
            // Try to move in lastInput direction
           // Debug.Log(lastInput);
            switch (lastInput)
            {
                case "UP": animator.SetInteger("direction", 0); break;
                case "DOWN": animator.SetInteger("direction", 1); break;
                case "LEFT": animator.SetInteger("direction", 2); break;
                case "RIGHT": animator.SetInteger("direction", 3); break;
                default: animator.SetInteger("direction", 3); break;
            }
          
            if (CanMoveInDirection(lastInput))
            {
                currentInput = lastInput;
                animator.SetBool("turned", true);
                StartMove(currentInput);
               
            }
            // If can't move in lastInput direction, try continuing in currentInput direction
            else if (CanMoveInDirection(currentInput))
            {
                StartMove(currentInput);
                animator.SetBool("turned", true);
            }
            // If can't move in either direction, stop
            else
            {
                StopMovement();
            }
        }
        // If currently moving, continue lerp
        else
        {
            lerpTime += Time.deltaTime * moveSpeed;
            transform.position = Vector2.Lerp(startPosition, targetPosition, lerpTime);

            // Check if reached target
            if (lerpTime >= 1f)
            {
                transform.position = targetPosition;
                isLerping = false;
                lerpTime = 0f;
                startPosition = targetPosition;
            }
        }
    }

    private bool CanMoveInDirection(string direction)
    {
        if (string.IsNullOrEmpty(direction)) return false;

        Vector2 nextPos = GetNextGridPosition(direction);
        int gridX = Mathf.RoundToInt((nextPos.x+10.5f)/0.5f); // Offset based on origin in LevelGenerator
        int gridY = Mathf.RoundToInt((-nextPos.y+7f)/0.5f);   // Offset based on origin in LevelGenerator
        Debug.Log(nextPos);
        Debug.Log(gridX);
        Debug.Log(gridY);
        // Check if position is within grid bounds
        if (gridX < 0 || gridX >= LevelGenerator.levelMap.GetLength(1) ||
            gridY < 0 || gridY >= LevelGenerator.levelMap.GetLength(0))
            return false;

        // Check if next position is walkable (not a wall)
        int tileType = LevelGenerator.levelMap[gridY, gridX];
        Debug.Log(tileType);
        return tileType != 1 && tileType != 2 && tileType != 3 && tileType != 4 && tileType != 7;
    }

    private Vector2 GetNextGridPosition(string direction)
    {
        Vector2 currentPos = transform.position;
        switch (direction)
        {
            case "UP": return currentPos + new Vector2(0, 0.5f);
            case "DOWN": return currentPos + new Vector2(0, -0.5f);
            case "LEFT": return currentPos + new Vector2(-0.5f, 0);
            case "RIGHT": return currentPos + new Vector2(0.5f, 0);
            default: return currentPos;
        }
    }

    private void StartMove(string direction)
    {
        targetPosition = GetNextGridPosition(direction);
        startPosition = transform.position;
        isLerping = true;
        lerpTime = 0f;

        // Start movement effects
        if (movementDust && !movementDust.isPlaying)
            movementDust.Play();

        if (animator)
            animator.SetBool("IsMoving", true);

        // Check if next position has a pellet to determine which sound to play
        Vector2 nextPos = GetNextGridPosition(direction);
        int gridX = Mathf.RoundToInt((nextPos.x + 10.5f) / 0.5f);
        int gridY = Mathf.RoundToInt((-nextPos.y + 7f) / 0.5f);
        bool hasPellet = LevelGenerator.levelMap[gridY, gridX] == 5;

        PlayMovementSound(hasPellet);
    }

    private void StopMovement()
    {
        if (movementDust && movementDust.isPlaying)
            movementDust.Stop();

        if (animator)
            animator.SetBool("IsMoving", false);

        if (audioSource.isPlaying)
            audioSource.Stop();
    }

    private void PlayMovementSound(bool hasPellet)
    {
        if (audioSource && !audioSource.isPlaying)
        {
            audioSource.clip = hasPellet ? pelletMovementSound : normalMovementSound;
            audioSource.Play();
        }
    }
}

