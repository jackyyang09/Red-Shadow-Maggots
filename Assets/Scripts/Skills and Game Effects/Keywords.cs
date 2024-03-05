using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RSMConstants
{
    public enum StatEnum
    {
        Attack = 0,
        AttackWindow = 1,
        CurrentHealth,
        CritChance,
        CritDamage,
        Defense,
        DamageReduction = 999,
        DefenseWindow,
        Health,
        HealReceived,
        Level,
        MaxHealth,
        Name,
        Shield,
        Wait,
        WaitFull,
        WaitLimit,
        WaitPercent,
        WaitTime,
    }

    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        SuperRare,
        UltraRare,
        Count
    }

    public static class Colours
    {
        public static readonly Color Buff = new Color(0.6039216f, 0.8039216f, 1);
        public static readonly Color Debuff = new Color(1f, 0.2431373f, 0.2431373f);
        public static readonly Color CreamWhite = new Color(0.9254902f, 0.8901961f, 0.7960784f);

        public static readonly Color Button = new Color(0.4627451f, 0.4196078f, 0.3686275f);
        public static readonly Color ButtonSelected = new Color(0.8666667f, 0.3921569f, 0.3098039f);

        public static readonly Color Common = new Color(0.6588235f, 0.4196079f, 0.2313726f);
        public static readonly Color Uncommon = new Color(0.6588235f, 0.4196079f, 0.2313726f);
        public static readonly Color Rare = Color.white;
        public static readonly Color SuperRare = new Color(1, 0.75f, 0);
        public static readonly Color UltraRare = new Color(1, 0.75f, 0);
        public static readonly Color[] ColourFromEnum = new Color[]
        {
            Common, Uncommon, Rare, SuperRare, UltraRare
        };
    }

    public static class Keywords
    {
        public static class Rarities
        {
            public const string Common = "Mercenary";
            public const string Uncommon = "Uncommon Mercenary";
            public const string Rare = "Primeval Warrior";
            public const string SuperRare = "Grizzled Veteran";
            public const string UltraRare = "Unusual Veteran";
            public static readonly string[] TitleFromEnum = new string[]
            { 
                Common, Uncommon, Rare, SuperRare, UltraRare
            };
        }

        public static class Short
        {
            public const string ATTACK = "ATK";
            public const string ATTACK_WINDOW = "ATK WINDOW";
            public const string CURRENT_HEALTH = "CURRENT HP";
            public const string CRIT_CHANCE = "CRIT CHANCE";
            public const string CRIT_DAMAGE = "CRIT DMG";
            public const string DEFENSE = "DEF";
            public const string DEFENSE_WINDOW = "DEF WINDOW";
            public const string DAMAGE = "DMG";
            public const string DAMAGE_ABSORPTION = "DMG ABSORPTION";
            public const string HEALTH = "HP";
            public const string HEAL_RECEIVED = "HEALING";
            public const string MAX_HEALTH = "MAX HP";
            public const string WAIT = "WAIT";
            public const string WAIT_LIMIT = "WAIT LIMIT";
            public const string WAIT_TIMER = "WAIT TIMER";
        }
    }

    public static class Extensions
    {
        public static string ToTitle(this Rarity r)
        {
            return Keywords.Rarities.TitleFromEnum[(int)r];
        }

        public static Color ToColour(this Rarity r)
        {
            return Colours.ColourFromEnum[(int)r];
        }
    }
}