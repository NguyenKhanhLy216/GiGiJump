using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    public GameObject[] platformPrefabs;
    public GameObject cloudPrefab; // Prefab của mây
    public Transform lastPlatform;
    public float minDistance = 7f;
    public float maxDistance = 12f;
    public int maxPlatforms = 10;
    public float movingSpeed = 2f; // Tốc độ di chuyển của khối
    public float cloudSpeed = 2f; // Tốc độ di chuyển của mây
    public float cloudHeight = 5f; // Độ cao của mây so với nền
    public GameObject heartPrefab;
    private Queue<GameObject> spawnedPlatforms = new Queue<GameObject>();
    private Queue<GameObject> spawnedClouds = new Queue<GameObject>(); // Hàng đợi quản lý mây
    private int maxClouds = 2; // Giới hạn số lượng mây trong game
    public GameObject shieldPrefab;
    private bool isGameOver = false;

    public void StopSpawning()
    {
        isGameOver = true;
    }

    void Update()
    {
        if (isGameOver) return; // Nếu game đã kết thúc, không spawn thêm platform
    }
    public void SpawnNextPlatform()
    {
        if (platformPrefabs.Length == 0) return;

        float distance = Random.Range(minDistance, maxDistance);
        Vector3 candidatePos = lastPlatform.position;

        int direction = Random.Range(0, 2);
        switch (direction)
        {
            case 0: candidatePos += new Vector3(-distance, 0, 0); break;
            case 1: candidatePos += new Vector3(0, 0, distance); break;
        }

        GameObject selectedPrefab = platformPrefabs[Random.Range(0, platformPrefabs.Length)];
        GameObject newPlatform = Instantiate(selectedPrefab, candidatePos, Quaternion.identity);
        spawnedPlatforms.Enqueue(newPlatform);
        lastPlatform = newPlatform.transform;

        if (spawnedPlatforms.Count > maxPlatforms)
        {
            GameObject oldPlatform = spawnedPlatforms.Dequeue();
            Destroy(oldPlatform);
        }

        JumpController player = FindObjectOfType<JumpController>();
        if (player != null)
        {
            bool spawnHeart = false;
            bool spawnShield = false;

            if (player.currentLives < player.maxLives && Random.value < 0.5f)
            {
                spawnHeart = true;
            }

            if (player.currentShields < player.maxShields && Random.value < 0.1f)
            {
                spawnShield = true;
            }

            if (spawnHeart && spawnShield)
            {
                if (Random.value < 0.5f)
                    spawnShield = false;
                else
                    spawnHeart = false;
            }

            if (spawnHeart)
            {
                Vector3 heartPosition = newPlatform.transform.position + new Vector3(0, newPlatform.GetComponent<Collider>().bounds.extents.y + 2f, 0);
                GameObject heart = Instantiate(heartPrefab, heartPosition, Quaternion.identity);
                heart.transform.parent = newPlatform.transform;
            }
            else if (spawnShield)
            {
                Vector3 shieldPosition = newPlatform.transform.position + new Vector3(0, newPlatform.GetComponent<Collider>().bounds.extents.y + 2f, 0);
                GameObject shield = Instantiate(shieldPrefab, shieldPosition, Quaternion.identity);
                shield.transform.parent = newPlatform.transform;
            }
        }

        ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        if (player != null && scoreManager != null)
        {
            player.SetNextJumpDirection(candidatePos - player.transform.position);

            int score = scoreManager.GetScore();

            if (score >= 50 && score < 100)
            {
                // Mức độ khó 1: Platform có thể đứng yên hoặc di chuyển
                bool shouldMove = Random.Range(0, 10) < 8; // 80% di chuyển
                if (shouldMove)
                {
                    AddMovingPlatform(newPlatform, direction);
                }
            }
            else if (score >= 100)
            {
                // Mức độ khó 2: Platform di chuyển hoặc xuất hiện mây
                int difficulty = Random.Range(1, 4); // 1, 2, hoặc 3
                switch (difficulty)
                {
                    case 2:
                        AddMovingPlatform(newPlatform, direction);
                        break;
                    case 3:
                        SpawnCloud(candidatePos, direction);
                        break;
                        // difficulty == 1: Không làm gì (platform đứng yên)
                }
            }
        }
    }

    private void AddMovingPlatform(GameObject platform, int direction)
    {
        MovingPlatform movingPlatform = platform.AddComponent<MovingPlatform>();
        int moveDirection = Random.Range(0, 2); // 0 hoặc 1
        if (moveDirection == 0)
        {
            movingPlatform.SetMovementDirection(direction == 0 ? Vector3.forward : Vector3.right);
        }
        else
        {
            movingPlatform.SetMovementDirection(direction == 0 ? Vector3.right : Vector3.forward);
        }
        movingPlatform.speed = movingSpeed;
        movingPlatform.moveRange = 2f;
    }

    private void SpawnCloud(Vector3 position, int direction)
    {
        GameObject cloud = Instantiate(cloudPrefab, position + Vector3.up * cloudHeight, Quaternion.identity);
        spawnedClouds.Enqueue(cloud);
        MovingPlatform cloudMovement = cloud.AddComponent<MovingPlatform>();
        if (direction == 0)
        {
            cloudMovement.SetMovementDirection(Vector3.forward); // Mây di chuyển theo trục X
            cloud.transform.rotation = Quaternion.Euler(0, 90, 0);
        }
        else
        {
            cloudMovement.SetMovementDirection(Vector3.right); // Mây di chuyển theo trục Z
        }
        cloudMovement.speed = cloudSpeed;
        cloudMovement.moveRange = 8f;

        if (spawnedClouds.Count > maxClouds)
        {
            Destroy(spawnedClouds.Dequeue());
        }
    }
}