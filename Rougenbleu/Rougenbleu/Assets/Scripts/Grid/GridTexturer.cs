using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New GridTexturer", menuName = "ScriptableObjects/GridTexturer", order = 1)]
public class GridTexturer : ScriptableObject
{
    [Header("Floor Textures")]
    public Material basicFloor;
    public Material basicFloorWall;

    [Header("Basic Wall Textures")]
    public Material bottomWall;
    public Material topWall;
    public Material bar;
    public Material wallEdge180;

    [Header("Inward Corner Wall Textures")]
    public Material bottomInnerWall;
    public Material topInnerWall;
    public Material wallEdge90;
    public Material wallCornerBar;

    [Header("Outward Corner Wall Textures")]
    public Material angleWall_75;
    public Material angleWall_5;
    public Material angleWall_32;
    public Material angleWall_1875;

    public Material flatWall_75;
    public Material flatWall_375;
    public Material flatWall_5;
    public Material flatWall_32;
    public Material flatWall_25;

    public Material wallEdge270;
    public Material halfFloorWall1;
    public Material halfFloorWall2;

    [Header("Ceiling Textures")]
    public Material basicCeiling;

    [Header("Door Textures")]
    public Material doorEdge;
    public Material doorTop;

    [Header("Pit Textures")]
    public Material pit;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
