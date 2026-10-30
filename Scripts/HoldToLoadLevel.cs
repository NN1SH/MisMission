using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HoldToLoadLevel : MonoBehaviour
{
    public float holdDuration = 1f; // Duration to hold the button
    public Image fillCircle;

    private float holdTimer = 0f;
    private bool isHolding = false;

    public static event Action OnHoldComplete;

    void Update()
    {
        if (!gameObject.activeInHierarchy)
        {
            ResetHold();
            return;
        }

        if (isHolding)
        {
            holdTimer += Time.deltaTime;
            if (fillCircle != null)
            {
                fillCircle.fillAmount = holdTimer / holdDuration;
            }
            if (holdTimer >= holdDuration)
            {
                OnHoldComplete?.Invoke();
                SoundEffectManager.Play("Hold");
                ResetHold();
            }
        }
    }

    public void OnHold(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            isHolding = true;
            holdTimer = 0f;
            if (fillCircle != null)
            {
                fillCircle.fillAmount = 0f;
            }
        }
        else if (context.canceled)
        {
            ResetHold();
        }
    }

    private void ResetHold()
    {
        isHolding = false;
        holdTimer = 0f;
        if (fillCircle != null)
        {
            fillCircle.fillAmount = 0f;
        }
    }
}