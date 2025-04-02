using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    private Transform targetPlatform;
    private PlatformSpawner platformSpawner;

    private static Vector3 cameraOffset = new Vector3(15f, 0f, -15f);
    private static Vector3 cameraEuler = new Vector3(17, -45, 0);
    private float fixedHeight = 12f;

    private bool isIntroDone = false;
    private float introDuration = 4f; // Thời gian quay intro lâu hơn chút
    private float elapsedTime = 0f;

    // Biến tĩnh để cho biết intro đang hoạt động
    public static bool IsIntroActive { get; private set; } = true;

    void Start()
    {
        platformSpawner = FindObjectOfType<PlatformSpawner>();
        targetPlatform = platformSpawner?.lastPlatform;
        if (PlayerPrefs.GetInt("IsReplay", 0) == 1)
        {
            isIntroDone = true; // Bỏ qua phần intro nếu đang replay
            IsIntroActive = false;
            PlayerPrefs.SetInt("IsReplay", 0); // Reset lại để lần sau vào game vẫn có intro
        }
    }

    void LateUpdate()
    {
        if (!isIntroDone)
        {
            PlayIntroAnimation();
        }
        else
        {
            IsIntroActive = false; // Khi intro hoàn tất, vô hiệu hóa flag này
            FollowPlayer();
        }
    }

    void PlayIntroAnimation()
    {
        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / introDuration);
        float smoothT = Mathf.SmoothStep(0, 1, t); // Làm mượt chuyển động

        // Xoay quanh nhân vật từ xa hơn
        float angle = Mathf.Lerp(0, 360, smoothT);
        Vector3 offset = Quaternion.Euler(0, angle, 0) * new Vector3(10f, 2f, -10f);
        Vector3 introPosition = player.position + offset;

        // Camera di chuyển mượt đến vị trí xoay
        transform.position = Vector3.Lerp(transform.position, introPosition, 2f * Time.deltaTime);
        transform.LookAt(player.position + Vector3.up * 1.5f);

        if (t >= 1)
        {
            isIntroDone = true;
        }
    }

    void FollowPlayer()
    {
        if (platformSpawner != null && platformSpawner.lastPlatform != null)
        {
            Transform nextPlatform = platformSpawner.lastPlatform;
            MovingPlatform movingPlatform = nextPlatform.GetComponent<MovingPlatform>();

            targetPlatform = (movingPlatform == null) ? nextPlatform : null;
        }

        if (player != null)
        {
            Vector3 targetPos;

            if (targetPlatform != null)
            {
                Vector3 middlePoint = (player.position + targetPlatform.position) / 2f;
                targetPos = middlePoint + cameraOffset;
            }
            else
            {
                targetPos = player.position + cameraOffset;
            }

            targetPos.y = fixedHeight;

            // Di chuyển mượt mà về góc chính
            transform.position = Vector3.Lerp(transform.position, targetPos, 2f * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(cameraEuler), 2f * Time.deltaTime);
        }
    }
}
