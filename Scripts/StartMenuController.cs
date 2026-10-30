using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    [Header("Menu UI")]
    [SerializeField] private GameObject settingsMenu; // Settings Panel หรือ Canvas
    [SerializeField] private GameObject mainMenu; // Main Menu Panel
    [SerializeField] private GameObject TutorialMenu; // Tutorial Panel หรือ Canvas

    [Header("Menu Settings")]
    [SerializeField] private Slider sfxSlider; // Slider ปรับเสียง SFX
    [SerializeField] private Slider musicSlider; // Slider ปรับเสียง Music
    [SerializeField] private AudioClip bgMusic; // เพลงพื้นหลัง
    [SerializeField] private TMP_Dropdown resolutionDropdown; // Dropdown สำหรับเลือกความละเอียด
    [SerializeField] private Toggle fullscreenToggle; // Toggle สำหรับโหมด Fullscreen
    private AudioSource musicSource; // AudioSource สำหรับเพลง

    // รายการความละเอียดที่เลือกไว้สำหรับ Dropdown
    private readonly Resolution[] supportedResolutions = new Resolution[]
    {
        new Resolution { width = 1280, height = 720 },
        new Resolution { width = 1920, height = 1080 }
    };
    private int currentResolutionIndex;

    void Awake()
    {
        // สร้างหรือรับ AudioSource สำหรับเพลง
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        if (bgMusic != null)
        {
            musicSource.clip = bgMusic;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("Background music clip is not assigned in StartMenuController!");
        }

        // ตั้งค่า Dropdown สำหรับความละเอียด
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
        // ปิด Settings Menu และ Overlay ตอนเริ่ม
        if (settingsMenu != null) settingsMenu.SetActive(false);
        if (TutorialMenu != null) TutorialMenu.SetActive(false);
        if (mainMenu != null) mainMenu.SetActive(true);

        // โหลดและตั้งค่า Settings
        LoadSettings();
    }

    void LoadSettings()
    {
        // โหลด SFX Volume
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

        // โหลด Music Volume
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

        // โหลด Resolution
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

        // โหลด Fullscreen
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

    public void OpenSettings()
    {
        if (settingsMenu != null) settingsMenu.SetActive(true);
        if (mainMenu != null) mainMenu.SetActive(false);
        SoundEffectManager.Play("Button");
    }

    public void CloseSettings()
    {
        if (settingsMenu != null) settingsMenu.SetActive(false);
        if (mainMenu != null) mainMenu.SetActive(true);
        SoundEffectManager.Play("Button");
    }

    public void OpenTutorial()
    {
        if (TutorialMenu != null) TutorialMenu.SetActive(true);
        if (mainMenu != null) mainMenu.SetActive(false);
        SoundEffectManager.Play("Button");
    }

    public void CloseTutorial()
    {
        if (TutorialMenu != null) TutorialMenu.SetActive(false);
        if (mainMenu != null) mainMenu.SetActive(true);
        SoundEffectManager.Play("Button");
    }

    public void StartGame()
    {
        if (musicSource != null) musicSource.Stop(); // หยุดเพลงเมื่อเริ่มเกม
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        if (musicSource != null) musicSource.Stop(); // หยุดเพลงเมื่อออกเกม
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}