using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryIntroController : MonoBehaviour
{
    [SerializeField] private GameObject[] slides;

    private int currentSlide;
    private bool loadingLevel;

    private void Start()
    {
        if (slides != null && slides.Length > 0)
        {
            ShowSlide();
        }
    }

    public void NextSlide()
    {
        if (loadingLevel || slides == null || slides.Length == 0)
        {
            return;
        }

        if (currentSlide == slides.Length - 1)
        {
            loadingLevel = true;
            SceneManager.LoadScene("Sloth");
            return;
        }

        currentSlide++;
        ShowSlide();
    }

    private void ShowSlide()
    {
        for (int i = 0; i < slides.Length; i++)
        {
            slides[i].SetActive(i == currentSlide);
        }
    }
}