using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseStatRenderer : MonoBehaviour
{
    public abstract void UpdateRendererForCharacter(PlayerData.MaggotState state, CharacterObject character, bool isEnemy);
}