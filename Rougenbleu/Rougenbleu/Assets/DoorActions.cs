using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorType
{
    Locked = 0,
    Sealed
}

public class DoorActions : MonoBehaviour
{
    public DoorType doorType = DoorType.Sealed;
    public Animator doorAnimator = null;

    public void OpenDoor()
    {
        if (doorType.Equals(DoorType.Sealed))
            OpenSealedDoor();
    }

    private void OpenSealedDoor()
    {
        doorAnimator.SetBool("Open", true);
    }

    public void EndDoorAction()
    {
        Destroy(gameObject);
    }
}
