using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

namespace Map
{
    public class MapManager : BasicSingleton<MapManager>
    {
        [SerializeField] MapConfig config;
        public MapConfig CurrentConfig { get { return config; } }
        public MapView view;

        public Map CurrentMap { get; private set; }

        public static string FILE_PATH { get { return System.IO.Path.Combine(Application.persistentDataPath, "Map.json"); } }

        private void Start()
        {
            if (System.IO.File.Exists(FILE_PATH))
            {
                Debug.Log("Existing map found, loading...");
                var mapJson = System.IO.File.ReadAllText(FILE_PATH);
                var map = JsonConvert.DeserializeObject<Map>(mapJson);
                // using this instead of .Contains()
                if (map.path.Any(p => p.Equals(map.GetBossNode().point)))
                {
                    // player has already reached the boss, generate a new map
                    GenerateNewMap();
                }
                else
                {
                    CurrentMap = map;
                    // player has not reached the boss yet, load the current map
                    view.ShowMap(map);
                }
            }
            else
            {
                GenerateNewMap();
            }
        }

        public void GenerateNewMap()
        {
            var map = MapGenerator.GetMap(config);
            CurrentMap = map;
            //CurrentMap.path.Add(CurrentMap.nodes[0].point);
            view.ShowMap(map);
        }

        public void SaveMap()
        {
            if (CurrentMap == null) return;

            var json = JsonConvert.SerializeObject(CurrentMap, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            System.IO.File.WriteAllText(FILE_PATH, json);
        }

#if UNITY_EDITOR
        public static void GenerateMap()
        {
            if (!Application.isPlaying) return;
            var m = FindObjectOfType<MapManager>();
            if (m) m.GenerateNewMap();
        }
#endif
    }
}
