using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldButton : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Image holdProgressImage = null;

    [SerializeField] float holdTriggerTime = 0.5f;
    [SerializeField] float holdGraphicDelay = 0.2f;

    public bool ForceReleaseAfterInvoke = true;

    float holdTimer;
    bool isHeld;
    bool delayBypassed;

    [SerializeField] public UnityEngine.Events.UnityEvent onHoldEvent = null;
    [SerializeField] public UnityEngine.Events.UnityEvent onClick = null;
    [SerializeField] public UnityEngine.Events.UnityEvent onRelease = null;

    // Update is called once per frame
    void Update()
    {
        if (isHeld)
        {
            if (!delayBypassed)
            {
                if (holdTimer >= holdGraphicDelay)
                {
                    holdTimer = 0;
                    delayBypassed = true;
                }
            }
            else
            {
                if (holdTimer >= holdTriggerTime)
                {
                    onHoldEvent?.Invoke();
                    if (ForceReleaseAfterInvoke)
                    {
                        isHeld = false;
                        holdProgressImage.enabled = false;
                    }
                    else
                    {
                        holdTimer = 0;
                        holdProgressImage.fillAmount = 0;
                    }
                }

                holdProgressImage.fillAmount = Mathf.Clamp01(holdTimer / holdTriggerTime);
            }

            holdTimer += Time.deltaTime;
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
            onClick?.Invoke();
        }

        onRelease?.Invoke();
        holdTimer = 0;
        isHeld = false;
        holdProgressImage.enabled = false;
        holdProgressImage.fillAmount = 0;
        delayBypassed = false;
    }
}