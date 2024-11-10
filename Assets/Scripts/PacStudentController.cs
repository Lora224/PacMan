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

    public int score = 0;
    public int lives = 3;
    public Text scoreText;
    public Text livesText;
    public Text gameOverText;
    public Text highScoreText;
    public Text timerText;
    public GameObject collisionEffectPrefab;
    public AudioClip powerPelletSound;
    public AudioClip wallCollisionSound;
    public AudioClip deathAudioClip;

    public GameObject ghostTimerUI;
    public float scaredDuration = 10f;

    private bool countdownActive = true; // Added boolean to track countdown state
    private float timer = 0f;
    private bool isGameRunning = false;
    private GhostController[] ghosts;
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
        UpdateScoreUI();
        UpdateLivesUI();
        ghostTimerUI.SetActive(false);
        ghosts = FindObjectsOfType<GhostController>();
        gameOverText.gameObject.SetActive(false);
        int savedScore = PlayerPrefs.GetInt("HighScore",0);
        highScoreText.text = "Highest: " + savedScore;
        Debug.Log("Saved Highest Score"+ savedScore);
        HUDController hUDController = FindObjectOfType<HUDController>(); // Ensure HUDController is correctly found
        if (hUDController != null)
        {
            StartCoroutine(hUDController.StartRoundCountdown(() =>
            {
                countdownActive = false; // Disable countdownActive after countdown ends
                StartCoroutine(WaitForPlayerInput()); // Wait for player input to start the game
            }));
        }
        else
        {
            Debug.LogError("HUDController not found!");
        }
    }

    private void Update()
    {
        if (countdownActive) return;
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
       // Debug.Log(nextPos);
        //Debug.Log(gridX);
       // Debug.Log(gridY);
        // Check if position is within grid bounds
        if (gridX < 0 || gridX >= LevelGenerator.levelMap.GetLength(1) ||
            gridY < 0 || gridY >= LevelGenerator.levelMap.GetLength(0))
            return false;

        // Check if next position is walkable (not a wall)
        int tileType = LevelGenerator.levelMap[gridY, gridX];
        //Debug.Log(tileType);
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
            Vector2 movementDirection = (targetPosition - startPosition).normalized;

            // Get the position of the wall
            Vector2 wallPosition = other.transform.position;
            Vector2 currentPosition = transform.position;

            // Check if the wall is in the direction PacStudent is moving
            Vector2 directionToWall = (wallPosition - currentPosition).normalized;
            // Allow a small threshold for detection
            float detectionThreshold = 0.02f;

            // Only trigger collision if the wall is directly in front of PacStudent
            if (Vector2.Dot(movementDirection, directionToWall) > 1 - detectionThreshold)
            {

                HandleWallCollision(other);
            }
        }
        else if (other.CompareTag("Teleporter"))
        {
            if (other.CompareTag("Teleporter"))
            {
                Debug.Log("Teleportation triggered by: " + other.name);
                HandleTeleportation(other);
            }
            else
            {
                Debug.Log("Collision detected with non-teleporter object: " + other.name);
            }
          //  HandleTeleportation(other);

        }
        else if (other.CompareTag("Pellet"))
        {
            HandlePelletCollision(other);
        }
        else if (other.CompareTag("Cherry"))
        {
            HandleCherryCollision(other);
        }
        else if (other.CompareTag("PowerPellet"))
        {
            HandlePowerPelletCollision(other);
        }
        else if (other.CompareTag("Ghost"))
        {
            HandleGhostCollision(other);
        }
    }
    private IEnumerator LerpBackToStartPosition(Vector2 startPos, Vector2 endPos, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            transform.position = Vector2.Lerp(endPos, startPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
        // Ensure final position is set to startPos at the end of the lerp
        transform.position = startPos;

        // Stop the coroutine after the lerp has completed
        StopCoroutine(LerpBackToStartPosition(startPos, endPos, duration));
        isLerping = false; // Reset lerping state
        lerpTime = 0f; // Reset lerp time if necessary
    }


    private float wallCollisionCooldown = 3f; // Time in seconds before allowing another collision with the same wall
    private float lastCollisionTime = -1f;    // Time of the last collision
    private Collider2D lastWall = null;       // Reference to the last wall collided with

    private void HandleWallCollision(Collider2D wall)
    {
        // Check if the wall is the same as the last wall collided with and if the cooldown period has passed
        if (wall == lastWall && Time.time - lastCollisionTime < wallCollisionCooldown)
        {
            // Ignore this collision as it is within the cooldown period
            return;
        }

        // Update the last collision time and wall reference
        lastCollisionTime = Time.time;
        lastWall = wall;
        // Debug.Log(Time.time - lastCollisionTime);
        // Calculate the direction PacStudent is moving in
        Debug.Log(wall);

        // Only trigger collision if the wall is directly in front of PacStudent

        Vector2 currentPosition = transform.position;
        Debug.Log("Collides with wall in front");

            // Create a small particle effect at the point of collision and destroy it after one frame
            if (collisionEffectPrefab != null)
            {
                GameObject effect = Instantiate(collisionEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 0.5f); // Destroy after a short delay (0.1 seconds)
            }

            // Play the wall collision sound effect (only once per collision)
            if (audioSource && wallCollisionSound)
            {
                audioSource.PlayOneShot(wallCollisionSound);
            }

            // Stop current movement and start lerp back to the previous valid position
            isLerping = false; // Stop current forward lerping
            lerpTime = 0f; // Reset lerp time

            StartCoroutine(LerpBackToStartPosition(startPosition, currentPosition, 0.2f)); 
        
    }



    private void HandleTeleportation(Collider2D teleporter)
    {

        isLerping = false;  // Stop any current movement
        lerpTime = 0f;      // Reset lerp time if necessary

        // Move PacStudent to the other side of the level
        Vector2 newPosition = GetOppositeTeleporterPosition(teleporter.transform.position);
        transform.position = newPosition;

        //keep moving after teleported
        Debug.Log("Teleporting from: " + transform.position + " to: " + newPosition);

        Debug.Log("Teleported to: " + newPosition);
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

    private void HandlePowerPelletCollision(Collider2D powerPill)
    {
        Destroy(powerPill.gameObject);
        audioSource.PlayOneShot(powerPelletSound);
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
    private bool isRespawning = false; // Flag to indicate if PacStudent is in the process of respawning

    private void HandleGhostCollision(Collider2D ghostCollider)
    {
        GhostController ghost = ghostCollider.GetComponent<GhostController>();

        if (ghost != null && !isRespawning)
        {
            if (ghost.currentState == GhostController.GhostState.Scared)
            {
                // Ghost dies, transition to Dead state, play audio, and add points
                score += 300;
                UpdateScoreUI();
                ghost.TransitionToDeadState();
                StartCoroutine(RespawnGhost(ghost.gameObject, 5f));
            }
            else if (ghost.currentState == GhostController.GhostState.Dead)
            {
                // Do nothing; PacStudent should pass through dead ghosts
            }
            else if (ghost.currentState == GhostController.GhostState.Walking)
            {
                // PacStudent loses a life, play death effect, and respawn
                lives--;
                UpdateLivesUI();

                StopMovement();
                isLerping = false;
                lerpTime = 0f;
                // Set the respawn flag to prevent further life loss during respawn
                isRespawning = true;

                // Trigger death animation
                if (animator != null)
                {
                    animator.SetTrigger("Die"); // Ensure "Die" is a trigger in the Animator
                }

                // Play death audio
                if (audioSource != null && deathAudioClip != null)
                {
                    audioSource.PlayOneShot(deathAudioClip);
                }

                if (lives <= 0)
                {
                    GameOver();
                }
                else
                {
                    Debug.Log("Respawn Student");
                    StartCoroutine(HandleRespawnWithDelay(2f)); // Delay to allow animation and audio to play
                }
            }
        }
    }

    private IEnumerator HandleRespawnWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        RespawnPacStudent();
    }

    private void RespawnPacStudent()
    {
        transform.position = new Vector2(-10, 6.5f); // Top-left corner starting position
        isGameRunning = false;
        isRespawning = false; // Reset respawn flag to allow future life loss after respawn
        StartCoroutine(WaitForPlayerInput()); // Wait for player input to resume game
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
        // Stop the game timer and prevent further movement
        isGameRunning = false;

        // Stop all player movement
        isLerping = false;
        lerpTime = 0f;

        // Stop ghost movement
        foreach (GhostController ghost in ghosts)
        {
            ghost.StopMovement();
        }

        // Display "Game Over" text
        gameOverText.gameObject.SetActive(true);

        // Save high score if conditions are met
        Debug.Log("Updating Highest Score-");
        SaveHighScore();

        // Start coroutine to wait and then return to start scene
        StartCoroutine(ReturnToStartScene());
    }

    private void SaveHighScore()
    {

        int savedScore = PlayerPrefs.GetInt("HighScore", 0);
        float savedTime = PlayerPrefs.GetFloat("HighScoreTime", float.MaxValue);
        Debug.Log(savedScore);
        // Save if the current score is higher or if the score is the same but the time is lower
        if (score > savedScore || (score == savedScore && timer < savedTime))
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.SetFloat("HighScoreTime", timer);
            PlayerPrefs.Save(); // Ensure changes are saved
            highScoreText.text = "Highest: " + score; // Update the high score text
            Debug.Log("Highest Score updated " + score);
        }
    }

    private IEnumerator ReturnToStartScene()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("StartScene");
    }

    private void UpdateScoreUI()
    {
        // Save high score and time

        scoreText.text = ""+score;

    }

    private void UpdateLivesUI()
    {
        livesText.text = "x " + lives;
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);
        int milliseconds = Mathf.FloorToInt((timer * 1000f) % 1000f);
        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
       // Debug.Log(timerText.text);
    }

    private Vector2 GetOppositeTeleporterPosition(Vector2 currentPos)
    {

        float threshold = 0.01f;
        //Debug.Log(currentPos);
        // Check if the current position is near the left teleporter
        if (Mathf.Abs(currentPos.x - (-11f)) < threshold && Mathf.Abs(currentPos.y)== 0) 
        {
            // Move to the right teleporter
            return new Vector2(1f, currentPos.y);
        }
        // Check if the current position is near the right teleporter
        else if (Mathf.Abs(currentPos.x - 2.5f) < threshold && Mathf.Abs(currentPos.y) ==0) 
        {
            // Move to the left teleporter
            return new Vector2(-9f, currentPos.y); 
        }

        // If not a teleporter, return the current position
        return currentPos;
    }




    private IEnumerator WaitForPlayerInput()
    {
        yield return new WaitUntil(() => Input.anyKeyDown);
        isGameRunning = true;
        if (animator)
            animator.SetBool("IsMoving", true);
    }


}
