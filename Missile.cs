using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Missile : MonoBehaviour
{

    public int force = 100;
    public Rigidbody missileBody;
    public Vector3 direction;
    private readonly Logger logger = new Logger(Debug.unityLogger);
    public bool loggerEnabled = false;
    private ParticleSystem explosion;

    public void Setup(Vector3 shootDirection) // Move to start, unnecesary call
    {
        logger.logEnabled = loggerEnabled;
        direction = shootDirection;
        missileBody = GetComponent<Rigidbody>();
        missileBody.AddForce(transform.forward * force, ForceMode.Impulse);
        Destroy(gameObject, 2f);
        explosion = transform.Find("Explosion").GetComponent<ParticleSystem>();
    }

    private void OnTriggerEnter(Collider other)
    {
        logger.Log("Misile HIT!");
        explosion.Play();
        //Destroy(gameObject);
    }
}
