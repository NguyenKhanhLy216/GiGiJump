using UnityEngine;

public class ShieldItem : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Lấy tham chiếu đến JumpController của người chơi
            JumpController player = other.GetComponent<JumpController>();

            if (player != null && player.currentShields < player.maxShields)
            {
                // Tăng số lượng khiên của người chơi
                player.currentShields++;
                player.itemManager.UpdateShields(player.currentShields); // Cập nhật giao diện khiên

                // Hủy đối tượng khiên
                Destroy(gameObject);

                Debug.Log("Khiên được tăng lên: " + player.currentShields);
            }
        }
    }
}