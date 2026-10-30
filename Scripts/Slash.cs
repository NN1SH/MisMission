using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slash : MonoBehaviour
{
    public float lifetime = 1f;
    public int damage = 1;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy)
        {
            enemy.TakeDamage(damage);
            SoundEffectManager.Play("EnemyHit");

            Destroy(gameObject);
        }
        
    }
}
