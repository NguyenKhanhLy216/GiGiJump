using UnityEngine;

public class RotateItem : MonoBehaviour
{
    public float rotationSpeed = 50f; // Tốc độ xoay (độ/giây)

    void Update()
    {
        // Xoay item theo trục Y
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
    }
}