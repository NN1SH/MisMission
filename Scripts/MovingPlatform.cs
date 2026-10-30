using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class MovingPlatform : MonoBehaviour
{
    public Transform pointA; // First point of the platform
    public Transform pointB; // Second point of the platform
    public float speed = 2f; // Speed of the platform movement

    private Vector3 nextPosition; // Current target position for the platform
    // Start is called before the first frame update
    void Start()
    {
        nextPosition = pointB.position; // Start moving towards point B
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, nextPosition, speed * Time.deltaTime);

        if (transform.position == nextPosition)
        {
            nextPosition = (nextPosition == pointA.position) ? pointB.position : pointA.position;
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.gameObject.transform.parent = transform; // Make the player a child of the platform
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.gameObject.transform.parent = null; // Remove the player from the platform
        }
    }
}
