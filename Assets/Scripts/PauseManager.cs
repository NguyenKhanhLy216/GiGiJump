using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    public GameObject pausePanel;
    public Button resumeButton;
    public Button homeButton;
    public Button muteButton;
    public Button pauseButton; // Nút pause trên giao diện
    public AudioSource buttonClickSound; // Thêm biến âm thanh

    // Thêm 2 sprite để thể hiện trạng thái âm bật và tắt
    public Sprite muteSprite;
    public Sprite unmuteSprite;

    private bool isPaused = false;
    private bool isMuted = false;
    public delegate void OnResumeGame();
    public static event OnResumeGame ResumeGameEvent;

    public bool IsPaused => isPaused;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (homeButton != null)
        {
            homeButton.onClick.AddListener(GoToHome);
        }

        if (muteButton != null)
        {
            muteButton.onClick.AddListener(ToggleMute);
            // Cập nhật sprite khởi tạo
            UpdateMuteButtonSprite();
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(TogglePause);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        PlayButtonSound();
        isPaused = !isPaused;
        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0f;
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        PlayButtonSound();
        isPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (ResumeGameEvent != null)
        {
            ResumeGameEvent.Invoke(); // Gọi sự kiện khi game tiếp tục
        }
    }

    void GoToHome()
    {
        PlayButtonSound();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuScene");
    }

    void ToggleMute()
    {
        PlayButtonSound();
        isMuted = !isMuted;
        AudioListener.volume = isMuted ? 0f : 1f;
        UpdateMuteButtonSprite();
    }

    void UpdateMuteButtonSprite()
    {
        if (muteButton != null)
        {
            Image muteButtonImage = muteButton.GetComponent<Image>();
            if (muteButtonImage != null)
            {
                muteButtonImage.sprite = isMuted ? muteSprite : unmuteSprite;
            }
        }
    }

    private void PlayButtonSound()
    {
        if (buttonClickSound != null)
        {
            buttonClickSound.Play();
        }
    }
}
