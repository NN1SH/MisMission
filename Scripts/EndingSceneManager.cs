using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingSceneManager : MonoBehaviour
{
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private AudioClip endingMusic;
    [SerializeField] private Image fadeImage;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        if (endingMusic != null)
        {
            audioSource.clip = endingMusic;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("EndingMusic is not assigned in EndingSceneManager!");
        }

        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
        }
        else
        {
            Debug.LogError("FadeImage is not assigned in EndingSceneManager!");
        }
        // ตั้งค่า Main Menu Button
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(() => 
            {
                Debug.Log("Main Menu Button Clicked");
                StartCoroutine(FadeToScene("StartScene"));
            });
        }
        else
        {
            Debug.LogError("MainMenuButton is not assigned in EndingSceneManager!");
        }

        Time.timeScale = 1f;
    }

    private IEnumerator FadeToScene(string sceneName)
    {
        Debug.Log($"Starting FadeToScene: {sceneName}");
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / 0.5f;
                fadeImage.color = new Color(0, 0, 0, t);
                yield return null;
            }
        }
        try
        {
            SceneManager.LoadScene(sceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load {sceneName}: {e.Message}");
        }
    }
}