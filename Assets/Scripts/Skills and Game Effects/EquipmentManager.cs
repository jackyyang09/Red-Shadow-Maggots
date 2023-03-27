using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : BasicSingleton<EquipmentManager>
{
    [SerializeField] EquipmentObject[] testEquips;
    List<RuntimeEquipment> runtimeEquipment;

    // Start is called before the first frame update
    void Start()
    {
        runtimeEquipment = new List<RuntimeEquipment>();
    
        for (int i = 0; i < testEquips.Length; i++)
        {
            var newEquip = new RuntimeEquipment();
            newEquip.Reference = testEquips[i];
            switch (testEquips[i].timing)
            {
                case EffectActivationTiming.BattlePhase:
                    BattleSystem.OnStartPhase[testEquips[i].battlePhase.ToInt()] += newEquip.Tick;
                    break;
            }
            runtimeEquipment.Add(newEquip);
        }
    }
}
