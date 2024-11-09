using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

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
    public int score = 0;
    public int lives = 3;
    public Text scoreText;
    public Text livesText;
    public Text gameOverText;
    public Text highScoreText;
    public Text timerText;
    public GameObject collisionEffectPrefab;
    public AudioClip powerPillSound;
    public AudioClip wallCollisionSound;

    public GameObject ghostTimerUI;
    public float scaredDuration = 10f;

    private float timer = 0f;
    private bool isGameRunning = false;
    private GhostController[] ghosts;
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        targetPosition = transform.position;
        startPosition = transform.position;
        UpdateScoreUI();
        UpdateLivesUI();
        ghostTimerUI.SetActive(false);
        ghosts = FindObjectsOfType<GhostController>();
        gameOverText.gameObject.SetActive(false);
        HUDController hUDController = new HUDController();
        StartCoroutine(hUDController.StartRoundCountdown());
    }

    private void Update()
    {
        if (isGameRunning)
        {
            timer += Time.deltaTime;
            UpdateTimerUI();
        }
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
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
        {
            HandleWallCollision(other);
        }
        else if (other.CompareTag("Teleporter"))
        {
            HandleTeleportation(other);
        }
        else if (other.CompareTag("Pellet"))
        {
            HandlePelletCollision(other);
        }
        else if (other.CompareTag("Cherry"))
        {
            HandleCherryCollision(other);
        }
        else if (other.CompareTag("PowerPill"))
        {
            HandlePowerPillCollision(other);
        }
        else if (other.CompareTag("Ghost"))
        {
            HandleGhostCollision(other);
        }
    }


    private void HandleWallCollision(Collider2D wall)
    {
        // Prevent movement and play collision effect and sound
        Instantiate(collisionEffectPrefab, transform.position, Quaternion.identity);
        audioSource.PlayOneShot(wallCollisionSound);
        // Implement logic to move PacStudent back to previous position
    }

    private void HandleTeleportation(Collider2D teleporter)
    {
        // Move PacStudent to the other side of the level
        Vector2 newPosition = GetOppositeTeleporterPosition(teleporter.transform.position);
        transform.position = newPosition;
    }

    private void HandlePelletCollision(Collider2D pellet)
    {
        score += 10;
        Destroy(pellet.gameObject);
        UpdateScoreUI();
    }

    private void HandleCherryCollision(Collider2D cherry)
    {
        score += 100;
        Destroy(cherry.gameObject);
        UpdateScoreUI();
    }

    private void HandlePowerPillCollision(Collider2D powerPill)
    {
        Destroy(powerPill.gameObject);
        audioSource.PlayOneShot(powerPillSound);
        StartCoroutine(StartGhostScaredState());
        score += 50;
        UpdateScoreUI();
    }

    private IEnumerator StartGhostScaredState()
    {
        ghostTimerUI.SetActive(true);
        float remainingTime = scaredDuration;

        // Change ghosts to "Scared" state and play corresponding music
        ChangeGhostStates("Scared");
        // Play scared background music

        while (remainingTime > 0)
        {
            ghostTimerUI.GetComponent<Text>().text = Mathf.CeilToInt(remainingTime).ToString();
            yield return new WaitForSeconds(1f);
            remainingTime--;

            if (remainingTime <= 3)
            {
                // Change ghosts to "Recovering" state for the last 3 seconds
                ChangeGhostStates("Recovering");
            }
        }

        ghostTimerUI.SetActive(false);
        ChangeGhostStates("Walking");
        // Return to normal background music
    }

    private void HandleGhostCollision(Collider2D ghost)
    {
        if (ghost.GetComponent<GhostController>().IsScared)
        {
            // Ghost dies, transition to Dead state, play audio, and add points
            score += 300;
            UpdateScoreUI();
            ghost.GetComponent<GhostController>().TransitionToDeadState();
            StartCoroutine(RespawnGhost(ghost.gameObject, 5f));
        }
        else
        {
            // PacStudent loses a life, play death effect, and respawn
            lives--;
            UpdateLivesUI();
            Instantiate(collisionEffectPrefab, transform.position, Quaternion.identity);
            if (lives <= 0)
            {
                GameOver();
            }
            else
            {
                RespawnPacStudent();
            }
        }
    }

    private IEnumerator RespawnGhost(GameObject ghost, float delay)
    {
        yield return new WaitForSeconds(delay);
        ghost.GetComponent<GhostController>().TransitionToWalkingState();
    }

    private void ChangeGhostStates(string state)
    {
        foreach (GhostController ghost in ghosts)
        {
            switch (state)
            {
                case "Scared":
                    ghost.TransitionToScaredState();
                    break;
                case "Recovering":
                    ghost.TransitionToRecoveringState();
                    break;
                case "Walking":
                    ghost.TransitionToWalkingState();
                    break;
            }
        }
    }

    private void GameOver()
    {
        isGameRunning = false;
        gameOverText.gameObject.SetActive(true);
        SaveHighScore();
        StartCoroutine(ReturnToStartScene());
    }

    private IEnumerator ReturnToStartScene()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("StartScene");
    }

    private void UpdateScoreUI()
    {
        scoreText.text = "Score: " + score;
    }

    private void UpdateLivesUI()
    {
        livesText.text = "Lives: " + lives;
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);
        int milliseconds = Mathf.FloorToInt((timer * 1000f) % 1000f);
        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    private Vector2 GetOppositeTeleporterPosition(Vector2 currentPos)
    {
        // Implement logic to return the position of the opposite teleporter
        return new Vector2(-currentPos.x, currentPos.y);
    }

    private void RespawnPacStudent()
    {
        transform.position = new Vector2(-9, 9);  // Top-left corner starting position
        isGameRunning = false;
        StartCoroutine(WaitForPlayerInput());
    }

    private IEnumerator WaitForPlayerInput()
    {
        yield return new WaitUntil(() => Input.anyKeyDown);
        isGameRunning = true;
    }

    private void SaveHighScore()
    {
        int savedScore = PlayerPrefs.GetInt("HighScore", 0);
        float savedTime = PlayerPrefs.GetFloat("HighScoreTime", float.MaxValue);

        if (score > savedScore || (score == savedScore && timer < savedTime))
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.SetFloat("HighScoreTime", timer);
        }
    }
}
