using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float distanceTillStartsFollowingX = 5;
    public float distanceTillStartsFollowingY = 2;
    public float xOffset = 0;
    public float yOffset = 0;
    public float interpSpeed = 0.3f;


    bool startFollowingX;
    bool startFollowingY;



    void Update()
    {
        if (startFollowingX)
        {
            if (Mathf.Abs(transform.position.x - (target.position.x + xOffset)) > 0.25f)
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(target.position.x, transform.position.y, transform.position.z), interpSpeed * Time.deltaTime);
            }
            else
            {
                startFollowingX = false;
            }
        }
        else
        {
            if (Mathf.Abs(transform.position.x - (target.position.x + yOffset)) > distanceTillStartsFollowingX)
            {
                startFollowingX = true;
            }
        }
        if (startFollowingY)
        {
            if (Mathf.Abs(transform.position.y - target.position.y) > 0.25f)
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, target.position.y, transform.position.z), interpSpeed * Time.deltaTime);
            }
            else
            {
                startFollowingY = false;
            }
        }
        else
        {
            if (Mathf.Abs(transform.position.y - target.position.y) > distanceTillStartsFollowingY)
            {
                startFollowingY = true;
            }
        }

       
      
    }
}
