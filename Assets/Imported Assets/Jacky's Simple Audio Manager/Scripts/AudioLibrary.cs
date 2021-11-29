﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSAM
{
    [CreateAssetMenu(fileName = "New Audio Library", menuName = "AudioManager/New Audio Library", order = 1)]
    public class AudioLibrary : ScriptableObject
    {
        [System.Serializable]
        public class CategoryToList
        {
            public string name;
            public List<BaseAudioFileObject> files;
            public bool foldout;
        }

        public List<string> soundCategories = new List<string>();
        public List<string> musicCategories = new List<string>();

        public List<JSAMSoundFileObject> Sounds = new List<JSAMSoundFileObject>();
        public List<JSAMMusicFileObject> Music = new List<JSAMMusicFileObject>();

        [Tooltip("Allows you to customize the enum and namespace names for your generated audio. You are recommended to leave this off, for your convenience.")]
        public bool useCustomNames = false;

        public string SafeName { get { return name.ConvertToAlphanumeric(); } }
        public string generatedName;

        public string musicEnum;
        public string defaultMusicEnum { get { return name.ConvertToAlphanumeric() + "Music"; } }
        public string musicEnumGenerated;
        public string musicNamespace;
        public string musicNamespaceGenerated;

        public string soundEnum;
        public string defaultSoundEnum { get { return name.ConvertToAlphanumeric() + "Sounds"; } }
        public string soundEnumGenerated;
        public string soundNamespace;
        public string soundNamespaceGenerated;

        [SerializeField] public List<CategoryToList> soundCategoriesToList = new List<CategoryToList>();
        [SerializeField] public List<CategoryToList> musicCategoriesToList = new List<CategoryToList>();

        void Reset()
        {
            soundCategories.Add(string.Empty);
            musicCategories.Add(string.Empty);

            var ctl = new CategoryToList();
            ctl.name = string.Empty;
            ctl.foldout = true;
            soundCategoriesToList.Add(ctl);
            musicCategoriesToList.Add(ctl);
        }

        public void InitializeValues()
        {
            soundEnum = defaultSoundEnum;
            musicEnum = defaultMusicEnum;
        }

        /// <summary>
        /// Returns an enum type given it's name as a string
        /// https://stackoverflow.com/questions/25404237/how-to-get-enum-type-by-specifying-its-name-in-string
        /// </summary>
        /// <param name="enumName"></param>
        /// <returns></returns>
        public static System.Type GetEnumType(string enumName)
        {
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var type = assemblies[i].GetType(enumName);
                if (type == null)
                    continue;
                if (type.IsEnum)
                    return type;
            }
            return null;
        }
    }
}