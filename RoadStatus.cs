using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadStatus : MonoBehaviour
{
    [SerializeField] private bool occupied;
    private int occupiedBy;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    //When the Primitive collides with the walls, it will reverse direction
    private void OnTriggerEnter(Collider other)
    {
        occupied = true;
        occupiedBy = other.gameObject.GetInstanceID();
    }

    //When the Primitive exits the collision, it will change Color
    private void OnTriggerExit(Collider other)
    {
        occupied = false;
        occupiedBy = -1;
    }

    public bool isRoadOccupied(int instanceId)
    {
        if (instanceId == occupiedBy) return false;
        else return occupied;
    }
}
