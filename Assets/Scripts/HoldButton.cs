using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldButton : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Image holdProgressImage = null;

    [SerializeField] float holdTriggerTime = 0.5f;
    [SerializeField] float holdGraphicDelay = 0.2f;

    float holdTimer;
    bool isHeld;

    [SerializeField] UnityEngine.Events.UnityEvent onHoldEvent = null;
    [SerializeField] UnityEngine.Events.UnityEvent onClick = null;
    [SerializeField] UnityEngine.Events.UnityEvent onRelease = null;

    // Update is called once per frame
    void Update()
    {
        if (isHeld)
        {
            if (holdTimer >= holdTriggerTime)
            {
                onHoldEvent.Invoke();
                isHeld = false;
                holdProgressImage.enabled = false;
            }
            else
            {
                holdTimer += Time.deltaTime;
                holdProgressImage.fillAmount = Mathf.Clamp01((holdTimer - holdGraphicDelay) / (holdTriggerTime - holdGraphicDelay));
            }
        }
    }

    public void ButtonPressed()
    {
        isHeld = true;
        holdProgressImage.enabled = true;
        holdProgressImage.fillAmount = 0;
    }

    public void ButtonRelease()
    {
        if (holdTimer < holdTriggerTime)
        {
            onClick.Invoke();
        }
        onRelease.Invoke();
        holdTimer = 0;
        isHeld = false;
        holdProgressImage.enabled = false;
        holdProgressImage.fillAmount = 0;
    }
}