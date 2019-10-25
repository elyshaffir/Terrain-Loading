using UnityEngine;

public class SmoothMover : MonoBehaviour
{
    private const float MIN_DISTANCE = .2f;

    public int movementSpeed = 100;

    private Vector3 targetPosition;
    private Vector3 movementDirection;

    void Start()
    {
        targetPosition = transform.position;
        movementDirection = new Vector3();
    }

    void Update()
    {
        if (Vector3.Distance(targetPosition, transform.position) > MIN_DISTANCE)
        {
            Vector3 goingToPosition = transform.position + (movementSpeed * movementDirection * Time.deltaTime);
            if (goingToPosition - transform.position != movementDirection)
            {
                transform.position = targetPosition;
            }
            else
            {
                transform.position += movementSpeed * movementDirection * Time.deltaTime;
            }
        }
    }

    public void ChageTargetPosition(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
        movementDirection = targetPosition - transform.position;
    }
}