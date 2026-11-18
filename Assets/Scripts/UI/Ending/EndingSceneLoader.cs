using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingSceneLoader : MonoBehaviour
{
    public void OpenEndingCredits()
    {
        SceneManager.LoadScene("EndingCredits");
    }
}