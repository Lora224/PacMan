using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
public class StartSceneController : MonoBehaviour
{
    public void LoadLevel1()
    {
        SceneManager.LoadScene("PacAstarion");
    }

    public void LoadLevel2()
    {
      SceneManager.LoadScene("InfiniteMap"); 
    }


    public Text highScoreText; // Assign this in the Unity Inspector
    public Text timeText;
    private void Start()
    {
        // Load the high score and high score time from PlayerPrefs
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        float highScoreTime = PlayerPrefs.GetFloat("HighScoreTime", float.MaxValue);

        // Format the high score time
        string formattedTime = FormatTime(highScoreTime);

        // Update the high score text in the UI
        highScoreText.text = $"High Score: {highScore}";
        timeText.text = $"Time: {formattedTime}";
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 1000f) % 1000f);
        return string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }
}


