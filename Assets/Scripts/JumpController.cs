using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class JumpController : MonoBehaviour
{
    public int maxLives = 3;
    public int currentLives;
    private bool isGameOver = false; // Biến kiểm soát trạng thái game
    public float minJumpForce = 5f;
    public float maxJumpForce = 15f;
    public float chargeTime = 5f;
    public AnimationCurve jumpCurve;
    public float jumpDuration = 1f;
    public float scaleRecoverTime = 0.5f;
    public Slider chargeBar;
    private float jumpPower;
    private bool isCharging;
    private float chargeStartTime;
    private Rigidbody rb;
    private Vector3 nextJumpDirection = Vector3.forward;
    private bool isGrounded = true;
    private bool hasSpawnedNextPlatform = false;
    private Vector3 originalScale;
    private Vector3 lastSafePosition;
    private Transform lastSafePlatform;
    private PlatformSpawner platformSpawner;
    private ScoreManager scoreManager;
    private Animator anim;
    public ItemManager itemManager;
    public GameObject protectionPrefab;
    private Renderer[] renderers;
    private Color[] originalColors;
    public int currentShields = 0;
    public int maxShields = 5;
    public ParticleSystem landingEffect; // Hiệu ứng khi tiếp đất
    // Tham chiếu đến AudioManager được gán từ Inspector
    public AudioManager audioManager;
    private bool inCloud = false;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        originalScale = transform.localScale;
        platformSpawner = FindObjectOfType<PlatformSpawner>();
        scoreManager = FindObjectOfType<ScoreManager>();
        itemManager = FindObjectOfType<ItemManager>();

        if (chargeBar != null)
        {
            chargeBar.minValue = 0f;
            chargeBar.maxValue = 1f;
            chargeBar.value = 0f;
        }
        rb.freezeRotation = true;

        currentLives = maxLives;
        itemManager.UpdateLives(currentLives);
        lastSafePosition = transform.position;

        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalColors[i] = renderers[i].material.color;
        }
        PauseManager.ResumeGameEvent += OnGameResumed;
    }

    void OnGameResumed()
    {
        isCharging = false; // Đảm bảo có thể tích lực lại ngay lập tức
        anim.SetBool("isCharging", false);
    }

    void OnDestroy()
    {
        PauseManager.ResumeGameEvent -= OnGameResumed;
    }

    void Update()
    {
        // Nếu game đang tạm dừng, game over hoặc intro thì không xử lý gì
        if (isGameOver || PauseManager.Instance.IsPaused || CameraFollow.IsIntroActive)
            return;

        if (Input.GetMouseButtonDown(0) && isGrounded)
        {
            isCharging = true;
            chargeStartTime = Time.time;
            anim.SetBool("isCharging", true);
            // Phát âm thanh khi bắt đầu tích lực
            if (audioManager != null)
                audioManager.PlaySFX(audioManager.chargeSound);
        }

        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            // Phát âm thanh khi vừa thả nhảy
            if (audioManager != null)
                audioManager.PlaySFX(audioManager.jumpSound);
            Jump();
            isCharging = false;
            hasSpawnedNextPlatform = false;
            anim.SetBool("isCharging", false);
            anim.SetBool("isJumping", true);
            if (chargeBar != null)
            {
                chargeBar.value = 0f;
            }
        }

        if (isCharging)
        {
            float pressTime = Mathf.Clamp(Time.time - chargeStartTime, 0, chargeTime);
            float pressRatio = Mathf.Lerp(1f, 0.6f, pressTime / chargeTime);
            transform.localScale = new Vector3(originalScale.x, originalScale.y * pressRatio, originalScale.z);
            if (chargeBar != null)
            {
                chargeBar.value = pressTime / chargeTime; // Tỷ lệ lực nhấn giữ
            }
        }

        if (transform.position.y < -5f)
        {
            // Phát âm thanh khi rơi xuống
            if (audioManager != null)
                audioManager.PlaySFX(audioManager.fallSound);
            HandleFallOff();
        }

        if (isGrounded)
        {
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }
    }

    public void SetNextJumpDirection(Vector3 direction)
    {
        nextJumpDirection = direction.normalized;
        transform.rotation = Quaternion.LookRotation(nextJumpDirection);
    }

    void Jump()
    {
        if (!isGrounded)
            return;

        isGrounded = false;
        float chargeDuration = Mathf.Clamp(Time.time - chargeStartTime, 0, chargeTime);
        jumpPower = Mathf.Lerp(minJumpForce, maxJumpForce, chargeDuration / chargeTime);

        Vector3 jumpVelocity = nextJumpDirection * jumpPower + Vector3.up * jumpPower;
        StartCoroutine(JumpAnimation(jumpVelocity));
        StartCoroutine(ScaleRecover());
    }

    IEnumerator JumpAnimation(Vector3 velocity)
    {
        float time = 0f;
        Vector3 startPos = transform.position;

        while (time <= jumpDuration)
        {
            float t = time / jumpDuration;
            transform.position = startPos + velocity * t;
            transform.position = new Vector3(
                transform.position.x,
                startPos.y + jumpCurve.Evaluate(t) * jumpPower,
                transform.position.z
            );
            time += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator ScaleRecover()
    {
        float time = 0f;
        Vector3 startScale = transform.localScale;

        while (time <= scaleRecoverTime)
        {
            transform.localScale = Vector3.Lerp(startScale, originalScale, time / scaleRecoverTime);
            time += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform") && !isGrounded)
        {
            bool validContact = false;
            foreach (ContactPoint cp in collision.contacts)
            {
                // Nếu contact normal có hướng lên (có giá trị y cao) => va chạm bên dưới
                if (cp.normal.y > 0.7f)
                {
                    validContact = true;
                    break;
                }
            }
            if (!validContact)
            {
                HandleFallOff();
                return;
            }

            isGrounded = true;
            anim.SetBool("isJumping", false);
            anim.SetBool("isCharging", false);

            lastSafePosition = transform.position;
            lastSafePlatform = collision.transform;

            // Nếu thu thập được heart, phát âm thanh item
            HeartItem heart = collision.gameObject.GetComponentInChildren<HeartItem>();
            if (heart != null && currentLives < maxLives)
            {
                currentLives++;
                itemManager.UpdateLives(currentLives);
                if (audioManager != null)
                    audioManager.PlaySFX(audioManager.collectItemSound);
                Destroy(heart.gameObject);
            }

            // Nếu thu thập được shield, phát âm thanh item
            ShieldItem shield = collision.gameObject.GetComponentInChildren<ShieldItem>();
            if (shield != null && currentShields < maxShields)
            {
                currentShields++;
                itemManager.UpdateShields(currentShields);
                if (audioManager != null)
                    audioManager.PlaySFX(audioManager.collectItemSound);
                Destroy(shield.gameObject);
            }

            if (!hasSpawnedNextPlatform)
            {
                hasSpawnedNextPlatform = true;
                platformSpawner.SpawnNextPlatform();
            }

            MovingPlatform movingPlatform = collision.gameObject.GetComponent<MovingPlatform>();
            if (movingPlatform != null)
            {
                movingPlatform.StopMoving();
            }
            if (landingEffect != null)
            {
                landingEffect.Play();
            }
            // Phát âm thanh khi tiếp đất
            if (audioManager != null)
                audioManager.PlaySFX(audioManager.landingSound);

            Vector3 platformCenter = collision.collider.bounds.center;
            float distanceX = Mathf.Abs(transform.position.x - platformCenter.x);
            float distanceZ = Mathf.Abs(transform.position.z - platformCenter.z);
            float maxDistance = collision.collider.bounds.extents.x;
            float distanceRatio = Mathf.Max(distanceX, distanceZ) / maxDistance;
            int points = distanceRatio < 0.15f ? 5 : (distanceRatio < 0.6f ? 3 : 1);
            scoreManager.AddScore(points);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Cloud"))
        {
            // Nếu lần va chạm này chưa gây damage
            if (!inCloud)
            {
                if (currentShields > 0)
                {
                    // Nếu có khiên, giảm khiên và hiển thị hiệu ứng bảo vệ
                    currentShields--;
                    itemManager.UpdateShields(currentShields);
                    if (protectionPrefab != null)
                    {
                        GameObject protection = Instantiate(protectionPrefab, transform.position, Quaternion.identity);
                        protection.transform.SetParent(transform);
                        protection.transform.localPosition = new Vector3(0, 0.5f, 0);
                        StartCoroutine(DestroyProtectionAfterTime(protection, 3f));
                    }
                }
                else
                {
                    HandleCloudHit();
                }
                // Đánh dấu đã bị damage từ mây
                inCloud = true;
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Cloud"))
        {
            // Khi player rời khỏi vùng mây, cho phép damage ở lần va chạm sau
            inCloud = false;
        }
    }

    IEnumerator DestroyProtectionAfterTime(GameObject protection, float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(protection);
    }

    void HandleFallOff()
    {
        currentLives--;
        itemManager.UpdateLives(currentLives);
        // Phát âm thanh mất mạng
        if (audioManager != null)
            audioManager.PlaySFX(audioManager.loseLifeSound);

        if (currentLives > 0)
        {
            if (lastSafePlatform != null)
            {
                Bounds platformBounds = lastSafePlatform.GetComponent<Collider>().bounds;
                transform.position = new Vector3(platformBounds.center.x, platformBounds.max.y, platformBounds.center.z);
            }
            else
            {
                transform.position = lastSafePosition;
            }
            isGrounded = true;
            anim.SetBool("isCharging", false);
            anim.SetBool("isJumping", false);

            if (lastSafePlatform != null)
            {
                SetNextJumpDirection(nextJumpDirection);
            }
            else
            {
                transform.rotation = Quaternion.identity;
            }
        }
        else
        {
            GameOver();
        }
    }

    void HandleCloudHit()
    {
        if (isGameOver)
            return;
        StartCoroutine(BurnEffect());
        currentLives--;
        itemManager.UpdateLives(currentLives);
        if (currentLives == 0)
        {
            GameOver();
        }
    }

    void GameOver()
    {
        isGameOver = true; // Đánh dấu game đã kết thúc
        if (audioManager != null)
            audioManager.PlaySFX(audioManager.gameOverSound);
        FindObjectOfType<GameOverManager>().ShowGameOver();

        // Dừng tất cả các hoạt động
        StopAllCoroutines();
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        enabled = false;

        MovingPlatform[] movingPlatforms = FindObjectsOfType<MovingPlatform>();
        foreach (var platform in movingPlatforms)
        {
            platform.StopMoving();
        }

        PlatformSpawner platformSpawner = FindObjectOfType<PlatformSpawner>();
        if (platformSpawner != null)
        {
            platformSpawner.enabled = false;
        }
    }

    IEnumerator BurnEffect()
    {
        foreach (Renderer rend in renderers)
        {
            rend.material.color = Color.black;
        }
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].material.color = originalColors[i];
        }
    }

    // Phương thức này dùng để reset trạng thái nạp lực khi game resume
    public void ResetJumpCharging()
    {
        isCharging = false;
        chargeStartTime = 0f;
        if (chargeBar != null)
        {
            chargeBar.value = 0f;
        }
        if (anim != null)
        {
            anim.SetBool("isCharging", false);
        }
    }
}
