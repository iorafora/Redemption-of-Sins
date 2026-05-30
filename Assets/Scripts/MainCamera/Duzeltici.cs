using UnityEngine;

public class Duzeltici : MonoBehaviour
{
    private Transform parentTransform;

    void Start()
    {
        // Kendisini etkileyen ana objeyi (Fireguy, Knight vb.) bul
        parentTransform = transform.parent;
    }

    void LateUpdate()
    {
        if (parentTransform != null)
        {
            // Ana objenin X yönünü al (1 veya -1)
            float parentSign = Mathf.Sign(parentTransform.localScale.x);

            // Eğer ana obje ters dönmüşse (-1), kendi X scale değerini de eksiyle çarparak düz kalmasını sağla
            transform.localScale = new Vector3(parentSign * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
}