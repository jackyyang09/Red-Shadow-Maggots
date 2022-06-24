using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapSystem : MonoBehaviour
{
    [System.Serializable]
    class MapNode
    {
        public MapNode parentNode;
        public Vector3 parentPosition { get { return parentNode.nodeObject.transform.position; } }
        public GameObject nodeObject;
        public Vector3 nodePosition { get { return nodeObject.transform.position; } }
    }

    [SerializeField] int rounds;

    [Header("Map Properties")]
    [SerializeField] ModularBox spawnArea;
    [SerializeField] float mapY;
    [SerializeField] Transform startNodePos;
    [SerializeField] Transform endNodePos;

    [Header("Object References")]
    [SerializeField] GameObject linePrefab;
    [SerializeField] GameObject[] nodePrefabs;
    [SerializeField] List<MapNode> mapNodes = new List<MapNode>();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < rounds; i++)
        {
            int nodesThisRound = 1;

            GameObject newObject = null;
            MapNode newNode = new MapNode();

            if (i != 0 || i != rounds - 1) nodesThisRound = Random.Range(1, 2);
            else newNode.nodeObject = newObject;

            for (int j = 0; j < nodesThisRound; j++)
            {
                var pos = spawnArea.GetRandomPointInBox();
                pos.y = mapY;
                newObject = Instantiate(nodePrefabs[Random.Range(0, nodePrefabs.Length)], transform);
                newObject.transform.position = pos;
                newNode.nodeObject = newObject;
                if (i > 0)
                {
                    newNode.parentNode = mapNodes[i - 1];
                }
            }
            mapNodes.Add(newNode);

            if (i == 0) newObject.transform.position = startNodePos.position;
            else if (i == rounds - 1) newObject.transform.position = endNodePos.position;
        }

        for (int i = 1; i < mapNodes.Count; i++)
        {
            var line = Instantiate(linePrefab, transform).GetComponent<LineRenderer>();
            line.SetPosition(0, mapNodes[i].parentPosition);
            line.SetPosition(1, mapNodes[i].nodePosition);
        }
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < mapNodes.Count; i++)
        {
            if (mapNodes[i].parentNode != null)
            {
                Gizmos.DrawLine(
                    mapNodes[i].parentPosition,
                    mapNodes[i].nodePosition);
            }
        }
    }
}