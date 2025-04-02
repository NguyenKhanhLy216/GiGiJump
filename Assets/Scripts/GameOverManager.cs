using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Firebase.Database;
using Firebase;

public class GameOverManager : MonoBehaviour
{
    public GameObject gameOverPanel;
    public TMP_Text scoreText;
    public TMP_Text highScoreText;
    public AudioSource buttonClickSound; // Thêm biến âm thanh

    private ScoreManager scoreManager;

    void Start()
    {
        gameOverPanel.SetActive(false);
        scoreManager = FindObjectOfType<ScoreManager>();
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        int currentScore = scoreManager.GetScore();
        scoreText.text = currentScore.ToString();

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
            UpdateHighScoreToFirebase(highScore);
        }
        highScoreText.text = highScore.ToString();
    }
    private async void UpdateHighScoreToFirebase(int highScore)
    {
        string userId = PlayerPrefs.GetString("UserId");
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("User ID not found.");
            return;
        }

        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            DatabaseReference databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            await databaseReference.Child("users").Child(userId).Child("score").SetValueAsync(highScore);
        }
        else
        {
            Debug.LogError("Could not resolve Firebase dependencies: " + dependencyStatus);
        }
    }
    public void ReplayGame()
    {
        PlayButtonSound();
        PlayerPrefs.SetInt("IsReplay", 1);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        PlayButtonSound();
        SceneManager.LoadScene("MenuScene");
    }

    private void PlayButtonSound()
    {
        if (buttonClickSound != null)
        {
            buttonClickSound.Play();
        }
    }
}
