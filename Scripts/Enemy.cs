using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Transform player;
    public float chaseSpeed = 5f; // เพิ่มความเร็วเพื่อทดสอบ
    public float jumpForce = 2f;
    public LayerMask groundLayer;
    public Animator animator;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool shouldJump;

    public int damaged = 1;

    public int health = 3;
    private int maxHealth = 3;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    [Header("Loot")]
    public List<LootItem> lootItems = new List<LootItem>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        maxHealth = health;
        originalColor = spriteRenderer.color;
        animator = GetComponent<Animator>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on Enemy!");
        }
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D not found on Enemy!");
        }
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                Debug.LogError("Player Transform not found! Ensure Player has 'Player' tag.");
            }
        }
    }

    void Update()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer);
        Debug.Log($"Is Grounded: {isGrounded}");

        float direction = Mathf.Sign(player.position.x - transform.position.x);
        bool isPlayerAbove = Physics2D.Raycast(transform.position, Vector2.up, 5f,1 << player.gameObject.layer);

        if (isGrounded)
        {
            rb.velocity = new Vector2(direction * chaseSpeed, rb.velocity.y);

            RaycastHit2D groundInFront = Physics2D.Raycast(transform.position, new Vector2(direction, 0), 4f, groundLayer);
            RaycastHit2D gapAhead = Physics2D.Raycast(transform.position + new Vector3(direction, 0, 0), Vector2.down, 3f, groundLayer);
            RaycastHit2D platformAbove = Physics2D.Raycast(transform.position, Vector2.up, 5f, groundLayer);

            if (!groundInFront.collider && !gapAhead.collider)
            {
                shouldJump = true;
            }
            else if (isPlayerAbove && platformAbove.collider)
            {
                shouldJump = true;
            }
            else
            {
                shouldJump = false;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isGrounded && shouldJump)
        {
            shouldJump = false;
            Vector2 direction = (player.position - transform.position).normalized;
            Vector2 jumpdirection = direction * jumpForce;
            rb.AddForce(new Vector2(jumpdirection.x, jumpForce), ForceMode2D.Impulse);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        StartCoroutine(FlashDamageEffect());
        if (health <= 0)
        {
            Die();
        }

    }
    private IEnumerator FlashDamageEffect()
    {
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }
    void Die()
    {
        foreach (LootItem loot in lootItems)
        {
            if (Random.Range(0f, 100f) <= loot.dropChance)
            {
                Instantiate(loot.lootPrefab, transform.position, Quaternion.identity);
                break;
            }
            
        }

        Destroy(gameObject);
    }

    void InstantiateLoot(GameObject loot)
    {
        if (loot)
        {
            GameObject droppedLoot = Instantiate(loot, transform.position, Quaternion.identity);

            droppedLoot.GetComponent<SpriteRenderer>().color = Color.red;
        }
    }
}