using System.Collections;
using UnityEngine;

public abstract class OptimizedAnimationBase : MonoBehaviour
{
    [SerializeField] protected bool stopOnParentHide = true;

    [SerializeField] protected float speedModifier = 1;

    OptimizedCanvas parentCanvas = null;

    private void OnEnable()
    {
        if (stopOnParentHide)
        {
            parentCanvas = transform.parent.GetComponentInParent<OptimizedCanvas>();
            parentCanvas.onCanvasHide.AddListener(StopAnimating);
        }
    }

    private void OnDisable()
    {
        if (stopOnParentHide && parentCanvas != null)
        {
            parentCanvas.onCanvasHide.RemoveListener(StopAnimating);
        }
    }

    Coroutine animationRoutine = null;
    public abstract IEnumerator PlayAnimation();
    public void StartAnimating() => animationRoutine = StartCoroutine(PlayAnimation());
    public void StopAnimating()
    {
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }
    }
}