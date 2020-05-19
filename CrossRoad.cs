using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossRoad : MonoBehaviour
{
    [SerializeField] private bool occupied;
    [SerializeField] private int occupiedBy;
    public enum DIRECTION_FROM { EAST, WEST, NORTH, SOUTH};
    [SerializeField] List<DIRECTION_FROM> directionsAvailable;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    public List<DIRECTION_FROM> GetPossibleDirections(DIRECTION_FROM from)
    {
        List<DIRECTION_FROM> possibilities = directionsAvailable.GetRange(0, directionsAvailable.Count);
        possibilities.Remove(from);
        return possibilities;
    }
    public bool IsRoadOccupied(int instanceId)
    {
        if (instanceId == occupiedBy) return false;
        else return occupied;
    }

    internal void OccupiedByMe(int instanceId)
    {
        occupiedBy = instanceId;
        occupied = true;
    }

    internal void FreeCrossRoad()
    {
        occupiedBy = -1;
        occupied = false;
    }
}
