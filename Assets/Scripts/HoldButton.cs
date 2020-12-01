using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldButton : MonoBehaviour
{
    [SerializeField] float holdTriggerTime;
    float holdTimer;
    bool isHeld;

    [SerializeField] UnityEngine.Events.UnityEvent onHoldEvent;
    [SerializeField] UnityEngine.Events.UnityEvent onClick;
    [SerializeField] UnityEngine.Events.UnityEvent onRelease;

    // Update is called once per frame
    void Update()
    {
        if (isHeld)
        {
            if (holdTimer >= holdTriggerTime)
            {
                onHoldEvent.Invoke();
                isHeld = false;
            }
            holdTimer += Time.deltaTime;
        }
    }

    public void ButtonPressed()
    {
        isHeld = true;
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
    }
}