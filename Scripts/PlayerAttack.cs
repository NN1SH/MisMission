using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public GameObject slashPrefab; // Prefab ของคลื่นดาบ
    public Transform slashSpawnPoint; // จุดปล่อยคลื่นดาบ
    public float slashSpeed = 10f;
    [SerializeField] private float attackCooldown = 0.15f; // Cooldown 0.15 วินาที

    private Animator animator;
    private PlayerMovement playerMovement; // อ้างอิงฝั่ง movement
    private float lastAttackTime; // เวลาที่โจมตีครั้งล่าสุด

    void Start()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        lastAttackTime = -attackCooldown; // ให้โจมตีได้ทันทีเมื่อเริ่ม
        if (slashPrefab == null) Debug.LogError("SlashPrefab is not assigned in PlayerAttack!");
        if (slashSpawnPoint == null) Debug.LogError("SlashSpawnPoint is not assigned in PlayerAttack!");
    }

    private void Update()
    {
        if (GameController.IsPaused) return;

        // ตรวจสอบการกดปุ่มโจมตีและ Cooldown
        if (Input.GetMouseButtonDown(0) && Time.unscaledTime >= lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.unscaledTime;
        }
    }

    void Attack()
    {
        // เล่นอนิเมชันโจมตี
        animator.SetTrigger("attack");

        SoundEffectManager.Play("Slash");

        // สร้างคลื่นดาบ
        GameObject slash = Instantiate(slashPrefab, slashSpawnPoint.position, Quaternion.identity);

        // กำหนดทิศทางตามการหันของผู้เล่น
        float direction = playerMovement.transform.localScale.x > 0 ? 1f : -1f;
        slash.GetComponent<Rigidbody2D>().velocity = new Vector2(direction * slashSpeed, 0f);
        Destroy(slash, 2f); // ลบคลื่นดาบหลังจาก 2 วินาที

        // กลับด้านภาพ Slash ถ้าผู้เล่นหันซ้าย
        if (direction < 0)
        {
            Vector3 scale = slash.transform.localScale;
            scale.x *= -1;
            slash.transform.localScale = scale;
        }
    }
}