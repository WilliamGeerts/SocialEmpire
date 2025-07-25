using UnityEngine;

public class DynamicSorting : MonoBehaviour
{

    void LateUpdate()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = -(int)(transform.position.y * 100);
        }
    }
}