using UnityEngine;
using UnityEngine.UI;

// Gắn lên Button để phát tiếng click (qua AudioManager.Instance, dùng được ở mọi scene).
[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        });
    }
}
