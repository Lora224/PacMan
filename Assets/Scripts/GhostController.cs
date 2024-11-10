using UnityEngine;
using System.Collections.Generic;

public class GhostController : MonoBehaviour
{
    public enum GhostState { Walking, Scared, Recovering, Dead }
    public GhostState currentState = GhostState.Walking;
    public float moveSpeed = 5f;
    private Vector2 targetPosition;
    private Vector2 startPosition;
    private float lerpTime = 0f;
    private bool isLerping = false;
    private Vector2 lastPosition;
    private Animator animator;
    private PacStudentController pacStudent;
    private LevelGenerator levelGenerator;
    private AudioSource audioSource;

    public AudioClip normalStateAudio;
    public AudioClip scaredStateAudio;
    public AudioClip deadStateAudio;

    private void Start()
    {
        startPosition = transform.position;
        targetPosition = transform.position;
        lastPosition = transform.position;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        pacStudent = FindObjectOfType<PacStudentController>();
        levelGenerator = FindObjectOfType<LevelGenerator>();

        // Play audio for ghosts in normal state at the start
        if (currentState == GhostState.Walking && normalStateAudio != null)
        {
            audioSource.clip = normalStateAudio;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void Update()
    {
        if (!isLerping)
        {
            switch (currentState)
            {
                case GhostState.Walking:
                    HandleWalkingMovement();
                    break;
                case GhostState.Scared:
                case GhostState.Recovering:
                    HandleScaredMovement();
                    break;
                case GhostState.Dead:
                    HandleDeadMovement();
                    break;
            }
        }
        else
        {
            // Continue lerping if currently moving
            lerpTime += Time.deltaTime * moveSpeed;
            transform.position = Vector2.Lerp(startPosition, targetPosition, lerpTime);

            if (lerpTime >= 1f)
            {
                transform.position = targetPosition;
                isLerping = false;
                lerpTime = 0f;
                startPosition = targetPosition;
            }
        }

        // Set the direction parameter and isMoving state in the animator
        Vector2 direction = (targetPosition - startPosition).normalized;
        SetAnimatorDirection(direction);
    }

    private void SetAnimatorDirection(Vector2 direction)
    {
        if (animator != null)
        {
            if (direction == Vector2.up)
                animator.SetInteger("direction", 0);
            else if (direction == Vector2.down)
                animator.SetInteger("direction", 1);
            else if (direction == Vector2.left)
                animator.SetInteger("direction", 2);
            else if (direction == Vector2.right)
                animator.SetInteger("direction", 3);

            animator.SetBool("isMoving", isLerping);
            //Debug.Log(isLerping);
        }
    }

    public void TransitionToScaredState()
    {
        Debug.Log("Ghost In Scared State");
        currentState = GhostState.Scared;
        if (animator != null)
        {
            animator.SetTrigger("isScared");
            animator.SetBool("inState",true);
        }
        if (audioSource != null && scaredStateAudio != null)
        {
            audioSource.clip = scaredStateAudio;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void TransitionToRecoveringState()
    {
        currentState = GhostState.Recovering;
        if (animator != null)
        {
            animator.SetTrigger("isRecovering");
            animator.SetBool("inState", true);
        }
    }

    public void TransitionToWalkingState()
    {
        currentState = GhostState.Walking;
        if (animator != null)
        {
            animator.ResetTrigger("isScared");
            animator.ResetTrigger("isRecovering");
            animator.SetBool("inState", false);
        }
        if (audioSource != null && normalStateAudio != null)
        {
            audioSource.clip = normalStateAudio;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    public void TransitionToDeadState()
    {
        currentState = GhostState.Dead;
        if (animator != null)
        {
            animator.SetTrigger("isDead");
        }
        if (audioSource != null && deadStateAudio != null)
        {
            audioSource.clip = deadStateAudio;
            audioSource.loop = false; // Play once
            audioSource.Play();
        }
    }

    private void HandleScaredMovement()
    {
        // Use the behavior of Ghost 1 for Scared or Recovering states
        MoveRandomlyAwayFromPacStudent();
    }

    private void HandleDeadMovement()
    {
        // Move directly toward the spawn area, ignoring walls and PacStudent
        Vector2 spawnPosition = new Vector2(-4, 0); // Adjust to your spawn area center
        transform.position = Vector2.MoveTowards(transform.position, spawnPosition, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, spawnPosition) < 0.1f)
        {
            currentState = GhostState.Walking;
            TransitionToWalkingState();
        }
    }


    public void StopMovement()
    {
        isLerping = false;
        lerpTime = 0f;
    }


    private void HandleWalkingMovement()
    {
        // Choose the correct behavior based on the ghost type (e.g., number)
        if (gameObject.name.Contains("Ghost1"))
        {
            MoveRandomlyAwayFromPacStudent();
        }
        else if (gameObject.name.Contains("Ghost2"))
        {
            MoveRandomlyTowardPacStudent();
        }
        else if (gameObject.name.Contains("Ghost3"))
        {
            MoveRandomly();
        }
        else if (gameObject.name.Contains("Ghost4"))
        {
            MoveClockwise();
        }
    }

    private void MoveRandomlyAwayFromPacStudent()
    {
        Vector2 bestPosition = transform.position;
        float maxDistance = Vector2.Distance(transform.position, pacStudent.transform.position);

        foreach (Vector2 direction in GetValidDirections())
        {
            Vector2 newPos = (Vector2)transform.position + direction;
            float distance = Vector2.Distance(newPos, pacStudent.transform.position);
            if (distance >= maxDistance && newPos != lastPosition)
            {
                bestPosition = newPos;
                maxDistance = distance;
            }
        }

        StartMove(bestPosition);
    }

    private void MoveRandomlyTowardPacStudent()
    {
        Vector2 bestPosition = transform.position;
        float minDistance = Vector2.Distance(transform.position, pacStudent.transform.position);

        foreach (Vector2 direction in GetValidDirections())
        {
            Vector2 newPos = (Vector2)transform.position + direction;
            float distance = Vector2.Distance(newPos, pacStudent.transform.position);
            if (distance <= minDistance && newPos != lastPosition)
            {
                bestPosition = newPos;
                minDistance = distance;
            }
        }

        StartMove(bestPosition);
    }

    private void MoveRandomly()
    {
        Vector2[] validDirections = GetValidDirections();
        if (validDirections.Length > 0)
        {
            Vector2 chosenDirection = validDirections[Random.Range(0, validDirections.Length)];
            if (chosenDirection != lastPosition)
            {
                StartMove((Vector2)transform.position + chosenDirection);
            }
        }
    }

    private void MoveClockwise()
    {
        // Implement logic to move clockwise around the map
        // You will need to map the positions to ensure the ghost follows the map's wall structure
        Vector2[] clockwiseDirections = { Vector2.right, Vector2.down, Vector2.left, Vector2.up };
        foreach (Vector2 direction in clockwiseDirections)
        {
            if (CanMoveInDirection(direction) && (Vector2)transform.position + direction != lastPosition)
            {
                StartMove((Vector2)transform.position + direction);
                break;
            }
        }
    }

    private Vector2[] GetValidDirections()
    {
        // Check all four directions for valid moves
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        List<Vector2> validDirections = new List<Vector2>();

        foreach (Vector2 direction in directions)
        {
            if (CanMoveInDirection(direction))
            {
                validDirections.Add(direction);
            }
        }

        return validDirections.ToArray();
    }

    private bool CanMoveInDirection(Vector2 direction)
    {
        Vector2 nextPos = (Vector2)transform.position + direction;
        int gridX = Mathf.RoundToInt((nextPos.x + 10.5f) / 0.5f);
        int gridY = Mathf.RoundToInt((-nextPos.y + 7f) / 0.5f);

        if (gridX < 0 || gridX >= LevelGenerator.levelMap.GetLength(1) ||
            gridY < 0 || gridY >= LevelGenerator.levelMap.GetLength(0))
            return false;

        int tileType = LevelGenerator.levelMap[gridY, gridX];
        return tileType != 1 && tileType != 2 && tileType != 3 && tileType != 4 && tileType != 7; // Ensure it's a walkable tile
    }

    private void StartMove(Vector2 target)
    {
        lastPosition = transform.position;
        targetPosition = target;
        startPosition = transform.position;
        isLerping = true;
        lerpTime = 0f;
    }
}
