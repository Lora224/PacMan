using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneController : MonoBehaviour
{
    public void LoadLevel1()
    {
        SceneManager.LoadScene("PacAstarion");
    }

    public void LoadLevel2()
    {
      SceneManager.LoadScene("DesignIterationScene"); 
    }
}
