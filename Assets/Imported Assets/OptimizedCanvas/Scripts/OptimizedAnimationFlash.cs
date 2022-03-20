using System.Collections;
using UnityEngine;

public class OptimizedAnimationFlash : OptimizedAnimationBase
{
    [SerializeField] OptimizedCanvas canvas = null;
    [SerializeField] float flashTime = 1;

    public override IEnumerator PlayAnimation()
    {
        while (true)
        {
            yield return new WaitForSeconds(flashTime * (1 / speedModifier));
            canvas.SetActive(!canvas.IsVisible);
        }
    }

    private void OnValidate()
    {
        if (canvas == null)
        {
            canvas = GetComponent<OptimizedCanvas>();
        }
    }
}
