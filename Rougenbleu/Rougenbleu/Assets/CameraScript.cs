using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    // The player character, which the camera follows
    public Transform playerTransform;
    public Transform thisTransform;

    // Bounds for the camera
    public Vector2[] bounds =
    {
        new Vector2(0, 0),      // Bottom-Left
        new Vector2(34, 0),     // Bottom-Right
        new Vector2(0, 34),     // Top-Left
        new Vector2(34, 34),    // Top-Right
    };

    // Start is called before the first frame update
    void Start()
    {
        thisTransform = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        FollowPlayer();
    }

    private void FollowPlayer()
    {
        Vector3 targetPos = new Vector3(playerTransform.position.x, thisTransform.position.y, playerTransform.position.z);
        thisTransform.position = Vector3.Lerp(thisTransform.position, targetPos, Time.deltaTime * 10);
    }
}
