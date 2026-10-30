using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxhealth;
    private int currentHealth;

    public HealthUI healthUI;

    private SpriteRenderer spriteRenderer;

    public static event Action OnplayerDied;

    void Start()
    {
        ResetHealth();
        spriteRenderer = GetComponent<SpriteRenderer>();
        GameController.OnReset += ResetHealth; // เปลี่ยนจาก OnResetGame เป็น OnReset
        HealhtItem.OnHealthCollected += Heal;
    }

    void OnDestroy()
    {
        GameController.OnReset -= ResetHealth; // ยกเลิกการสมัคร
        HealhtItem.OnHealthCollected -= Heal;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy)
        {
            SoundEffectManager.Play("PlayerHit");
            TakeDamage(enemy.damaged);
        }
        Trap trap = collision.GetComponent<Trap>();
        if (trap && trap.damage > 0)
        {
            SoundEffectManager.Play("PlayerHit");
            TakeDamage(trap.damage);
        }
        else if (trap)
        {
            SoundEffectManager.Play("Bounce");
        }
    }

    void Heal(int healamount)
    {
        currentHealth += healamount;
        if (currentHealth > maxhealth)
        {
            currentHealth = maxhealth;
        }
        healthUI.UpdateHearts(currentHealth);
    }

    void ResetHealth()
    {
        currentHealth = maxhealth;
        healthUI.SetMaxHeart(maxhealth);
        Debug.Log("PlayerHealth: ResetHealth called");
    }

    private void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthUI.UpdateHearts(currentHealth);

        StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            OnplayerDied?.Invoke();
        }
    }

    private IEnumerator FlashRed()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        spriteRenderer.color = Color.blue; // อาจเปลี่ยนเป็น Color.red เพื่อให้สมเหตุสมผล
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = Color.white;
    }
}