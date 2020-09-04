using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleHealth : MonoBehaviour
{
    [SerializeField]
    float updateDelay = 0.5f;

    [SerializeField]
    Image healthBar;

    [SerializeField]
    Image tweenBar;

    [SerializeField]
    BaseCharacter baseCharacter;

    // Start is called before the first frame update
    void Start()
    {
        baseCharacter.onTakeDamage += UpdateValue;

        healthBar.fillAmount = baseCharacter.GetHealthPercent();
    }

    // Update is called once per frame
    //void Update()
    //{
    //    
    //}

    public void UpdateValue()
    {
        tweenBar.fillAmount = healthBar.fillAmount;
        healthBar.fillAmount = baseCharacter.GetHealthPercent();
        if (IsInvoking("CatchUp"))
        {
            CancelInvoke("CatchUp");
        }
        Invoke("CatchUp", updateDelay);
    }

    void CatchUp()
    {
        tweenBar.fillAmount = healthBar.fillAmount;
    }

    //private void OnEnable()
    //{
    //    
    //}
}
