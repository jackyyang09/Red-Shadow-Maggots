using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Map
{
    public class MapView : MonoBehaviour
    {
        public enum MapOrientation
        {
            BottomToTop,
            TopToBottom,
            RightToLeft,
            LeftToRight
        }

        public MapManager mapManager;
        public MapOrientation orientation;

        [Tooltip(
            "List of all the MapConfig scriptable objects from the Assets folder that might be used to construct maps. " +
            "Similar to Acts in Slay The Spire (define general layout, types of bosses.)")]
        public GameObject nodePrefab;
        [Tooltip("Offset of the start/end nodes of the map from the edges of the screen")]
        public float orientationOffset;
        [Header("Background Settings")]
        [Tooltip("If the background sprite is null, background will not be shown")]
        public Sprite background;
        public Color32 backgroundColor = Color.white;
        [SerializeField] float offsetX;
        [Header("Line Settings")]
        public GameObject linePrefab;
        [Tooltip("Line point count should be > 2 to get smooth color gradients")]
        [Range(3, 10)]
        public int linePointsCount = 10;
        [Tooltip("Distance from the node till the line starting point")]
        public float offsetFromNodes = 0.5f;
        [Header("Colors")]
        [Tooltip("Node Visited or Attainable color")]
        public Color32 visitedColor = Color.white;
        [Tooltip("Locked node color")]
        public Color32 lockedColor = Color.gray;
        [Tooltip("Visited or available path color")]
        public Color32 lineVisitedColor = Color.white;
        [Tooltip("Unavailable path color")]
        public Color32 lineLockedColor = Color.gray;

        [Header("Object References")]
        [SerializeField] private GameObject firstParent;
        [SerializeField] private GameObject mapParent;
        [SerializeField] private SpriteRenderer backgroundObject;
        [SerializeField] private Transform nodeParent, lineParent;
        [SerializeField] Transform startNode, endNode;

        private List<List<Point>> paths;
        private Camera cam;
        // ALL nodes:
        public readonly List<MapNode> MapNodes = new List<MapNode>();

        private readonly List<LineConnection> lineConnections = new List<LineConnection>();

        public static MapView Instance;

        private void Awake()
        {
            Instance = this;
            cam = Camera.main;
        }

        private void ClearMap()
        {
            if (nodeParent)
            {
                for (int i = nodeParent.childCount - 1; i > -1; i--)
                {
                    Destroy(nodeParent.GetChild(i).gameObject);
                }
            }

            if (lineParent)
            {
                for (int i = lineParent.childCount - 1; i > -1; i--)
                {
                    Destroy(lineParent.GetChild(i).gameObject);
                }
            }

            MapNodes.Clear();
            lineConnections.Clear();
        }

        public void ShowMap(Map m)
        {
            if (m == null)
            {
                Debug.LogWarning("Map was null in MapView.ShowMap()");
                return;
            }

            ClearMap();

            float scale = Vector3.Distance(startNode.position, endNode.position) / m.DistanceBetweenFirstAndLastLayers();

            CreateNodes(m.nodes, scale, m);

            DrawLines();
            
            SetOrientation();
            
            SetAttainableNodes();
            
            SetLineColors();
            
            CreateMapBackground(m);
        }

        private void CreateMapBackground(Map m)
        {
            if (background == null) return;

            var bossNode = MapNodes.FirstOrDefault(node => node.Node.nodeType == NodeType.Boss);
            backgroundObject.color = backgroundColor;
            backgroundObject.drawMode = SpriteDrawMode.Sliced;
            backgroundObject.sprite = background;
        }

        private void CreateNodes(IEnumerable<Node> nodes, float scale, Map m)
        {
            GameObject worldNode;
            var mapNode = CreateMapNode(nodes.Last(), scale, out worldNode);
            MapNodes.Add(mapNode);

            var offset = endNode.position - worldNode.transform.position;
            nodeParent.position += offset;

            for (int i = 0; i < nodes.Count() - 1; i++)
            {
                mapNode = CreateMapNode(nodes.ElementAt(i), scale, out worldNode);
                MapNodes.Add(mapNode);
            }
        }

        private MapNode CreateMapNode(Node node, float scale, out GameObject worldNode)
        {
            var mapNodeObject = Instantiate(nodePrefab, nodeParent);
            var mapNode = mapNodeObject.GetComponent<MapNode>();
            var blueprint = GetBlueprint(node.blueprintName);
            mapNode.SetUp(node, blueprint);
            mapNode.transform.position = new Vector3(node.position.y, 0, node.position.x);
            mapNode.transform.localPosition = new Vector3(mapNode.transform.localPosition.x, mapNode.transform.localPosition.y, 0) * scale;
            mapNodeObject.transform.localEulerAngles = Vector3.zero;
            mapNode.transform.localScale *= scale;
            worldNode = mapNodeObject;
            return mapNode;
        }

        public void SetAttainableNodes()
        {
            // first set all the nodes as unattainable/locked:
            foreach (var node in MapNodes)
                node.SetState(NodeStates.Locked);

            if (mapManager.CurrentMap.path.Count == 0)
            {
                // we have not started traveling on this map yet, set entire first layer as attainable:
                foreach (var node in MapNodes.Where(n => n.Node.point.y == 0))
                    node.SetState(NodeStates.Attainable);
            }
            else
            {
                // we have already started moving on this map, first highlight the path as visited:
                foreach (var point in mapManager.CurrentMap.path)
                {
                    var mapNode = GetNode(point);
                    if (mapNode != null)
                        mapNode.SetState(NodeStates.Visited);
                }

                var currentPoint = mapManager.CurrentMap.path[mapManager.CurrentMap.path.Count - 1];
                var currentNode = mapManager.CurrentMap.GetNode(currentPoint);

                // set all the nodes that we can travel to as attainable:
                foreach (var point in currentNode.outgoing)
                {
                    var mapNode = GetNode(point);
                    if (mapNode != null)
                        mapNode.SetState(NodeStates.Attainable);
                }
            }
        }

        public void SetLineColors()
        {
            // set all lines to grayed out first:
            foreach (var connection in lineConnections)
                connection.SetColor(lineLockedColor);

            // set all lines that are a part of the path to visited color:
            // if we have not started moving on the map yet, leave everything as is:
            if (mapManager.CurrentMap.path.Count == 0)
                return;

            // in any case, we mark outgoing connections from the final node with visible/attainable color:
            var currentPoint = mapManager.CurrentMap.path[mapManager.CurrentMap.path.Count - 1];
            var currentNode = mapManager.CurrentMap.GetNode(currentPoint);

            foreach (var point in currentNode.outgoing)
            {
                var lineConnection = lineConnections.FirstOrDefault(conn => conn.from.Node == currentNode &&
                                                                            conn.to.Node.point.Equals(point));
                lineConnection?.SetColor(lineVisitedColor);
            }

            if (mapManager.CurrentMap.path.Count <= 1) return;

            for (var i = 0; i < mapManager.CurrentMap.path.Count - 1; i++)
            {
                var current = mapManager.CurrentMap.path[i];
                var next = mapManager.CurrentMap.path[i + 1];
                var lineConnection = lineConnections.FirstOrDefault(conn => conn.@from.Node.point.Equals(current) &&
                                                                            conn.to.Node.point.Equals(next));
                lineConnection?.SetColor(lineVisitedColor);
            }
        }

        private void SetOrientation()
        {
            var span = mapManager.CurrentMap.DistanceBetweenFirstAndLastLayers();
            var bossNode = MapNodes.FirstOrDefault(node => node.Node.nodeType == NodeType.Boss);
            Debug.Log("Map span in set orientation: " + span + " camera aspect: " + cam.aspect);

            // setting first parent to be right in front of the camera first:
            //firstParent.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
            var offset = orientationOffset;
            switch (orientation)
            {
                case MapOrientation.BottomToTop:
                    //firstParent.transform.localPosition += new Vector3(0, offset, 0);
                    break;
                case MapOrientation.TopToBottom:
                    //mapParent.transform.eulerAngles = new Vector3(0, 0, 180);
                    // factor in map span:
                    //firstParent.transform.localPosition += new Vector3(0, -offset, 0);
                    break;
                case MapOrientation.RightToLeft:
                    offset *= cam.aspect;
                    //mapParent.transform.eulerAngles = new Vector3(0, 0, 90);
                    // factor in map span:
                    //firstParent.transform.localPosition -= new Vector3(offset, bossNode.transform.position.y, 0);
                    break;
                case MapOrientation.LeftToRight:
                    offset *= cam.aspect;
                    //mapParent.transform.eulerAngles = new Vector3(0, 0, -90);
                    //firstParent.transform.localPosition += new Vector3(offset, -bossNode.transform.position.y, 0);
                    break;
            }
        }

        private void DrawLines()
        {
            foreach (var node in MapNodes)
            {
                foreach (var connection in node.Node.outgoing)
                    AddLineConnection(node, GetNode(connection));
            }
        }

        private void ResetNodesRotation()
        {
            foreach (var node in MapNodes)
                node.transform.rotation = Quaternion.identity;
        }

        public void AddLineConnection(MapNode from, MapNode to)
        {
            var lineObject = Instantiate(linePrefab, lineParent.transform);
            var lineRenderer = lineObject.GetComponent<LineRenderer>();
            var fromPoint = from.transform.position +
                            (to.transform.position - from.transform.position).normalized * offsetFromNodes;

            var toPoint = to.transform.position +
                          (from.transform.position - to.transform.position).normalized * offsetFromNodes;

            lineObject.transform.position = fromPoint;

            // line renderer with 2 points only does not handle transparency properly:
            lineRenderer.positionCount = linePointsCount;
            for (var i = 0; i < linePointsCount; i++)
            {
                lineRenderer.SetPosition(i,
                    Vector3.Lerp(toPoint, fromPoint, (float)i / (linePointsCount - 1)));
            }

            var dottedLine = lineObject.GetComponent<DottedLineRenderer>();
            if (dottedLine != null) dottedLine.ScaleMaterial();

            lineConnections.Add(new LineConnection(lineRenderer, from, to));
        }

        private MapNode GetNode(Point p)
        {
            return MapNodes.FirstOrDefault(n => n.Node.point.Equals(p));
        }

        public NodeBlueprint GetBlueprint(NodeType type)
        {
            var config = mapManager.CurrentConfig;
            return config.nodeBlueprints.FirstOrDefault(n => n.nodeType == type);
        }

        public NodeBlueprint GetBlueprint(string blueprintName)
        {
            var config = mapManager.CurrentConfig;
            return config.nodeBlueprints.FirstOrDefault(n => n.name == blueprintName);
        }
    }
}
