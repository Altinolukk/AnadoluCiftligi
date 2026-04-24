using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    // Inspector'dan atayacağımız referanslar
    public GameManager gameManager;
    public TextMeshProUGUI altinText;

    // Her frame'de çalışır, altın yazısını günceller
    void Update()
    {
        if (gameManager != null && altinText != null)
        {
            altinText.text = "Altin: " + gameManager.altin;
        }
    }
}