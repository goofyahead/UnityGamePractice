using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerTransform : MonoBehaviour
{
    public CharacterController character;
    public float speed = 12f;
    public float gravity = -19.85f;
    public Transform groundDetector;
    public float groundDetectRadius = 0.2f;
    public LayerMask maskCollider;
    public float jumpForce = 200;
    Vector3 gravityVector;
    public bool loggerEnabled;
    private readonly Logger logger = new Logger(Debug.unityLogger);

    void Start()
    {
        logger.logEnabled = loggerEnabled;
    }

    // Update is called once per frame
    void Update()
    {
        // If we are on the ground stop building momentum
        if (Physics.CheckSphere(groundDetector.position, groundDetectRadius, maskCollider))
        {
            Debug.Log("Touchdown!");
            gravityVector.y = -1f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 movement = transform.right * x + transform.forward * z;
        character.Move(movement * speed * Time.deltaTime);

        // Apply gravity
        gravityVector.y += gravity * Time.deltaTime; //(float) Math.Pow(Time.deltaTime, 2)
        character.Move(gravityVector * Time.deltaTime);

        if (Input.GetKeyDown("space"))
        {
            logger.Log("space key was pressed"); 
        }
    }
}
