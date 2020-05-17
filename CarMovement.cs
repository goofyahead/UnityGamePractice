using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    private const int VIEW_DISTANCE = 6;
    [SerializeField] private const int DETECTION_ANGLE = 15;
    [SerializeField] private const int DETECTION_DISTANCE = 2;
    [SerializeField] private int speed = 3;

    void Start()
    {

    }

    void Update()
    {
        DetectRoadAhead();
        //DetectObstacles();
        //DebugRays();
        MoveVehicle();
    }

    private void DebugRays()
    {
        Vector3 ray1 = transform.forward * DETECTION_DISTANCE;
        ray1.y = ray1.y - 0.6f;
        Debug.DrawRay(transform.position + new Vector3(0, 2.8f, 0), ray1 * 8, Color.cyan);

    }

    private void DetectObstacles()
    {
        Vector3 collisionDetector = transform.forward * VIEW_DISTANCE;
        RaycastHit hit;
        Ray drivingRay = new Ray(transform.position + new Vector3(0, 2.8f, 0), collisionDetector);
        Debug.DrawRay(transform.position + new Vector3(0, 1f, 0), collisionDetector, Color.red);

        if (Physics.Raycast(drivingRay, out hit, VIEW_DISTANCE))
        {
            GameObject objectFound = hit.transform.gameObject;
            Debug.Log("Stopping to avoid collision with: " + objectFound.name);
            speed = 0;
        }
        else
        {
            speed = 5;
        }
    }

    private void DetectRoadAhead()
    {
        Vector3 roadDectector = transform.forward * DETECTION_DISTANCE;
        roadDectector.y -= 0.6f; ;
              
        RaycastHit hit;
        Ray drivingRay = new Ray(transform.position + new Vector3(0, 2.8f, 0), roadDectector);
        Debug.DrawRay(transform.position + new Vector3(0, 2.8f, 0), roadDectector * 8, Color.yellow);
        
        if (Physics.Raycast(drivingRay, out hit))
        {
            GameObject objectFound = hit.transform.gameObject;

            if (objectFound.name.Equals("road_collider"))
            {
                if (objectFound.GetComponent<RoadStatus>() != null && objectFound.GetComponent<RoadStatus>().isRoadOccupied(gameObject.GetInstanceID()))
                {
                    Debug.Log("Stopping at road cross a vehicle is crossing");
                    speed = 0;
                }
                else
                {
                    Debug.Log("Cross is ok to go");
                    speed = 5;
                }

            }
            Debug.Log(objectFound.tag + " with name: " + objectFound.name);
        }
    }

    private void MoveVehicle()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
