using System.Collections;
using UnityEngine;

public class GillerCollision : MonoBehaviour
{
    public string obstacleTag = "Obstacle";

    public float disappearDuration = 4;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the other object has the "Obstacle" tag
        if (other.CompareTag(obstacleTag))
        {
            gameObject.SetActive(false);
            Invoke("ReactivateObject", disappearDuration);
        }
    }

    private void ReactivateObject()
    {
        gameObject.SetActive(true);
    }
}