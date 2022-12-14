using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using Priority_Queue;

public class CubeCoord {
    private Vector3Int coords;

    public CubeCoord(int x, int y, int z) 
    {
        coords = new Vector3Int(x, y, z);
    }

    public CubeCoord(Vector3Int vec)
    {
        coords = vec;
    }

    public int x {
        get { return coords.x; }
        set { coords.x = value; }
    }

    public int y {
        get { return coords.y; }
        set { coords.y = value; }
    }

    public int z {
        get { return coords.z; }
        set { coords.z = value; }
    }

    public int q {
        get { return coords.x; }
        set { coords.x = value; }
    }

    public int r {
        get { return coords.y; }
        set { coords.y = value; }
    }

    public int s {
        get { return coords.z; }
        set { coords.z = value; }
    }

    public static CubeCoord operator +(CubeCoord a, CubeCoord b) {
        return new CubeCoord(a.coords + b.coords);
    }

    public static CubeCoord operator -(CubeCoord a, CubeCoord b) {
        return new CubeCoord(a.coords + b.coords);
    }

    public override string ToString() {
        return coords.ToString();
    }
}

public class TileManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap map;
    
    [SerializeField]
    private List<TileDataScriptableObject> tileDatas;

    private List<Vector3Int> coloredTiles = new List<Vector3Int>();

    private CubeCoord[] directions = {new CubeCoord(1, 0, -1), new CubeCoord(1, -1, 0), 
             new CubeCoord(0, -1, 1), new CubeCoord(-1, 0, 1), new CubeCoord(-1, 1, 0), new CubeCoord(0, 1, -1)};

    public enum CubeDirections : int {
        RIGHT = 0,
        TOP_RIGHT = 1,
        TOP_LEFT = 2,
        LEFT = 3,
        BOTTOM_LEFT = 4,
        BOTTOM_RIGHT = 5
    }

    private Dictionary<TileBase, TileDataScriptableObject> baseTileDatas;  
    public Dictionary<Vector3Int, DynamicTileData> dynamicTileDatas;
    
    private Unit selectedUnit;

    // Start is called before the first frame update
    void Awake()
    {
        baseTileDatas = new Dictionary<TileBase, TileDataScriptableObject>();
        dynamicTileDatas = new Dictionary<Vector3Int, DynamicTileData>();
        foreach (TileDataScriptableObject tileData in tileDatas)
        {
            foreach (TileBase tile in tileData.tiles)
            {
                baseTileDatas.Add(tile, tileData);
            }
        }
        for (int x = (int)map.localBounds.min.x; x < map.localBounds.max.x; x++)
        {
            for (int y = (int)map.localBounds.min.y; y < map.localBounds.max.y; y++)
            {
                for (int z = (int)map.localBounds.min.z; z < map.localBounds.max.z; z++)
                {
                    dynamicTileDatas.Add(new Vector3Int(x, y, z), new DynamicTileData());
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Vector2 mapPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int gridPosition = map.WorldToCell(mapPosition);

            TileBase clickedTile = map.GetTile(gridPosition);

            if (dynamicTileDatas[gridPosition].unit == null)
            {

            }
            Debug.Log("Clicked: " + gridPosition);
            ClearHighlights();
            List<Vector3Int> path = FindShortestPath(Vector3Int.zero, gridPosition);
            foreach(Vector3Int tile in path)
            {
                Debug.Log(tile);
            }
            HighlightPath(path, Color.yellow);
        }
    }

    public void AddUnit(Vector3Int location, Unit unit)
    {
        dynamicTileDatas[location].unit = unit;
    }

    public int Distance(CubeCoord start, CubeCoord end)
    {
        CubeCoord temp = start - end;
        return Mathf.Max(Mathf.Abs(temp.x), Mathf.Abs(temp.y), Mathf.Abs(temp.z));
    }

    public int Distance(Vector3Int start, Vector3Int end) 
    {
        return Distance(UnityCellToCube(start), UnityCellToCube(end));
    }

    public bool IsImpassable(Vector3Int cellCoords)
    {
        TileBase tile = map.GetTile(cellCoords);
        if(!tile)
        {
            // Tiles outside map are impassible
            return true;
        }
        return false; // TODO: baseTileDatas doesn't work. Will probably need to make child of Tile class ?
    }

    public bool IsImpassable(CubeCoord cubeCoords) 
    {
        return IsImpassable(CubeToUnityCell(cubeCoords));
    }

    public bool InBounds(Vector3Int cellCoords)
    {
        if (dynamicTileDatas.ContainsKey(cellCoords))
        {
            return true;
        }
        return false;
    }

    public List<Vector3Int> FindShortestPath(Vector3Int start, Vector3Int goal)
    {
        return FindShortestPath(start, goal, (pos) => 10);
    }

    public List<Vector3Int> FindShortestPath(Vector3Int start, Vector3Int goal, System.Func<Vector3Int, float> tileCostFunction)
    {
        if(!map.GetTile(start))
        {
            return new List<Vector3Int>();
        }

        SimplePriorityQueue<Vector3Int> frontier = new SimplePriorityQueue<Vector3Int>();
        frontier.Enqueue(start, 0);
        Dictionary<Vector3Int, Vector3Int> came_from = new Dictionary<Vector3Int, Vector3Int>();
        Dictionary<Vector3Int, float> cost_so_far = new Dictionary<Vector3Int, float>();
        came_from[start] = start;
        cost_so_far[start] = 0;

        while(frontier.Count > 0)
        {
            Vector3Int current = frontier.Dequeue();

            if(current == goal)
            {
                break;
            }

            foreach(CubeDirections direction in System.Enum.GetValues(typeof(CubeDirections)))
            {
                Vector3Int next = CubeToUnityCell(CubeNeighbor(current, direction));
                if(!IsImpassable(next))
                {
                    float new_cost = cost_so_far[current] + tileCostFunction(next);
                    if(!cost_so_far.ContainsKey(next) || new_cost < cost_so_far[next])
                    {
                        cost_so_far[next] = new_cost;
                        float priority = new_cost + Distance(next, goal);
                        frontier.Enqueue(next, priority);
                        came_from[next] = current;
                    }
                }
            }
        }

        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int last = goal;
        if(!came_from.ContainsKey(last))
        {
            return path;
        }
        path.Insert(0, last);
        while(!came_from[last].Equals(start))
        {
            last = came_from[last];
            path.Insert(0, last);
        }

        return path;
    }

    public void SetTileColor(Vector3Int cellCoord, Color color)
    {
        map.SetTileFlags(cellCoord, TileFlags.None);
        map.SetColor(cellCoord, color);
        coloredTiles.Add(cellCoord);
    }

    public void HighlightPath(List<Vector3Int> path, Color color)
    {
        foreach(Vector3Int tile in path)
        {
            SetTileColor(tile, color);
        }
    }

    public void ClearHighlights()
    {
        foreach(Vector3Int tile in coloredTiles)
        {
            map.SetColor(tile, Color.white);
        }
        coloredTiles.Clear();
    }

    public CubeCoord CubeNeighbor(Vector3Int cellCoord, CubeDirections direction)
    {
        return CubeNeighbor(UnityCellToCube(cellCoord), direction);
    }

    public CubeCoord CubeNeighbor(CubeCoord cubeCoords, CubeDirections direction)
    {
        return cubeCoords + GetCubeDirection(direction);
    }

    private CubeCoord GetCubeDirection(CubeDirections direction)
    {
        return directions[(int) direction];
    }

    private CubeCoord UnityCellToCube(Vector3Int cell)
    {
        var col = cell.x; 
        var row = cell.y * -1;
        var q = col - (row - (row & 1)) / 2;
        var r = row;
        var s = -q - r;
        return new CubeCoord(q, r, s);
    }

    private Vector3Int CubeToUnityCell(CubeCoord cube)
    {
        var q = cube.x;
        var r = cube.y;
        var col = q + (r - (r & 1)) / 2;
        var row = r * -1;

        return new Vector3Int(col, row,  0);
    }

}
