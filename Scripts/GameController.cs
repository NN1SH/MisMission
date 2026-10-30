using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class GameController : MonoBehaviour
{
    int progressAmount;
    public Slider progressBar;

    public GameObject player;
    public GameObject LoadCanvas;
    public List<GameObject> levels;
    private int currentLevelIndex = 0;

    public GameObject gameOverScreen;
    public TMP_Text completeText;
    private int completeCount;

    public GameObject pauseMenu;
    [Header("Pause Menu Settings")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private AudioClip bgMusic;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Image fadeImage;
    private AudioSource musicSource;

    private readonly Resolution[] supportedResolutions = new Resolution[]
    {
        new Resolution { width = 1280, height = 720 },
        new Resolution { width = 1920, height = 1080 }
    };
    private int currentResolutionIndex;

    public static bool IsPaused { get; private set; } = false;
    public static GameController Instance { get; private set; }

    public static event Action<Tilemap> OnResetGame;
    public static event Action OnReset;

    void Awake()
    {
        Instance = this;
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        if (bgMusic != null)
        {
            musicSource.clip = bgMusic;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("Background music clip is not assigned in GameController!");
        }

        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            List<string> options = new List<string>();
            int defaultIndex = 0;
            for (int i = 0; i < supportedResolutions.Length; i++)
            {
                string option = $"{supportedResolutions[i].width} x {supportedResolutions[i].height}";
                options.Add(option);
                if (supportedResolutions[i].width == Screen.currentResolution.width &&
                    supportedResolutions[i].height == Screen.currentResolution.height)
                {
                    defaultIndex = i;
                }
            }
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.value = defaultIndex;
            resolutionDropdown.RefreshShownValue();
        }
    }

    void Start()
    {
        progressAmount = 0;
        if (progressBar != null)
        {
            progressBar.minValue = 0;
            progressBar.maxValue = 100;
            progressBar.value = 0;
        }
        else
        {
            Debug.LogError("Progress Bar is not assigned in GameController!");
        }

        Star.OnStarCollect += IncreaseProgressAmount;
        HoldToLoadLevel.OnHoldComplete += LoadNextLevel;
        PlayerHealth.OnplayerDied += GameOverScreen;
        Door.OnDoorEntered += LoadEndingScene;

        LoadCanvas.SetActive(false);
        gameOverScreen.SetActive(false);
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }
        else
        {
            Debug.LogError("PauseMenu is not assigned in GameController!");
        }

        LoadSettings();
        StartCoroutine(LoadLevelWithDelay(0, false));
    }

    void OnDestroy()
    {
        Star.OnStarCollect -= IncreaseProgressAmount;
        HoldToLoadLevel.OnHoldComplete -= LoadNextLevel;
        PlayerHealth.OnplayerDied -= GameOverScreen;
        Door.OnDoorEntered -= LoadEndingScene;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && !gameOverScreen.activeInHierarchy)
        {
            if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void Pause(InputAction.CallbackContext context)
    {
        if (context.performed && !gameOverScreen.activeInHierarchy)
        {
            if (IsPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    void LoadSettings()
    {
        if (sfxSlider != null)
        {
            float savedSfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.value = savedSfxVolume;
            SoundEffectManager.SetVolume(savedSfxVolume);

            sfxSlider.onValueChanged.AddListener((value) =>
            {
                SoundEffectManager.SetVolume(value);
                PlayerPrefs.SetFloat("SFXVolume", value);
                PlayerPrefs.Save();
            });
        }

        if (musicSlider != null)
        {
            float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.value = savedMusicVolume;
            musicSource.volume = savedMusicVolume;

            musicSlider.onValueChanged.AddListener((value) =>
            {
                musicSource.volume = value;
                PlayerPrefs.SetFloat("MusicVolume", value);
                PlayerPrefs.Save();
            });
        }

        if (resolutionDropdown != null)
        {
            currentResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", 0);
            if (currentResolutionIndex >= supportedResolutions.Length)
                currentResolutionIndex = 0;
            resolutionDropdown.value = currentResolutionIndex;
            Resolution resolution = supportedResolutions[currentResolutionIndex];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

            resolutionDropdown.onValueChanged.AddListener((value) =>
            {
                currentResolutionIndex = value;
                Resolution resolution = supportedResolutions[value];
                Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
                PlayerPrefs.SetInt("ResolutionIndex", value);
                PlayerPrefs.Save();
            });
        }

        if (fullscreenToggle != null)
        {
            bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
            fullscreenToggle.isOn = isFullscreen;
            Screen.fullScreen = isFullscreen;

            fullscreenToggle.onValueChanged.AddListener((value) =>
            {
                Screen.fullScreen = value;
                PlayerPrefs.SetInt("Fullscreen", value ? 1 : 0);
                PlayerPrefs.Save();
            });
        }
    }

    void GameOverScreen()
    {
        gameOverScreen.SetActive(true);
        completeText.text = $"You collected {completeCount} Star";
        if (completeCount != 1) completeText.text += "s";
        Time.timeScale = 0f;
        IsPaused = true;
        musicSource.Pause();
    }

    void LoadEndingScene()
    {
        PlayerPrefs.SetInt("CompleteCount", completeCount);
        PlayerPrefs.Save();
        Time.timeScale = 0f;
        IsPaused = true;
        musicSource.Pause();
        SoundEffectManager.Play("GameComplete");
        Debug.Log("Loading EndingScene with Fade...");
        StartCoroutine(FadeToScene("Ending"));
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
                t += Time.unscaledDeltaTime / 1f;
                fadeImage.color = new Color(0, 0, 0, t);
                yield return null;
            }
        }
        Time.timeScale = 1f; // ÃÕà«çµ Time.timeScale ¡èÍ¹âËÅ´ Scene
        try
        {
            SceneManager.LoadScene(sceneName);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load {sceneName}: {e.Message}");
        }
    }

    public void ResetGame()
    {
        gameOverScreen.SetActive(false);
        completeCount = 0;
        StartCoroutine(LoadLevelWithDelay(0, false));
        Time.timeScale = 1f;
        SoundEffectManager.Play("Button");
        IsPaused = false;
        if (musicSource != null && bgMusic != null)
        {
            musicSource.Stop();
            musicSource.clip = bgMusic;
            musicSource.Play();
        }
    }

    public void IncreaseProgressAmount(int amount)
    {
        progressAmount += amount;
        if (progressAmount > 100) progressAmount = 100;

        if (progressBar != null)
            progressBar.value = progressAmount;

        if (progressAmount >= 100)
        {
            LoadCanvas.SetActive(true);
        }
    }

    private IEnumerator LoadLevelWithDelay(int level, bool wantCompleteincrease)
    {
        if (levels == null || levels.Count == 0)
        {
            Debug.LogError("Levels list is empty or null in GameController!");
            yield break;
        }

        LoadCanvas.SetActive(false);
        if (currentLevelIndex >= 0 && currentLevelIndex < levels.Count)
        {
            levels[currentLevelIndex].SetActive(false);
        }
        if (level >= 0 && level < levels.Count)
        {
            levels[level].SetActive(true);
        }
        else
        {
            Debug.LogError($"Invalid level index: {level}. Levels count: {levels.Count}");
            yield break;
        }

        player.transform.position = Vector3.zero;
        currentLevelIndex = level;
        progressAmount = 0;
        if (progressBar != null)
            progressBar.value = 0;

        if (wantCompleteincrease) completeCount++;

        yield return new WaitForSeconds(0.1f);
        Tilemap tilemap = null;
        if (levels[level] != null)
        {
            tilemap = levels[level].GetComponentInChildren<Tilemap>();
            if (tilemap == null || !tilemap.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"Tilemap not found or inactive in level {level}");
            }
        }
        OnResetGame?.Invoke(tilemap);
        OnReset?.Invoke();
        Debug.Log($"Loaded level {level}, Invoking OnResetGame with Tilemap: {(tilemap != null ? tilemap.name : "null")} and OnReset");
    }

    void LoadNextLevel()
    {
        int nextLevelIndex = (currentLevelIndex == levels.Count - 1) ? 0 : currentLevelIndex + 1;
        StartCoroutine(LoadLevelWithDelay(nextLevelIndex, true));
    }

    public void PauseGame()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
            SoundEffectManager.Play("Button");
            IsPaused = true;
            musicSource.Pause();
            Debug.Log("Game Paused");
        }
        else
        {
            Debug.LogError("Cannot Pause: PauseMenu is not assigned!");
        }
    }

    public void ResumeGame()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1f;
            SoundEffectManager.Play("Button");
            IsPaused = false;
            if (musicSource != null && musicSource.clip != null)
            {
                musicSource.Play();
            }
            Debug.Log("Game Resumed");
        }
        else
        {
            Debug.LogError("Cannot Resume: PauseMenu is not assigned!");
        }
    }

    public void QuitToMainMenu()
    {
        StartCoroutine(FadeToScene("StartScene"));
    }
}