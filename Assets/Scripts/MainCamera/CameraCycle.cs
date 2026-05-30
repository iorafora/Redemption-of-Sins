using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KameraDongusu : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float hareketHizi = 1f;
    public float sallanmaHizi = 1.5f;
    public float sallanmaMiktari = 0.3f;

    [Header("Geçiş Ayarları")]
    public float sahnedeKalmaSuresi = 5f;
    public float kararmaSuresi = 1.5f;
    public float spawnSagKaydirma = 2f;

    [Header("Referanslar")]
    public Transform[] baslangicNoktalari;
    public Image siyahEkran;

    private int aktifArkaplanIndex = 0;
    private float baslangicY;

    void Start()
    {
        if (siyahEkran != null)
        {
            siyahEkran.color = new Color(0, 0, 0, 1);
            siyahEkran.raycastTarget = false;
        }

        // Oyuna başlarken kamerayı zorla ilk hedefe koy
        if (baslangicNoktalari.Length > 0 && baslangicNoktalari[0] != null)
        {
            Vector3 ilkHedef = baslangicNoktalari[0].position;
            transform.position = new Vector3(ilkHedef.x + spawnSagKaydirma, ilkHedef.y, transform.position.z);
            baslangicY = ilkHedef.y;
        }

        StartCoroutine(GecisDongusu());
    }

    void Update()
    {
        // BÜTÜN FRENLER İPTAL! 
        // Kamera her karede (frame) ne ekranı umursar ne de ışınlanmayı. SADECE AKAR.
        float yeniX = transform.position.x + (hareketHizi * Time.deltaTime);
        float yeniY = baslangicY + (Mathf.Sin(Time.time * sallanmaHizi) * sallanmaMiktari);

        transform.position = new Vector3(yeniX, yeniY, transform.position.z);
    }

    IEnumerator GecisDongusu()
    {
        while (true)
        {
            // 1. AŞAMA: EKRAN AYDINLANIYOR (Kamera arkada durmadan akıyor)
            float sayac = 0;
            while (sayac < kararmaSuresi)
            {
                sayac += Time.deltaTime;
                float saydamlik = Mathf.Lerp(1, 0, sayac / kararmaSuresi);
                if (siyahEkran != null) siyahEkran.color = new Color(0, 0, 0, saydamlik);
                yield return null;
            }

            // 2. AŞAMA: NORMAL İZLEME SÜRESİ (Kamera durmadan akıyor)
            yield return new WaitForSeconds(sahnedeKalmaSuresi);

            // 3. AŞAMA: EKRAN KARARIYOR (Kamera karanlığın içine doğru durmadan akıyor)
            sayac = 0;
            while (sayac < kararmaSuresi)
            {
                sayac += Time.deltaTime;
                float saydamlik = Mathf.Lerp(0, 1, sayac / kararmaSuresi);
                if (siyahEkran != null) siyahEkran.color = new Color(0, 0, 0, saydamlik);
                yield return null;
            }

            // 4. AŞAMA: EKRAN TAM SİYAH! (Salise bile sürmeden sıradakine ışınlanıyor)
            aktifArkaplanIndex++;
            if (aktifArkaplanIndex >= baslangicNoktalari.Length)
            {
                aktifArkaplanIndex = 0;
            }

            Vector3 hedefNokta = baslangicNoktalari[aktifArkaplanIndex].position;
            transform.position = new Vector3(hedefNokta.x + spawnSagKaydirma, hedefNokta.y, transform.position.z);
            baslangicY = hedefNokta.y;
        }
    }
}