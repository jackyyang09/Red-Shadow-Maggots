using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RSMConstants
{
    public static class Colours
    {
        public static readonly Color Buff = new Color(0.6039216f, 0.8039216f, 1);
        public static readonly Color Debuff = new Color(1f, 0.2431373f, 0.2431373f);
        public static readonly Color CreamWhite = new Color(0.9254902f, 0.8901961f, 0.7960784f);
    }

    public enum StatEnum
    {
        Attack,
        AttackWindow,
        CurrentHealth,
        CritChance,
        CritDamage,
        Defense,
        DefenseWindow,
        Health,
        HealReceived,
        MaxHealth,
        Wait,
        WaitLimit,
        WaitTimer
    }

    public static class Keywords
    {
        public static class Short
        {
            public static string ATTACK = "ATK";
            public static string ATTACK_WINDOW = "ATK WINDOW";
            public static string CURRENT_HEALTH = "CURRENT HP";
            public static string CRIT_CHANCE = "CRIT CHANCE";
            public static string CRIT_DAMAGE = "CRIT DMG";
            public static string DEFENSE = "DEF";
            public static string DEFENSE_WINDOW = "DEF WINDOW";
            public static string DAMAGE = "DMG";
            public static string HEALTH = "HP";
            public static string HEAL_RECEIVED = "HEALING";
            public static string MAX_HEALTH = "MAX HP";
            public static string WAIT = "WAIT";
            public static string WAIT_LIMIT = "WAIT LIMIT";
            public static string WAIT_TIMER = "WAIT TIMER";
        }
    }
}