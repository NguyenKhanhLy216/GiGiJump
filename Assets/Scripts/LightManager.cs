using UnityEngine;
using UnityEngine.SceneManagement;

public class LightManager : MonoBehaviour
{
    private Light dirLight; // Biến lưu trữ ánh sáng chính

    void Start()
    {
        // Tìm Directional Light trong scene
        dirLight = FindObjectOfType<Light>();

        if (dirLight == null)
        {
            Debug.LogError("Không tìm thấy Directional Light! Đảm bảo có ánh sáng trong scene.");
            return;
        }

        // Đảm bảo ánh sáng không bị tắt
        dirLight.enabled = true;

        // Cập nhật Global Illumination để tránh mất ánh sáng khi load scene
        DynamicGI.UpdateEnvironment();
    }
}
