using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using System.IO;

public class Maze3DBit : MonoBehaviour
{
    public int mazeSize = 5;
    public bool drawGizmos = true;
    [Range(0f, 1f)]
    public float wallDensity = 0.5f;
    public Color wallColor = Color.white;
    [ReadOnly]
    public int farestPathLength;
    Dictionary<EDirection, int> dicDirections;
    List<EDirection> lstDirections;
    List<int> pathGenerated;
    byte[] maze;
    [Button]
    void GenerateMaze()
    {
        maze = new byte[mazeSize * mazeSize * mazeSize];
        for (int i = 0; i < maze.Length; i++)
        {
            maze[i] = 0;
        }

        dicDirections = new Dictionary<EDirection, int>();
        dicDirections.Add(EDirection.Left, -1);
        dicDirections.Add(EDirection.Right, 1);
        dicDirections.Add(EDirection.Up, mazeSize);
        dicDirections.Add(EDirection.Down, -mazeSize);
        dicDirections.Add(EDirection.Forward, mazeSize * mazeSize);
        dicDirections.Add(EDirection.Backward, -mazeSize * mazeSize);

        lstDirections = dicDirections.Keys.ToList();
    }
    [Button]
    void GetClosetPath()
    {
        GenerateMaze();
        int start = Random.Range(0, maze.Length);
        Debug.Log($"Start {start} {GetNodeX(start)} {GetNodeY(start)} {GetNodeZ(start)}");
        int end = Random.Range(0, maze.Length);
        Debug.Log($"End {end} {GetNodeX(end)} {GetNodeY(end)} {GetNodeZ(end)}");
        List<List<int>> lstPaths = new List<List<int>>();
        List<int> lstSearchedPath = new List<int>() { start };
        lstPaths.Add(new List<int> { start });
        int count = 0;
        while (count < mazeSize * 3)
        {
            count++;
            List<List<int>> lstNewPaths = new List<List<int>>();
            foreach (var path in lstPaths)
            {
                int node = path[path.Count - 1];
                foreach (var dir in dicDirections)
                {
                    if (ValidDir(node, dir.Key))
                    {
                        int newNode = node + dir.Value;
                        if (lstSearchedPath.Contains(newNode))
                        {
                            continue;
                        }
                        if (ValidNode(newNode, path, node))
                        {
                            List<int> newPath = new List<int>(path);
                            newPath.Add(newNode);
                            lstSearchedPath.Add(newNode);
                            lstNewPaths.Add(newPath);
                            if (newNode == end)
                            {
                                Debug.Log($"Find Path In {count} step");
                                foreach (var item in newPath)
                                {
                                    maze[item] = 1;
                                }
                                return;
                            }
                        }
                    }
                }
            }
            lstPaths = lstNewPaths;
            Debug.Log($"Step {count} Paths {lstPaths.Count}");
        }
        Debug.Log("No Path Found");
    }
    [Button()]
    void GetFarestPath()
    {
        GenerateMaze();
        int start = Random.Range(0, maze.Length);
        Debug.Log($"Start {start} {GetNodeX(start)} {GetNodeY(start)} {GetNodeZ(start)}");
        List<int> lstValidNode = new List<int>();
        List<List<int>> lstPaths = new List<List<int>>();
        for (int i = 0; i < mazeSize * 2; i++)
        {
            lstPaths.Add(new List<int> { start });
        }
        int count = 0;
        while (count < mazeSize * mazeSize * 3)
        {
            count++;
            foreach (var path in lstPaths)
            {
                int node = path[path.Count - 1];
                lstValidNode.Clear();
                foreach (var direction in dicDirections)
                {
                    if (ValidDir(node, direction.Key))
                    {
                        int newNode = node + direction.Value;
                        if (ValidNode(newNode, path, node))
                        {
                            lstValidNode.Add(newNode);
                        }
                    }
                }
                if (lstValidNode.Count > 0)
                    path.Add(lstValidNode.GetRandom());
            }
        }
        pathGenerated = lstPaths.OrderByDescending(x => x.Count).FirstOrDefault();
        farestPathLength = pathGenerated.Count;
        Debug.Log($"Farest Path {pathGenerated.Count}");
        foreach (var item in pathGenerated)
        {
            maze[item] = 1;
        }
    }
    [Button()]
    void GenerateRandomPos()
    {
        int count = 0;
        List<int> pathToCheck = new List<int>();
        List<int> lstValidNode = new List<int>();
        while (pathGenerated.Count < maze.Length / 2 && count < maze.Length * 3)
        {
            count++;
            pathToCheck.Clear();
            lstValidNode.Clear();
            int node = pathGenerated.GetRandom();
            foreach (var direction in dicDirections)
            {
                if (ValidDir(node, direction.Key))
                {
                    int newNode = node + direction.Value;
                    if (ValidNode(newNode, pathGenerated, node))
                    {
                        lstValidNode.Add(newNode);
                    }
                }
            }
            if (lstValidNode.Count > 0)
            {
                int _node = lstValidNode.GetRandom();
                pathGenerated.Add(_node);
                maze[_node] = 1;
            }
        }
        Debug.Log($"Generate Random Pos On {count} Steps, total node {pathGenerated.Count}");
    }
    bool ValidNode(int node, List<int> path, int lastNode)
    {
        for (int i = 0; i < path.Count; i++)
        {
            if (path[i] == lastNode)
            {
                continue;
            }
            int item = path[i];
            if (Mathf.Abs(GetNodeX(item) - GetNodeX(node)) + Mathf.Abs(GetNodeY(item) - GetNodeY(node))
                + Mathf.Abs(GetNodeZ(item) - GetNodeZ(node)) <= 1)
                return false;
        }
        return true;
    }

    bool ValidDir(int node, EDirection direction)
    {
        switch (direction)
        {
            case EDirection.Left:
                return GetNodeX(node) > 0;
            case EDirection.Right:
                return GetNodeX(node) < mazeSize - 1;
            case EDirection.Up:
                return GetNodeY(node) < mazeSize - 1;
            case EDirection.Down:
                return GetNodeY(node) > 0;
            case EDirection.Forward:
                return GetNodeZ(node) < mazeSize - 1;
            case EDirection.Backward:
                return GetNodeZ(node) > 0;
            default:
                break;
        }
        return false;
    }
    int GetNodeX(int node)
    {
        return node % mazeSize;
    }
    int GetNodeY(int node)
    {
        return (node / mazeSize) % mazeSize;
    }
    int GetNodeZ(int node)
    {
        return node / (mazeSize * mazeSize);
    }
    int GetNode(int x, int y, int z)
    {
        return x + y * mazeSize + z * mazeSize * mazeSize;
    }
    [Button()]
    public void RotateXY(int z)
    {
        //rotate xy plane of z on z axis
        byte[] tmp = (byte[])maze.Clone();
        for (int x = 0; x < mazeSize; x++)
        {
            for (int y = 0; y < mazeSize; y++)
            {
                maze[GetNode(x, y, z)] = tmp[GetNode(y, mazeSize - 1 - x, z)];
            }
        }
        Debug.Log("Rotate XY");
    }
    [Button()]
    public void RotateXZ(int y)
    {
        //Rotate xz plane of y on y axis
        byte[] tmp = (byte[])maze.Clone();
        for (int x = 0; x < mazeSize; x++)
        {
            for (int z = 0; z < mazeSize; z++)
            {
                maze[GetNode(x, y, z)] = tmp[GetNode(z, y, mazeSize - 1 - x)];
            }
        }
    }
    [Button()]
    public void RotateYZ(int x)
    {
        //Rotate yz plane of x on x axis
        byte[] tmp = (byte[])maze.Clone();
        for (int y = 0; y < mazeSize; y++)
        {
            for (int z = 0; z < mazeSize; z++)
            {
                maze[GetNode(x, y, z)] = tmp[GetNode(x, z, mazeSize - 1 - y)];
            }
        }
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (maze == null || !drawGizmos)
            return;
        wallColor.a = wallDensity;
        for (int i = 0; i < maze.Length; i++)
        {
            Gizmos.color = maze[i] == 0 ? wallColor : Color.red;
            Gizmos.DrawCube(new Vector3(i % mazeSize, (i / mazeSize) % mazeSize, i / (mazeSize * mazeSize)), Vector3.one);
        }
    }
#endif
}