using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    private const int VIEW_DISTANCE = 8;
    private enum TURN { LEFT, RIGHT, STRAIGHT, BACK };
    private const int DETECTION_DISTANCE = 2;
    [SerializeField] private int maxSpeed = 9;
    [SerializeField] private int currentSpeed = 6;
    [SerializeField] private bool executingCoroutine = false;
    public bool loggerEnabled;
    public LayerMask roadDetectorLayer;
    private readonly Logger logger = new Logger(Debug.unityLogger);

    void Start()
    {
        logger.logEnabled = loggerEnabled;
    }

    void Update()
    {
        DetectRoadAhead();
        MoveVehicle();
        DetectObstacles();
    }

    private void DebugRays()
    {
        Vector3 ray1 = transform.forward * DETECTION_DISTANCE;
        ray1.y = ray1.y - 0.6f;
        Debug.DrawRay(transform.position + new Vector3(0, 2.8f, 0), ray1 * 8, Color.cyan);

    }

    private void DetectObstacles()
    {
        if (!executingCoroutine)
        {
            Vector3 collisionDetector = transform.forward * VIEW_DISTANCE;
            RaycastHit hit;
            Ray drivingRay = new Ray(transform.position + new Vector3(0, 2f, 0), collisionDetector);
            Debug.DrawRay(transform.position + new Vector3(0, 1f, 0), collisionDetector, Color.red);

            if (Physics.Raycast(drivingRay, out hit, VIEW_DISTANCE))
            {
                GameObject objectFound = hit.transform.gameObject;
                if (! objectFound.CompareTag("Wall")) currentSpeed = 0;
            }
            else
            {
                currentSpeed = maxSpeed;
            }
        }
    }

    private GameObject DetectRoadAhead()
    {
        GameObject objectFound = null;
        Vector3 roadDectector = transform.forward * DETECTION_DISTANCE;
        Vector3 fromDetector = transform.position + new Vector3(0, 2.8f, 3);
        roadDectector.y -= 0.7f; ;

        RaycastHit hit;
        Ray drivingRay = new Ray(transform.position + new Vector3(0, 2f, 0), roadDectector);
        Debug.DrawRay(transform.position + new Vector3(0, 2f, 0), roadDectector * 8, Color.yellow);

        if (Physics.Raycast(drivingRay, out hit, 20, roadDetectorLayer)) // TODO chequear solo el suelo
        {
            objectFound = hit.transform.gameObject;

            if (objectFound.GetComponent<CrossRoad>() != null)
            {
                if (!executingCoroutine) // TO-DO check to move this to function level somehow
                {
                    CrossRoad crossRoad = objectFound.GetComponent<CrossRoad>();
                    logger.Log(transform.name + "Found a crossroad, taking decisions.");

                    if (crossRoad.IsRoadOccupied(gameObject.GetInstanceID()))
                    {
                        // Coroutine to wait and mark as turning
                        logger.Log("Stopping at road cross a vehicle is crossing");
                        StartCoroutine(WaitAtCrossRoad(crossRoad));
                    }
                    else
                    {
                        // Choose to go straight, left or right
                        CrossRoad.DIRECTION_FROM directionFrom = GetMyDirectionFrom();
                        List<CrossRoad.DIRECTION_FROM> possibleDirections = crossRoad.GetPossibleDirections(directionFrom);
                        int randomChoose = UnityEngine.Random.Range(0, possibleDirections.Count);
                        CrossRoad.DIRECTION_FROM directionTo = possibleDirections[randomChoose];
                        TURN turn = GetTurn(directionFrom, directionTo);
                        switch (turn)
                        {
                            case TURN.STRAIGHT:
                                // Go straight
                                logger.Log("Continuing straight");
                                StartCoroutine(CrossStraight(crossRoad));
                                break;
                            case TURN.LEFT:
                                // Turn left
                                logger.Log("Lets go left");
                                StartCoroutine(GoLeft(crossRoad));
                                break;
                            case TURN.RIGHT:
                                // Turn right
                                logger.Log("Lets go right");
                                StartCoroutine(GoRight(crossRoad));
                                break;
                            case TURN.BACK:
                                // Turn back: THIS IS NOT ALLOWED AS CROSS TAKES OUT THE ORIGIN
                                logger.Log("Found a wall doing a 180 turn.");
                                StartCoroutine(MakeUTurnOnCross(crossRoad));
                                break;
                            default:
                                // Go straight
                                logger.Log("[Default] Continuing straight");
                                StartCoroutine(CrossStraight(crossRoad));
                                break;
                        }
                    }
                }
            }

            if (objectFound.CompareTag("Wall"))
            {
                if (!executingCoroutine)
                {
                    logger.Log("Found a wall doing a 180 turn.");
                    StartCoroutine(TurnBack());
                }
            }
        }

        return objectFound;
    }

    private TURN GetTurn(
        CrossRoad.DIRECTION_FROM directionFrom,
        CrossRoad.DIRECTION_FROM directionTo)
    {
        switch (directionFrom)
        {
            case CrossRoad.DIRECTION_FROM.NORTH:
                switch (directionTo)
                {
                    case CrossRoad.DIRECTION_FROM.NORTH:
                        return TURN.BACK;
                    case CrossRoad.DIRECTION_FROM.SOUTH:
                        return TURN.STRAIGHT;
                    case CrossRoad.DIRECTION_FROM.EAST:
                        return TURN.LEFT;
                    case CrossRoad.DIRECTION_FROM.WEST:
                        return TURN.RIGHT;
                    default:
                        throw new Exception("Unhandled enum option");
                }
            case CrossRoad.DIRECTION_FROM.SOUTH:
                switch (directionTo)
                {
                    case CrossRoad.DIRECTION_FROM.NORTH:
                        return TURN.STRAIGHT;
                    case CrossRoad.DIRECTION_FROM.SOUTH:
                        return TURN.BACK;
                    case CrossRoad.DIRECTION_FROM.EAST:
                        return TURN.RIGHT;
                    case CrossRoad.DIRECTION_FROM.WEST:
                        return TURN.LEFT;
                    default:
                        throw new Exception("Unhandled enum option");
                }
            case CrossRoad.DIRECTION_FROM.EAST:
                switch (directionTo)
                {
                    case CrossRoad.DIRECTION_FROM.NORTH:
                        return TURN.RIGHT;
                    case CrossRoad.DIRECTION_FROM.SOUTH:
                        return TURN.LEFT;
                    case CrossRoad.DIRECTION_FROM.EAST:
                        return TURN.BACK;
                    case CrossRoad.DIRECTION_FROM.WEST:
                        return TURN.STRAIGHT;
                    default:
                        throw new Exception("Unhandled enum option");
                }
            case CrossRoad.DIRECTION_FROM.WEST:
                switch (directionTo)
                {
                    case CrossRoad.DIRECTION_FROM.NORTH:
                        return TURN.LEFT;
                    case CrossRoad.DIRECTION_FROM.SOUTH:
                        return TURN.RIGHT;
                    case CrossRoad.DIRECTION_FROM.EAST:
                        return TURN.STRAIGHT;
                    case CrossRoad.DIRECTION_FROM.WEST:
                        return TURN.BACK;
                    default:
                        throw new Exception("Unhandled enum option");
                }
            default:
                throw new Exception("Unhandled enum option");
        }
    }

    private CrossRoad.DIRECTION_FROM GetMyDirectionFrom()
    {
        float myRotation = transform.eulerAngles.y;
        myRotation = myRotation < 0 ? myRotation * -1 : myRotation;
        myRotation = myRotation % 360;

        if (-10 < myRotation && myRotation < 15) //0 o -360
        {
            return CrossRoad.DIRECTION_FROM.SOUTH;
        }
        else if (70 < myRotation && myRotation < 110) //90
        {
            return CrossRoad.DIRECTION_FROM.WEST;
        }
        else if (160 < myRotation && myRotation < 200) //180
        {
            return CrossRoad.DIRECTION_FROM.NORTH;
        }
        else if (250 < myRotation && myRotation < 290) //270
        {
            return CrossRoad.DIRECTION_FROM.EAST;
        }
        else
        {
            throw new Exception("There is something wrong with the direction evaluation");
        }
    }

    IEnumerator WaitAtCrossRoad(CrossRoad crossRoad)
    {
        executingCoroutine = true;
        currentSpeed = 0;
        while (crossRoad.IsRoadOccupied(gameObject.GetInstanceID()))
        {
            yield return new WaitForSeconds(0.5f); // What if two cars enter at the same time
        }
        currentSpeed = maxSpeed;
        executingCoroutine = false;
    }

    IEnumerator TurnBack()
    {
        executingCoroutine = true;
        currentSpeed = 5;
        Vector3 currentRotation = transform.rotation.eulerAngles;
        Vector3 rotation = new Vector3(currentRotation.x, currentRotation.y - 180, currentRotation.z);

        // Do turn
        var fromAngle = transform.rotation;
        var toAngle = Quaternion.Euler(rotation);
        while (Quaternion.Angle(fromAngle, toAngle) > 1)
        {
            transform.Rotate(new Vector3(0, -1.1f, 0));
            fromAngle = transform.rotation;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        transform.rotation = Quaternion.Euler(currentRotation.x, NormalizeDegrees(currentRotation.y - 180), currentRotation.z);

        // Go straight to exit cross
        yield return new WaitForSeconds(1);
        currentSpeed = maxSpeed;
        executingCoroutine = false;
    }

    IEnumerator MakeUTurnOnCross(CrossRoad crossRoad)
    {
        executingCoroutine = true;
        crossRoad.OccupiedByMe(gameObject.GetInstanceID());
        currentSpeed = 5;
        Vector3 currentRotation = transform.rotation.eulerAngles;
        Vector3 rotation = new Vector3(currentRotation.x, currentRotation.y - 180, currentRotation.z);

        // Do turn
        var fromAngle = transform.rotation;
        var toAngle = Quaternion.Euler(rotation);
        while (Quaternion.Angle(fromAngle, toAngle) > 1)
        {
            transform.Rotate(new Vector3(0, -1f, 0));
            fromAngle = transform.rotation;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        transform.rotation = Quaternion.Euler(currentRotation.x, NormalizeDegrees(currentRotation.y - 180), currentRotation.z);


        // Go straight to exit cross
        yield return new WaitForSeconds(2);
        currentSpeed = maxSpeed;
        executingCoroutine = false;
        crossRoad.FreeCrossRoad();
    }

    IEnumerator CrossStraight(CrossRoad crossRoad)
    {
        executingCoroutine = true;
        crossRoad.OccupiedByMe(gameObject.GetInstanceID());
        currentSpeed = maxSpeed;
        // Go straight to exit cross
        yield return new WaitForSeconds(4);
        executingCoroutine = false;
        crossRoad.FreeCrossRoad();
    }

    IEnumerator GoLeft(CrossRoad crossRoad)
    {
        executingCoroutine = true;
        crossRoad.OccupiedByMe(gameObject.GetInstanceID());
        Vector3 currentRotation = transform.rotation.eulerAngles;
        Vector3 rotation = new Vector3(currentRotation.x, currentRotation.y - 90, currentRotation.z);
        currentSpeed = 5;
        // Go straigth
        yield return new WaitForSeconds(3.3f);

        // Do turn
        var fromAngle = transform.rotation;
        var toAngle = Quaternion.Euler(rotation);
        while (Quaternion.Angle(fromAngle, toAngle) > 1)
        {
            transform.Rotate(new Vector3(0, -1f, 0));
            fromAngle = transform.rotation;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        transform.rotation = Quaternion.Euler(currentRotation.x, NormalizeDegrees(currentRotation.y - 90), currentRotation.z);

        // Go straight to exit cross
        yield return new WaitForSeconds(2);
        currentSpeed = maxSpeed;
        executingCoroutine = false;
        crossRoad.FreeCrossRoad();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            logger.Log("collided with " + collision.transform.name);
            collision.collider.transform.SetParent(transform);
        } 
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            logger.Log("left " + collision.transform.name);
            collision.collider.transform.SetParent(null);
        }
    }


    IEnumerator GoRight(CrossRoad crossRoad)
    {
        executingCoroutine = true;
        crossRoad.OccupiedByMe(gameObject.GetInstanceID());
        Vector3 currentRotation = transform.rotation.eulerAngles;
        //float normalizedAngle = NormalizeDegrees(currentRotation.y + 90);
        Vector3 rotation = new Vector3(currentRotation.x, currentRotation.y + 90, currentRotation.z);
        currentSpeed = 5;
        // Go straigth
        yield return new WaitForSeconds(1.8f);

        var fromAngle = transform.rotation;
        var toAngle = Quaternion.Euler(rotation);
        while (Quaternion.Angle(fromAngle, toAngle) > 0)
        {
            transform.Rotate(new Vector3(0, 1f, 0));
            fromAngle = transform.rotation;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        transform.rotation = Quaternion.Euler(currentRotation.x, NormalizeDegrees(currentRotation.y + 90), currentRotation.z);

        // Go straight to exit cross
        yield return new WaitForSeconds(1.5f);
        executingCoroutine = false;
        currentSpeed = maxSpeed;
        crossRoad.FreeCrossRoad();
    }

    private float NormalizeDegrees(float angle)
    {
        angle = angle > 0 ? angle % 360 : 1 * angle % 360; 

        /*if (angle % 360 < 5 || angle % 0 < 5)
        {
            angle = 0;
        } else if ( angle % 270 < 5)
        {
            angle = 270;
        } else if (angle % 180 < 5)
        {
            angle = 180;
        } else if ( angle % 90 < 5)
        {
            angle = 90;
        }*/

        return angle;
    }

    private void MoveVehicle()
    {
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
    }
}
