using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class CarMovement : MonoBehaviour
{
    private const int VIEW_DISTANCE = 6;
    private Vector3 AXIS_Y = new Vector3(0, 1, 0);
    private enum DIRECTION {LEFT, RIGHT};
    private const int DETECTION_ANGLE = 15;
    private const int DETECTION_DISTANCE = 2;
    [SerializeField] private int speed = 3;
    [SerializeField] private int ROTATION_SPEED = 50;
    [SerializeField] private bool turning = false;

    void Start()
    {

    }

    void Update()
    {
        GameObject road = DetectRoadAhead();
        //DetectObstacles();
        //DebugRays();
        MoveVehicle();
        Turn(DIRECTION.LEFT, road);
    }

    private void Turn(DIRECTION direction, GameObject around)
    {
        if (!turning)
        {
            var fromAngle = transform.rotation;
            var toAngle = Quaternion.Euler(transform.eulerAngles + new Vector3(0, 90, 0));
            transform.rotation = Quaternion.Slerp(fromAngle, toAngle, 0.005f);
        }
        //StartCoroutine(RotateMe(new Vector3(0, 90, 0)));
        //gameObject.transform.RotateAround(around.transform.position, - AXIS_Y, 1 * Time.deltaTime);
        //gameObject.transform.Rotate(AXIS_Y * ROTATION_SPEED * Time.deltaTime);
    }

    IEnumerator SmoothRotate(Vector3 byAngles, float inTime)
    {
        var fromAngle = transform.rotation;
        var toAngle = Quaternion.Euler(transform.eulerAngles + byAngles);
        for (var t = 0f; t < 1; t += Time.deltaTime / inTime)
        {
            transform.rotation = Quaternion.Slerp(fromAngle, toAngle, t);
            yield return null;
        }
    }

    IEnumerator RotateMe(Vector3 byAngles)
    {
        Debug.Log("Starting co routine");
        turning = true;
        var fromAngle = transform.rotation;
        var toAngle = Quaternion.Euler(transform.eulerAngles + byAngles);
        Debug.Log("Starting difference is: " + Quaternion.Angle(fromAngle, toAngle));
        //while(Quaternion.Angle(fromAngle, toAngle) > 5) {
            Debug.Log("difference is: " + Quaternion.Angle(fromAngle, toAngle));
            transform.rotation = Quaternion.Slerp(fromAngle, toAngle, 0.02f);
            yield return null;
        //}
        Debug.Log("Ending co routine");
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

    private GameObject DetectRoadAhead()
    {
        GameObject objectFound = null;
        Vector3 roadDectector = transform.forward * DETECTION_DISTANCE;
        roadDectector.y -= 0.6f; ;
              
        RaycastHit hit;
        Ray drivingRay = new Ray(transform.position + new Vector3(0, 2.8f, 0), roadDectector);
        Debug.DrawRay(transform.position + new Vector3(0, 2.8f, 0), roadDectector * 8, Color.yellow);
        
        if (Physics.Raycast(drivingRay, out hit))
        {
            objectFound = hit.transform.gameObject;

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
        }

        return objectFound;
    }

    private void MoveVehicle()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
