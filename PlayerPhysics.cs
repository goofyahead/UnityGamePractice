using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerPhysics : MonoBehaviour
{
    public bool loggerEnabled;
    public Transform missilePf;
    public float speed = 5;
    private readonly Logger logger = new Logger(Debug.unityLogger);
    public Rigidbody character;
    private Transform weapon;
    public ForceMode forceMode;
    private Vector3 relativeMovement;
    public float jumpForce = 5;
    public bool canJump = true;
    public Transform groundDetector;
    public float groundDetectRadius = 0.05f;
    public LayerMask maskCollider;

    void Start()
    {
        logger.logEnabled = loggerEnabled;
        character = GetComponent<Rigidbody>();
        weapon = transform.Find("Camera/Weapon").gameObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
        float gravity = character.velocity.y;
        relativeMovement = (transform.right * Input.GetAxis("Horizontal") * speed) + (transform.forward * Input.GetAxis("Vertical") * speed);
        relativeMovement.y = gravity;

        if (Input.GetKeyDown("space") && canJump)
        {
            canJump = false;
            logger.Log("space key was pressed");
            character.AddForce(Vector3.up * jumpForce, forceMode);
        }

        if (Input.GetMouseButtonDown(0))
        {
            logger.Log("Shoot!");
            MouseLookAround cameraScript = GetComponentInChildren<MouseLookAround>();
            Transform bullet = Instantiate(missilePf, weapon.position, Quaternion.Euler(cameraScript.GetCameraRotation()));
            Missile missile = bullet.GetComponent<Missile>();
            missile.Setup(cameraScript.GetCameraRotation());
        }

        if (Physics.CheckSphere(groundDetector.position, groundDetectRadius, maskCollider))
        {
            Debug.Log("Ready to Jump");
            canJump = true;
        }
    }
    private void FixedUpdate()
    {
        character.velocity = relativeMovement;
    }
}
