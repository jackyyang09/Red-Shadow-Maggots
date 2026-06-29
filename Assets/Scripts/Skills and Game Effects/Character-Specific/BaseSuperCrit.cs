using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Facade;

namespace SuperCrits
{
    public class BaseSuperCrit : BaseCharacterPassive
    {
        BaseAbilityObject SuperCrit => baseCharacter.Reference.superCritical;

        protected override void Init()
        {
        }

        protected override void Cleanup()
        {
        }

        public void OnBeginSuperCrit(int startIndex)
        {
            effectsApplied = startIndex;
        }

        int effectsApplied = 0;
        public void ApplyNextSuperCritEffect()
        {
            var group = SuperCrit.effects[effectsApplied].Copy();
            var targets = group.effectTarget.GetTargets(baseCharacter, battleSystem.OpposingCharacter);

            StartCoroutine(group.appStyle.Apply(group, baseCharacter, targets[0]));

            effectsApplied++;
        }
    }
}