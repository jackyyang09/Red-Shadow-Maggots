using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RepeatingButton : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Button referenceButton;
    [SerializeField] Vector2 invokeTimeRange = new Vector2(0.5f, 0.05f);
    [SerializeField] int invokesToRampUp = 15;
    [SerializeField] UnityEngine.Events.UnityEvent onInvoke = null;

    bool isHeld = false;
    int invokeCount;
    float timer;
    float invokeTime;

    // Update is called once per frame
    void Update()
    {
        if (!referenceButton.interactable)
        {
            isHeld = false;
            enabled = false;
        }

        if (isHeld)
        {
            if (timer > invokeTime)
            {
                onInvoke.Invoke();
                timer = 0;
                invokeCount = Mathf.Clamp(invokeCount + 1, 0, invokesToRampUp);
                invokeTime = Mathf.Lerp(invokeTimeRange.x, invokeTimeRange.y, (float)invokeCount / (float)invokesToRampUp);
            }
            timer += Time.deltaTime;
        }
    }

    public void ButtonPressed()
    {
        enabled = true;
        isHeld = true;
        invokeCount = 0;
        invokeTime = Mathf.Lerp(invokeTimeRange.x, invokeTimeRange.y, 0);
        timer = 0;
        onInvoke.Invoke();
    }

    public void ButtonRelease()
    {
        enabled = false;
        isHeld = false;
    }
}
