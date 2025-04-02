using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource bgmSource;      // Nhạc nền của GameScene
    public AudioSource sfxSource;      // Hiệu ứng âm thanh của GameScene

    [Header("Background Music")]
    public AudioClip backgroundMusic;

    [Header("SFX Clips")]
    public AudioClip chargeSound;
    public AudioClip jumpSound;
    public AudioClip landingSound;
    public AudioClip collectItemSound;
    public AudioClip loseLifeSound;
    public AudioClip cloudHitSound;
    public AudioClip fallSound;
    public AudioClip gameOverSound;

    void Start()
    {
        // Lấy lại giá trị âm lượng đã lưu (mặc định là 1)
        float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);
        AudioListener.volume = savedVolume;

        // Cập nhật volume của bgmSource
        if (bgmSource != null)
        {
            bgmSource.volume = savedVolume;
            bgmSource.loop = true;
            if (backgroundMusic != null)
            {
                bgmSource.clip = backgroundMusic;
                bgmSource.Play();
            }
        }
    }

    // Nếu bạn muốn đảm bảo volume luôn cập nhật, bạn có thể thêm Update() sau:
    void Update()
    {
        float currentVol = PlayerPrefs.GetFloat("GameVolume", 1f);
        if (bgmSource != null)
            bgmSource.volume = currentVol;
        if (sfxSource != null)
            sfxSource.volume = currentVol;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            // sfxSource.volume được cập nhật từ AudioListener.volume (hoặc PlayerPrefs)
            sfxSource.PlayOneShot(clip);
        }
    }
}
