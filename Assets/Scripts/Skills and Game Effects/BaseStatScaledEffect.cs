using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseStatScaledEffect : BaseGameEffect
{
    [SerializeField] protected BaseGameStat stat;
    public BaseGameStat Stat => stat;
}