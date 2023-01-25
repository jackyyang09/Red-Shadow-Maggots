using System.Collections.Generic;
using UnityEngine;

public class PositionListTool : MonoBehaviour
{
    [SerializeField]
    List<Vector3> positions = new List<Vector3>();

    void OnValidate()
    {
        transform.hideFlags = HideFlags.None;
        this.hideFlags = HideFlags.None;
    }

    public List<Vector3> GetPositions()
    {
        return positions;
    }

    public Vector3 GetPositionAtIndex(int index)
    {
        return positions[index];
    }

    public void SetPositionAtIndex(int index, Vector3 newVec)
    {
        positions[index] = newVec;
    }

    public void AddNewPosition(Vector3 newVec)
    {
        positions.Add(newVec);
    }

    public int Count()
    {
        return positions.Count;
    }
}
