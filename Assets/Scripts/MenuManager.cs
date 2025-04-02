using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using System.Threading.Tasks;
using TMPro;

public class MenuManager : MonoBehaviour
{
    // Các panel và UI elements mới thêm
    public GameObject usernamePanel;
    public TMP_InputField usernameInput;
    public Button saveUsernameButton;
    public TMP_Text currentUsernameText;
    public GameObject changeUsernamePanel;
    public TMP_InputField newUsernameInput;
    public Button saveNewUsernameButton;
    public GameObject rankPanel;
    public Button closeChangeUsernameButton;
    // Các biến Firebase
    private DatabaseReference databaseReference;
    private string userId;
    // Thêm các biến mới
    public TMP_Text currentRankText;
    public TMP_Text currentScoreText;
    public GameObject gameplayPanel;
    public GameObject soundSettingPanel;
    public Slider volumeSlider;
    public Button muteButton;
    public Sprite muteSprite;
    public Sprite unmuteSprite;
    public AudioSource backgroundMusic;
    public AudioSource buttonClickSound;
    public GameObject loadingPanel;
    public Slider loadingBar;
    private float previousVolume;
    public TMP_Text usernameErrorText;
    public TMP_Text changeUsernameErrorText;
    public GameObject rankEntryPrefab; // Prefab bạn vừa tạo
    public Transform rankContentParent;
    public int maxEntries = 5;
    public Button closeRankButton;
    private void Start()
    {
        //PlayerPrefs.DeleteAll();
        currentUsernameText.text = PlayerPrefs.GetString("Username", "");
        // Khởi tạo Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
                userId = SystemInfo.deviceUniqueIdentifier;
                PlayerPrefs.SetString("UserId", userId);
                CheckFirstTimeLogin();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + task.Result);
            }
        });

        // Các thiết lập âm thanh
        float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
        AudioListener.volume = savedVolume;
        volumeSlider.value = savedVolume;
        previousVolume = savedVolume;
        UpdateMuteButtonSprite();

        if (backgroundMusic != null)
        {
            backgroundMusic.loop = true;
            backgroundMusic.volume = savedVolume;
            backgroundMusic.Play();
        }

        ShowGameplayPanel();
        if (!PlayerPrefs.HasKey("Username"))
        {
            usernamePanel.SetActive(true);
            // Ẩn thông báo lỗi khi mở panel
            usernameErrorText.gameObject.SetActive(false);
        }
    }

    private void CheckFirstTimeLogin()
    {
        if (!PlayerPrefs.HasKey("Username"))
        {
            usernamePanel.SetActive(true);
        }
    }

    // Xử lý username
    public async void OnSaveUsernameClicked()
    {
        // Ẩn thông báo lỗi cũ
        usernameErrorText.gameObject.SetActive(false);

        string username = usernameInput.text.Trim();
        if (string.IsNullOrEmpty(username))
        {
            usernameErrorText.text = "Vui lòng nhập tên người dùng!";
            usernameErrorText.gameObject.SetActive(true);
            return;
        }

        if (await IsUsernameAvailable(username))
        {
            PlayerPrefs.SetString("Username", username);
            CreateNewUser(username);
            usernamePanel.SetActive(false);
            Canvas.ForceUpdateCanvases();
            currentUsernameText.text = username;
        }
        else
        {
            usernameErrorText.text = "Tên người dùng đã tồn tại!";
            usernameErrorText.gameObject.SetActive(true);
        }
    }

    private async Task<bool> IsUsernameAvailable(string username)
    {
        var snapshot = await databaseReference.Child("users").OrderByChild("username").EqualTo(username).GetValueAsync();
        return !snapshot.Exists;
    }

    private void CreateNewUser(string username)
    {
        UserData newUser = new UserData(username, 0);
        string json = JsonUtility.ToJson(newUser);
        databaseReference.Child("users").Child(userId).SetRawJsonValueAsync(json);
    }

    // Đổi username
    public void OnChangeUsernameClicked()
    {
        soundSettingPanel.SetActive(false);
        changeUsernamePanel.SetActive(true);
        changeUsernameErrorText.gameObject.SetActive(false);
        currentUsernameText.text = PlayerPrefs.GetString("Username", "");
    }

    public async void OnSaveNewUsernameClicked()
    {
        // Ẩn thông báo lỗi cũ
        changeUsernameErrorText.gameObject.SetActive(false);

        string newUsername = newUsernameInput.text.Trim();
        if (string.IsNullOrEmpty(newUsername))
        {
            changeUsernameErrorText.text = "Vui lòng nhập tên mới!";
            changeUsernameErrorText.gameObject.SetActive(true);
            return;
        }

        if (await IsUsernameAvailable(newUsername))
        {
            PlayerPrefs.SetString("Username", newUsername);

            await databaseReference.Child("users").Child(userId).Child("username").SetValueAsync(newUsername);
            Canvas.ForceUpdateCanvases();

            changeUsernamePanel.SetActive(false);
            soundSettingPanel.SetActive(true);
            currentUsernameText.text = newUsername;
        }
        else
        {
            changeUsernameErrorText.text = "Tên người dùng đã tồn tại!";
            changeUsernameErrorText.gameObject.SetActive(true);
        }
    }
    public void OnCloseChangeUsernamePanelClicked()
    {
        PlayButtonSound();
        changeUsernamePanel.SetActive(false);
        soundSettingPanel.SetActive(true);

        newUsernameInput.text = "";
    }
    // Xử lý bảng xếp hạng
    public async void OnRankButtonClicked()
    {
        PlayButtonSound();
        rankPanel.SetActive(true);
        await LoadRankingData();
    }

    private async Task LoadRankingData()
    {
        // Xóa các dòng cũ
        foreach (Transform child in rankContentParent)
        {
            Destroy(child.gameObject);
        }

        var snapshot = await databaseReference.Child("users").OrderByChild("score").GetValueAsync();

        List<UserData> users = new List<UserData>();
        foreach (DataSnapshot child in snapshot.Children)
        {
            UserData user = JsonUtility.FromJson<UserData>(child.GetRawJsonValue());
            users.Add(user);
        }

        // Sắp xếp giảm dần
        users.Sort((a, b) => b.score.CompareTo(a.score));
        string currentUsername = PlayerPrefs.GetString("Username");
        int currentRank = -1;
        int currentScore = 0;

        for (int i = 0; i < users.Count; i++)
        {
            if (users[i].username == currentUsername)
            {
                currentRank = i + 1;
                currentScore = users[i].score;
                break;
            }
        }

        // Hiển thị thông tin
        if (currentRank > 0)
        {
            currentRankText.text = $"Rank: {currentRank}";
            currentScoreText.text = $"Highest Score: {currentScore}";
        }
        else
        {
            currentRankText.text = "Rank: Unranked";
            currentScoreText.text = "Score: 0";
        }
        // Hiển thị top N
        for (int i = 0; i < Mathf.Min(maxEntries, users.Count); i++)
        {
            // Tạo instance mới từ prefab
            GameObject entry = Instantiate(rankEntryPrefab, rankContentParent);

            // Lấy các thành phần Text
            TMP_Text rankText = entry.transform.Find("RankText").GetComponent<TMP_Text>();
            TMP_Text nameText = entry.transform.Find("NameText").GetComponent<TMP_Text>();
            TMP_Text scoreText = entry.transform.Find("ScoreText").GetComponent<TMP_Text>();

            // Gán giá trị
            rankText.text = (i + 1).ToString();
            nameText.text = users[i].username;
            scoreText.text = users[i].score.ToString();
        }

        // Thêm thông báo nếu không có dữ liệu
        if (users.Count == 0)
        {
            GameObject entry = Instantiate(rankEntryPrefab, rankContentParent);
            entry.transform.Find("RankText").GetComponent<TMP_Text>().text = "No players yet!";
        }
    }

    public void OnPlayButtonClicked()
    {
        PlayButtonSound();
        loadingPanel.SetActive(true); // Hiện giao diện loading
        StartCoroutine(LoadGameScene());
    }

    private IEnumerator LoadGameScene()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync("GameScene");
        operation.allowSceneActivation = false; // Chờ cho đến khi load xong

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f); // 0.9 là 100%
            if (loadingBar != null)
                loadingBar.value = progress; // Cập nhật thanh loading

            if (operation.progress >= 0.9f) // Khi tải xong
            {
                yield return new WaitForSeconds(1f); // Chờ 1 giây cho hiệu ứng loading
                operation.allowSceneActivation = true; // Chuyển sang GameScene
            }
            yield return null;
        }
    }
    public void OnCloseRankPanelClicked()
    {
        PlayButtonSound();
        rankPanel.SetActive(false);
    }
    public void OnGameplayButtonClicked()
    {
        PlayButtonSound();
        ShowGameplayPanel();
    }

    public void OnSoundSettingButtonClicked()
    {
        PlayButtonSound();
        ShowSoundSettingPanel();
    }

    public void OnVolumeChanged()
    {
        float vol = volumeSlider.value;
        // Cập nhật AudioListener và AudioSource
        AudioListener.volume = vol;
        if (backgroundMusic != null)
            backgroundMusic.volume = vol;
        PlayerPrefs.SetFloat("GameVolume", vol);
        if (vol > 0)
            previousVolume = vol;
        UpdateMuteButtonSprite();
    }

    public void OnMuteButtonClicked()
    {
        PlayButtonSound();
        if (AudioListener.volume > 0f)
        {
            // Tắt âm: lưu lại giá trị hiện tại và đặt âm lượng về 0
            previousVolume = volumeSlider.value;
            AudioListener.volume = 0f;
            volumeSlider.value = 0f;
            if (backgroundMusic != null)
                backgroundMusic.volume = 0f;
        }
        else
        {
            // Bật âm: khôi phục âm lượng trước đó
            AudioListener.volume = previousVolume;
            volumeSlider.value = previousVolume;
            if (backgroundMusic != null)
                backgroundMusic.volume = previousVolume;
        }
        PlayerPrefs.SetFloat("GameVolume", AudioListener.volume);
        UpdateMuteButtonSprite();
    }

    private void UpdateMuteButtonSprite()
    {
        if (muteButton != null)
        {
            Image muteImage = muteButton.GetComponent<Image>();
            if (AudioListener.volume <= 0f)
                muteImage.sprite = muteSprite;
            else
                muteImage.sprite = unmuteSprite;
        }
    }

    private void ShowGameplayPanel()
    {
        gameplayPanel.SetActive(true);
        soundSettingPanel.SetActive(false);
    }

    private void ShowSoundSettingPanel()
    {
        gameplayPanel.SetActive(false);
        soundSettingPanel.SetActive(true);
    }

    private void PlayButtonSound()
    {
        if (buttonClickSound != null)
            buttonClickSound.Play();
    }
}

[System.Serializable]
public class UserData
{
    public string username;
    public int score;

    public UserData(string username, int score)
    {
        this.username = username;
        this.score = score;
    }
}