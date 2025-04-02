using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    public TMP_Text lifeText;
    public Image heartIcon;
    public TMP_Text shieldText;
    public Image shieldIcon;

    void Start()
    {
        // Đảm bảo số khiên và mạng sống được hiển thị ngay từ đầu game
        UpdateLives(3); // Giả sử người chơi bắt đầu với 3 mạng sống
        UpdateShields(0); // Người chơi bắt đầu với 0 khiên
    }

    public void UpdateLives(int lives)
    {
        lifeText.text = lives.ToString(); // Chỉ hiển thị số lượng mạng sống
    }

    public void UpdateShields(int shields)
    {
        shieldText.text = shields.ToString(); // Chỉ hiển thị số lượng khiên
    }
}