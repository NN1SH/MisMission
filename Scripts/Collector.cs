using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collector : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        lItem item = collision.GetComponent<lItem>();
        if (item != null)
        {
            Debug.Log($"Collecting item: {collision.gameObject.name}");
            item.Collect();
        }
    }
}