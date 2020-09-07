using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShuffleController : MonoBehaviour
{
    public Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("s"))
        {
            anim.SetTrigger("ShuffleTrigger");
        }

        if (Input.GetKeyDown("k"))
        {
            anim.SetTrigger("PresentTrigger");
        }
    }
}
