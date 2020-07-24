using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SwitchEffect
{
    OpenDoor = 0,
    SpawnBridge,
    SpawnChest
}

public class PowerSource : MonoBehaviour
{
    [Header("Type and Effect")]
    public SwitchEffect effect = SwitchEffect.OpenDoor;
    public bool active = false;

    [Header("Color Switches")]
    public FloorSwitch[] connectedSwitches;
    public SwitchColor requiredColor;

    [Header("Affected Objects")]
    public DoorActions door = null;
    public GameObject chest = null;
    public GameObject bridge = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfActive();
    }

    private void CheckIfActive()
    {
        if (!active)
        {
            int count = 0;
            foreach (FloorSwitch fs in connectedSwitches)
            {
                if (fs.switchColor.Equals(requiredColor))
                    count++;
            }
            
            if (count.Equals(connectedSwitches.Length))
                ActivateSwitch();
        }
    }

    private void ActivateSwitch()
    {
        active = true;
        if (effect.Equals(SwitchEffect.OpenDoor))
            door.OpenDoor();
    }
}
