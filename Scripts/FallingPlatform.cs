using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    public float fallDelay = 2f; 
    public float destroyDelay = 1f;

    bool isFallin;
    Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isFallin && collision.CompareTag("Player"))
        {
            StartCoroutine(fall()); // Start the falling coroutine when the player collides with the platform
        }
    }

    private IEnumerator fall()
    {
        isFallin = true;
        yield return new WaitForSeconds(fallDelay);
        rb.bodyType = RigidbodyType2D.Dynamic; // Change to dynamic to allow falling
        Destroy(gameObject, destroyDelay); // Destroy the platform after a delay
    }

}
