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

    private void Start()
    {
        startPosition = transform.position;
        targetPosition = transform.position;
        lastPosition = transform.position;
        animator = GetComponent<Animator>();
        pacStudent = FindObjectOfType<PacStudentController>();
        levelGenerator = FindObjectOfType<LevelGenerator>();
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
    public bool IsScared { get; private set; }

    public void TransitionToScaredState()
    {
        IsScared = true;
        // Trigger "Scared" animation and state logic
    }

    public void TransitionToRecoveringState()
    {
        IsScared = false;
        // Trigger "Recovering" animation and state logic
    }

    public void TransitionToWalkingState()
    {
        IsScared = false;
        // Trigger "Walking" animation and state logic
    }

    public void TransitionToDeadState()
    {
        // Trigger "Dead" animation and state logic
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
            // Reset to a non-dead state when at the spawn area
            currentState = GhostState.Walking;
            // Adjust background music if needed
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
