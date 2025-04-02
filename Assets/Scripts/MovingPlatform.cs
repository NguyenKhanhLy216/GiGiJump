using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public float speed = 1f;
    public float moveRange = 2f; // Biên độ di chuyển
    private Vector3 startPosition;
    private Vector3 direction;
    private bool isMoving = true;

    public void SetMovementDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    void Start()
    {
        startPosition = transform.position; // Ghi lại vị trí ban đầu
    }

    void Update()
    {
        if (isMoving) // Chỉ di chuyển khi isMoving == true
        {
            float movement = Mathf.PingPong(Time.time * speed, moveRange * 2) - moveRange;
            transform.position = startPosition + direction * movement;
        }
    }

    public void StopMoving()
    {
        isMoving = false;
    }
}