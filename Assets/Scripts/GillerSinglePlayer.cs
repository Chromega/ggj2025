using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GillerSinglePlayer : MonoBehaviour
{

    private CharacterController controller;

    //Variables for SWIMMING
    Vector3 _velocity;

    [SerializeField]
    float SinkAcceleration;

    [SerializeField]
    float UpSwimBoost;

    [SerializeField]
    float HorizontalSwimSpeed;
    //Variables for SWIMMING

    //Variables for PUSHING
    string obstacleTag = "Obstacle";
    float pushForce = 2f;
    float pushDuration = 0.2f;

    Vector3 pushDirection;
    bool isBeingPushed = false;
    float pushTimer = 0f;
    //Variables for PUSHING


    //Checks to see if player has entered obstacle
    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag(obstacleTag))
        {

            pushDirection = (transform.position - other.transform.position).normalized;


            isBeingPushed = true;
            pushTimer = pushDuration;
        }
    }

    private void Start()
    {
        controller = gameObject.AddComponent<CharacterController>();
    }

    void Update()
    {

        float xInput = Input.GetAxis("Horizontal");


        _velocity.x = xInput * HorizontalSwimSpeed;
        _velocity += new Vector3(0, SinkAcceleration, 0) * Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _velocity.y = UpSwimBoost;
        }
 

        Vector3 newPosition = transform.position + _velocity * Time.deltaTime;
        if (newPosition.y < 0)
            newPosition.y = 0;
        transform.position = newPosition;


        // Pushes player away from obstacle
        if (isBeingPushed)
        {

            float pushStep = (pushForce / pushDuration) * Time.deltaTime;


            transform.position += pushDirection * pushStep;


            pushTimer -= Time.deltaTime;

            if (pushTimer <= 0f)
            {
                isBeingPushed = false;
            }
        }

    }
}
