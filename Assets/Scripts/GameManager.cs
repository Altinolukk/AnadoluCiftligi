using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Oyuncunun altın miktarı. public = başka scriptler de görebilir.
    public int altin = 0;

    // Oyun başladığında bir kere çalışır
    void Start()
    {
        Debug.Log("Oyun basladi. Mevcut altin: " + altin);
    }

    // Altını 1 artıran fonksiyon. Butonla tetiklenecek.
    public void AltinArtir()
    {
        altin++;
        Debug.Log("Altin artti. Yeni deger: " + altin);
    }
}