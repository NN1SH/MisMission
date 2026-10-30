using System;
using System.Collections;
using UnityEngine;

public class Door : MonoBehaviour
{
    public static event Action OnDoorEntered;
    [SerializeField] private Animator animator; // Animator สำหรับ Door

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Player entered Door!");
            if (animator != null) animator.SetTrigger("Open"); // เรียก Animation
            SoundEffectManager.Play("DoorEnter");
            StartCoroutine(TriggerDoorEnter()); // หน่วงเวลาเพื่อให้ Animation เล่นก่อน
        }
    }

    private IEnumerator TriggerDoorEnter()
    {
        yield return new WaitForSeconds(0.5f); // รอให้ Animation เล่น (ปรับเวลาได้)
        OnDoorEntered?.Invoke();
    }
}