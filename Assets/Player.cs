using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public Vector3 destinataion;
    public Vector3 moveDirection;
    private bool hasDestination;
    public float moveSpeed = 1.0f;
    public float turnSpeed = 10.0f;
    public float snapDistance = 0.5f;
    public float targetAngle;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (hasDestination)
        {
            Vector3 currentPosition = transform.position;
            if ((currentPosition - destinataion).magnitude < snapDistance)
            {
                transform.position = destinataion;
                hasDestination = false;
            }
            else
            {
                Vector3 target = moveDirection * moveSpeed + currentPosition;
                transform.position = Vector3.Lerp(currentPosition, target, Time.deltaTime);

                targetAngle = Mathf.Atan2( moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
                transform.rotation =
                    Quaternion.Slerp(transform.rotation,
                                     Quaternion.Euler(90, targetAngle, 0),
                                     turnSpeed * Time.deltaTime);
            }
        }

    }

    public void MoveTo( float x, float z)
    {
        Vector3 currentPosition = transform.position;
        destinataion = new Vector3(x, currentPosition.y, z);
        hasDestination = true;
        moveDirection = destinataion - currentPosition;
        moveDirection.Normalize();
    }
}
