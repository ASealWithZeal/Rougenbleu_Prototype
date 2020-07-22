using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FacingDirection
{
    Up = 0,
    Down,
    Left,
    Right,
    UpLeft,
    UpRight,
    DownLeft,
    DownRight
}

public enum MoveState
{
    Walking = 0,
    Rolling
}

public class PlayerMovement : MonoBehaviour
{
    private CharacterController controller;
    private FacingDirection direction;
    private Vector3[] directions =
    {
        new Vector3(0, 0, 1), new Vector3(0, 0, -1), new Vector3(-1, 0, 0), new Vector3(1, 0, 0),
        new Vector3(-1, 0, 1), new Vector3(1, 0, 1), new Vector3(-1, 0, -1), new Vector3(1, 0, -1)
    };

    private MoveState moveState = MoveState.Walking;
    public float speed = 0.2f;
    public float maxRollTime = 0.2f; private float rollTimeCounter = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        CheckRollCooldown();
        CheckInput();
    }

    private void CheckRollCooldown()
    {
        if (moveState.Equals(MoveState.Rolling))
        {
            rollTimeCounter += Time.deltaTime;

            if (rollTimeCounter >= maxRollTime)
            {
                moveState = MoveState.Walking;
                rollTimeCounter = 0;
                speed /= 2.5f;
            }
        }
    }

    private void CheckInput()
    {
        // Vector used to store directional information
        Vector3 newDirection = new Vector3();

        // If the player is currently walking, do the following:
        if (moveState.Equals(MoveState.Walking))
        {
            // Walk
            if (Input.GetKey(KeyCode.W))
                newDirection += directions[0];
            else if (Input.GetKey(KeyCode.S))
                newDirection += directions[1];
            if (Input.GetKey(KeyCode.A))
                newDirection += directions[2];
            else if (Input.GetKey(KeyCode.D))
                newDirection += directions[3];

            // Changes the player's facing direction to match the new direction
            GetFacingDirection(newDirection);

            // Roll
            if (Input.GetMouseButtonDown(1) && newDirection != new Vector3())
            {
                speed *= 2.5f;
                moveState = MoveState.Rolling;
            }
        }

        // If the player is currently rolling, do the following:
        else if (moveState.Equals(MoveState.Rolling))
        {
            // Forces the player to move in the current facing direction
            newDirection += directions[(int)direction];

            // Allows slight deviations from the current direction
            if (Input.GetKey(KeyCode.W))
                newDirection += directions[0] / 10.0f;
            else if (Input.GetKey(KeyCode.S))
                newDirection += directions[1] / 10.0f;
            if (Input.GetKey(KeyCode.A))
                newDirection += directions[2] / 10.0f;
            else if (Input.GetKey(KeyCode.D))
                newDirection += directions[3] / 10.0f;
        }

        MoveInDirection(newDirection);
    }

    private void GetFacingDirection(Vector3 newDirection)
    {
        for (int i = 0; i < directions.Length; ++i)
            if (newDirection.Equals(directions[i]))
            {
                direction = (FacingDirection)i;
                continue;
            }
    }

    private void MoveInDirection(Vector3 moveDirection)
    {
        controller.Move(moveDirection * speed * Time.deltaTime);
        //transform.position += (moveDirection * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("ENTER");
        if (other.gameObject.tag == "Switch")
            other.gameObject.GetComponent<FloorSwitch>().ChangeColor();
    }
}
