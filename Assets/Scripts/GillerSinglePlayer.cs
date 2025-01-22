using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GillerSinglePlayer : MonoBehaviour
{

    private CharacterController controller;

    Vector3 _velocity;

    [SerializeField]
    float SinkAcceleration;

    [SerializeField]
    float UpSwimBoost;

    [SerializeField]
    float HorizontalSwimSpeed;

    private void Start()
    {
        controller = gameObject.AddComponent<CharacterController>();
    }

    void Update()
    {

        float xInput = Input.GetAxis("Horizontal");
        //float yInput = Input.GetAxis("Vertical");

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

    }
}
