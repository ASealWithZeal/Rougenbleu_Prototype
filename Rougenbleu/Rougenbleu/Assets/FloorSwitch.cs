using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SwitchColor
{
    Red = 0,
    Blue
}

public class FloorSwitch : MonoBehaviour
{
    public SwitchColor switchColor = SwitchColor.Red;
    public Material[] switchMats;
    public MeshRenderer meshRenderer;

    public void InstantiateSwitch(SwitchColor color)
    {
        switchColor = color;

        switch (switchColor)
        {
            case SwitchColor.Red:
                meshRenderer.material = switchMats[0];
                switchColor = SwitchColor.Red;
                break;
            case SwitchColor.Blue:
                meshRenderer.material = switchMats[1];
                switchColor = SwitchColor.Blue;
                break;
        }
    }

    public void ChangeColor()
    {
        switch (switchColor)
        {
            case SwitchColor.Red:
                meshRenderer.material = switchMats[1];
                switchColor = SwitchColor.Blue;
                break;
            case SwitchColor.Blue:
                meshRenderer.material = switchMats[0];
                switchColor = SwitchColor.Red;
                break;
        }
    }
}
