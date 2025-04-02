using UnityEngine;

public class HeartItem : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Lấy tham chiếu đến JumpController của người chơi
            JumpController player = other.GetComponent<JumpController>();

            if (player != null && player.currentLives < player.maxLives)
            {
                // Tăng mạng sống của người chơi
                player.currentLives++;
                player.itemManager.UpdateLives(player.currentLives); // Cập nhật giao diện mạng sống

                // Hủy đối tượng tim
                Destroy(gameObject);

                Debug.Log("Mạng sống được tăng lên: " + player.currentLives);
            }
        }
    }
}