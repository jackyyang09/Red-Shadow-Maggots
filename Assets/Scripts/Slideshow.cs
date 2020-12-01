using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slideshow : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Image image = null;
    [SerializeField] List<Sprite> pictures = new List<Sprite>();
    [SerializeField] int currentIndex = 0;

    [SerializeField] float timeToTransition = 1;
    [SerializeField] bool restartOnEnable = false;

    // Start is called before the first frame update
    //void Start()
    //{
    //    
    //}

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    Coroutine slideShowRoutine;

    public void OnEnable()
    {
        if (slideShowRoutine == null)
        {
            slideShowRoutine = StartCoroutine(SlideShowTime());
        }
    }

    public void OnDisable()
    {
        if (slideShowRoutine != null)
        {
            StopCoroutine(slideShowRoutine);
            slideShowRoutine = null;
        }
    }

    IEnumerator SlideShowTime()
    {
        if (restartOnEnable) currentIndex = 0;
        while (true)
        {
            image.sprite = pictures[currentIndex];
            currentIndex = (int)Mathf.Repeat(currentIndex + 1, pictures.Count);
            yield return new WaitForSeconds(timeToTransition);
        }
    }
}
