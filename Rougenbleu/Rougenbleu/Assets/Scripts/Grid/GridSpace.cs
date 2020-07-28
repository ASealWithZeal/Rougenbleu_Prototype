using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpaceType
{
    Ground = 0,
    Pit,
    Wall,
    InwardWall,
    OutwardWall,
    Ceiling,
    Door
}

public class GridSpace : MonoBehaviour
{
    public Vector2Int coordinates = new Vector2Int(0, 0);
    public SpaceType type = SpaceType.Ground;
    public Material[] baseMaterials;
    // 1. Ground
    // 2. Water
    // 3. Wall
    // 4. Ceiling
    public Color[] spaceColors;

    [Header("Tile Objects")]
    public GameObject wall;
    public GameObject box;
    public GameObject pit;
    public GameObject inwardCornerWall;
    public GameObject outwardCornerWall;
    public GameObject switchSpace;
    public GameObject ceilingTile;
    public GameObject rightDoorway;
    public GameObject leftDoorway;
    public GameObject fullDoorway;

    [Header("Other")]
    public bool isSwitch = false;
    public bool lockTextures = false;

    // Instantiates grid space values
    public void Instantiate(Color pixelColor, Color[] surroundingPixels, Vector2Int newCoordinates)
    {
        // Sets coordinate information
        coordinates = newCoordinates;

        // Determines the space's type based on the pixel's color
        if (pixelColor.Equals(spaceColors[0]))
        {
            type = SpaceType.Ground;
            for (int i = 0; i < 9; ++i)
            {
                if (surroundingPixels[i].Equals(spaceColors[2]))
                {
                    for (int j = 2; j < 6; ++j)
                        transform.GetChild(0).GetChild(j).gameObject.SetActive(true);
                    continue;
                }
            }
        }

        // Wall
        else if (pixelColor.Equals(spaceColors[1]))
        {
            type = SpaceType.Wall;
            CheckSurroundingWalls(surroundingPixels);
            //GetComponent<Renderer>().material = baseMaterials[0];
            //
            //for (int i = 0; i < transform.childCount; ++i)
            //    transform.GetChild(i).GetComponent<Renderer>().material = baseMaterials[1];
        }

        // Pit
        else if (pixelColor.Equals(spaceColors[2]))
        {
            type = SpaceType.Pit;

            // Destroys the box and replaces it with a pit
            DestroyImmediate(transform.GetChild(0).gameObject);
            GameObject g = Instantiate(pit, new Vector3(coordinates.x, -1, coordinates.y), Quaternion.identity, transform);
        }

        // Ceiling
        else if (pixelColor.Equals(spaceColors[3]))
        {
            type = SpaceType.Ceiling;

            // Checks to see whether or not the ceiling object is bordered by any walls or doors
            // Because these tiles have their own "ceiling" pieces, none should draw if they exist
            bool canDrawCeiling = true;
            for (int i = 0; i < surroundingPixels.Length; ++i)
                if (surroundingPixels[i].Equals(spaceColors[1]) || surroundingPixels[i].Equals(spaceColors[6]))
                    canDrawCeiling = false;

            // Turns all floor tiles black, for use in doors and other "void" areas
            transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = baseMaterials[2];
            if (canDrawCeiling)
            {
                Instantiate(ceilingTile, new Vector3(coordinates.x, ceilingTile.transform.position.y, coordinates.y), ceilingTile.transform.rotation, transform);

                // Destroys the floor, which no longer serves a purpose
                //for (int i = 0; i < 5; ++i)
                //    DestroyImmediate(transform.GetChild(1).gameObject);
            }
        }

        // Switch
        else if (pixelColor.Equals(spaceColors[4]) || pixelColor.Equals(spaceColors[5]))
        {
            type = SpaceType.Ground;
            GameObject g = Instantiate(switchSpace, new Vector3(coordinates.x, 0, coordinates.y), Quaternion.identity, transform);

            // Destroys the space's old top space, which is now unecessary (MAY RETURN WHEN I GET BETTER SWITCHES)
            DestroyImmediate(transform.GetChild(0).GetChild(0).gameObject);

            if (pixelColor.Equals(spaceColors[4]))
                g.GetComponent<FloorSwitch>().InstantiateSwitch(SwitchColor.Red);
            else
                g.GetComponent<FloorSwitch>().InstantiateSwitch(SwitchColor.Blue);
        }

        // Doorway
        else if (pixelColor.Equals(spaceColors[6]))
        {
            type = SpaceType.Door;
            CheckSurroundingDoors(surroundingPixels);
            //GetComponent<Renderer>().material = baseMaterials[0];
            //
            //for (int i = 0; i < transform.childCount; ++i)
            //    transform.GetChild(i).GetComponent<Renderer>().material = baseMaterials[1];
        }

        else { type = SpaceType.Ground; }
    }

    #region Wall Creation
    private void CheckSurroundingWalls(Color[] surroundingColors)
    {
        // Top, Down, Left, Right
        bool[] directionBools = new bool[9];

        // Checks the surrounding tiles for whether or not they contain walls
        for (int i = 0; i < surroundingColors.Length; ++i)
            if (surroundingColors[i] == spaceColors[1] || surroundingColors[i] == spaceColors[6])
                directionBools[i] = true;

        // Sets up more information for the walls
        SetUpWall(directionBools, surroundingColors);
    }

    private void SetUpWall(bool[] directions, Color[] surroundingColors)
    {
        // HERE WE GO
        int numWalls = 0;
        for (int i = 0; i < 4; ++i)
        {
            if (directions[(i * 2) + 1])
                numWalls++;
        }

        // Checks if there are walls above and below the current one, but NOT to either side
        if (directions[1] && !directions[3] && !directions[5] && directions[7] && !CheckForDiagonalWalls(directions, surroundingColors))
        {
            // Creates a left-facing wall
            if (surroundingColors[3] != spaceColors[3] && (surroundingColors[5] == new Color() || surroundingColors[5] == spaceColors[3]))
                CreateWall(true, 0, Quaternion.Euler(0, 90, 0), surroundingColors);

            // Creates a right-facing wall
            else if (surroundingColors[5] != spaceColors[3] && (surroundingColors[3] == new Color() || surroundingColors[3] == spaceColors[3]))
                CreateWall(true, 0, Quaternion.Euler(0, -90, 0), surroundingColors);

            // Creates a straight wall
            else
                CreateWall(false, 0, Quaternion.Euler(0, 0, 0), surroundingColors);
        }

        // Checks if there are walls to the left and right of the current one, but NOT above or below
        else if (!directions[1] && directions[3] && directions[5] && !directions[7] && !CheckForDiagonalWalls(directions, surroundingColors))
        {
            // Creates an up-facing wall
            if (surroundingColors[1] != null && (surroundingColors[7] == new Color() || surroundingColors[7] == spaceColors[3]))
                CreateWall(true, 0, Quaternion.Euler(0, 0, 0), surroundingColors);

            // Creates a down-facing wall
            else if (surroundingColors[7] != null && (surroundingColors[1] == new Color() || surroundingColors[1] == spaceColors[3]))
                CreateWall(true, 0, Quaternion.Euler(0, 180, 0), surroundingColors);

            // Creates a straight wall
            else
                CreateWall(false, 0, Quaternion.Euler(0, 0, 0), surroundingColors);
        }

        // If there are two walls, check to determine their setup
        else if (numWalls == 2 && CheckConnectingWalls(directions))
        {
            bool borderedByCeiling = false;
            for (int i = 0; i < 4; ++i)
                if (surroundingColors[(i * 2) + 1].Equals(spaceColors[3]) || surroundingColors[(i * 2) + 1].Equals(new Color()))
                {
                    borderedByCeiling = true;
                    continue;
                }

            if (borderedByCeiling)
            {
                CreateWall(true, 1, DetermineAngleFromSurroundings(1, surroundingColors), surroundingColors);
                type = SpaceType.InwardWall;
            }

            else
            {
                CreateWall(true, 2, DetermineAngleFromSurroundings(2, surroundingColors), surroundingColors);
                type = SpaceType.OutwardWall;
            }
        }

        else if (CheckForDiagonalWalls(directions, surroundingColors))
        {
            // Rotates the "empty" space to allow for easy texturing later on
            if (surroundingColors[1].Equals(spaceColors[3]))
                transform.rotation = Quaternion.Euler(0, 180, 0);
            else if (surroundingColors[3].Equals(spaceColors[3]))
                transform.rotation = Quaternion.Euler(0, -90, 0);
            else if (surroundingColors[5].Equals(spaceColors[3]))
                transform.rotation = Quaternion.Euler(0, 90, 0);
            else if (surroundingColors[7].Equals(spaceColors[3]))
                transform.rotation = Quaternion.Euler(0, 0, 0);

            CheckForAdjacentPits(surroundingColors, 3);
        }
    }

    private bool CheckConnectingWalls(bool[] directions)
    {
        // Ensures that a diagonal wall cannot exist between two adjacent walls
        if (!(directions[1] && !directions[3] && !directions[5] && directions[7]) && !(!directions[1] && directions[3] && directions[5] && !directions[7]))
            return true;

        return false;
    }

    private bool CheckForDiagonalWalls(bool[] directions, Color[] surroundingColors)
    {
        bool returnBool = false;
        int index = 0;

        for (int i = 0; i < 4; ++i)
        {
            index = (i * 2) + 1;
            // Checks the diagonal spaces
            if (directions[index].Equals(true))
            {
                if (index.Equals(1) || index.Equals(7))
                    returnBool = CheckForAdjacentDiagonalWalls(surroundingColors, index, true);
                else
                    returnBool = CheckForAdjacentDiagonalWalls(surroundingColors, index, false);
            }

            // If the diagonals ever return a positive value, return "true" immediately
            if (returnBool)
                return true;
        }

        return false;
    }

    // Checks to determine if an adjacent wall is bordered by a wall (diagonal to the first) and a non-ceiling tile
    private bool CheckForAdjacentDiagonalWalls(Color[] surroundingColors, int index, bool sideways)
    {
        // Checks horizontally
        if (sideways)
        {
            if (surroundingColors[index + 1].Equals(spaceColors[1]) && (surroundingColors[index - 1] != spaceColors[3] && surroundingColors[index - 1] != new Color()))
                return true;

            else if (surroundingColors[index - 1].Equals(spaceColors[1]) && (surroundingColors[index + 1] != spaceColors[3] && surroundingColors[index + 1] != new Color()))
                return true;
        }

        // Checks vertically
        else
        {
            if (surroundingColors[index + 3].Equals(spaceColors[1]) && (surroundingColors[index - 3] != spaceColors[3] && surroundingColors[index - 3] != new Color()))
                return true;

            else if (surroundingColors[index - 3].Equals(spaceColors[1]) && (surroundingColors[index + 3] != spaceColors[3] && surroundingColors[index + 3] != new Color()))
                return true;
        }

        return false;
    }

    private Quaternion DetermineAngleFromSurroundings(int type, Color[] surroundingColors)
    {
        if (type.Equals(1))
        {
            // Bordered on Top-Left
            if ((surroundingColors[1].Equals(spaceColors[3]) || surroundingColors[1].Equals(new Color()))
                && (surroundingColors[3].Equals(spaceColors[3]) || surroundingColors[3].Equals(new Color())))
                return Quaternion.Euler(0, 0, 0);

            // Bordered on Top-Right
            else if ((surroundingColors[1].Equals(spaceColors[3]) || surroundingColors[1].Equals(new Color()))
                && (surroundingColors[5].Equals(spaceColors[3]) || surroundingColors[5].Equals(new Color())))
                return Quaternion.Euler(0, -90, 0);

            // Bordered on Bottom-Left
            else if ((surroundingColors[7].Equals(spaceColors[3]) || surroundingColors[7].Equals(new Color()))
                && (surroundingColors[3].Equals(spaceColors[3]) || surroundingColors[3].Equals(new Color())))
                return Quaternion.Euler(0, 90, 0);

            // Bordered on Bottom-Right
            else if ((surroundingColors[7].Equals(spaceColors[3]) || surroundingColors[7].Equals(new Color()))
                && (surroundingColors[5].Equals(spaceColors[3]) || surroundingColors[5].Equals(new Color())))
                return Quaternion.Euler(0, 180, 0);
        }

        else if (type.Equals(2))
        {
            // Bordered on Top-Left
            if (surroundingColors[7] != spaceColors[1] && surroundingColors[5] != spaceColors[1])
                return Quaternion.Euler(0, -90, 0);

            // Bordered on Top-Right
            else if (surroundingColors[7] != spaceColors[1] && surroundingColors[3] != spaceColors[1])
                return Quaternion.Euler(0, 180, 0);

            // Bordered on Bottom-Left
            else if (surroundingColors[1] != spaceColors[1] && surroundingColors[5] != spaceColors[1])
                return Quaternion.Euler(0, 0, 0);

            // Bordered on Bottom-Right
            else if (surroundingColors[1] != spaceColors[1] && surroundingColors[3] != spaceColors[1])
                return Quaternion.Euler(0, 90, 0);
        }
        
        return Quaternion.Euler(0, 0, 0);
    }

    private void CreateWall(bool angled, int type, Quaternion angle, Color[] surroundingColors)
    {
        GameObject g = null;

        if (angled)
        {
            // One-directional walls
            if (type.Equals(0))
            {
                g = Instantiate(wall, new Vector3(coordinates.x, 1, coordinates.y), wall.transform.rotation, transform);
                g.transform.localPosition = new Vector3(0, 0.5f, 0.20725f);

                CheckForAdjacentPits(surroundingColors, 0);
            }

            // Inward-facing corner wall
            else if (type.Equals(1))
            {
                g = Instantiate(inwardCornerWall, new Vector3(coordinates.x - 1f, 0, coordinates.y - 1.5f), inwardCornerWall.transform.rotation, transform);
                RemoveExtraWalls(g, surroundingColors);

                CheckForAdjacentPits(surroundingColors, 1);
            }

            // Outward-facing corner wall
            else if (type.Equals(2))
            {
                g = Instantiate(outwardCornerWall, new Vector3(coordinates.x - 1f, 0, coordinates.y), outwardCornerWall.transform.rotation, transform);
                CheckForAdjacentPits(surroundingColors, 2);
            }
            
            g.transform.localPosition += new Vector3(0, 0, 0.5f);
            transform.rotation = angle;
        }

        else
            g = Instantiate(wall, new Vector3(coordinates.x, 1, coordinates.y), Quaternion.identity, transform);
    }

    // Removes unnecessary walls from any inward corner pieces to prevent overlap
    private void RemoveExtraWalls(GameObject wall, Color[] surroundingColors)
    {
        for (int i = 0; i < 9; ++i)
        {
            if (surroundingColors[i].Equals(spaceColors[1]))
            {
                // Up
                if (i.Equals(1) && CheckForAdjacentDiagonalWalls(surroundingColors, i, true))
                {
                    if (surroundingColors[3].Equals(spaceColors[1]))
                        wall.transform.GetChild(0).gameObject.SetActive(false);
                    if (surroundingColors[5].Equals(spaceColors[1]))
                        wall.transform.GetChild(1).gameObject.SetActive(false);
                }

                // Down
                else if (i.Equals(7) && CheckForAdjacentDiagonalWalls(surroundingColors, i, true))
                {
                    if (surroundingColors[3].Equals(spaceColors[1]))
                        wall.transform.GetChild(1).gameObject.SetActive(false);
                    if (surroundingColors[5].Equals(spaceColors[1]))
                        wall.transform.GetChild(0).gameObject.SetActive(false);
                }

                // Left
                else if (i.Equals(3) && CheckForAdjacentDiagonalWalls(surroundingColors, i, false))
                {
                    if (surroundingColors[1].Equals(spaceColors[1]))
                        wall.transform.GetChild(1).gameObject.SetActive(false);
                    if (surroundingColors[7].Equals(spaceColors[1]))
                        wall.transform.GetChild(0).gameObject.SetActive(false);
                }

                // Right
                else if (i.Equals(5) && CheckForAdjacentDiagonalWalls(surroundingColors, i, false))
                {
                    if (surroundingColors[1].Equals(spaceColors[1]))
                        wall.transform.GetChild(0).gameObject.SetActive(false);
                    if (surroundingColors[7].Equals(spaceColors[1]))
                        wall.transform.GetChild(1).gameObject.SetActive(false);
                }

                //    wall.transform.GetChild(0).gameObject.SetActive(false);
                //else if (i.Equals(7) && CheckForAdjacentDiagonalWalls(surroundingColors, i, true))
                //    wall.transform.GetChild(1).gameObject.SetActive(false);
                //else if (i.Equals(3) && CheckForAdjacentDiagonalWalls(surroundingColors, i, false))
                //    wall.transform.GetChild(0).gameObject.SetActive(false);
                //else if (i.Equals(5) && CheckForAdjacentDiagonalWalls(surroundingColors, i, false))
                //    wall.transform.GetChild(1).gameObject.SetActive(false);
            }
        }
    }

    // If there are any pits next to the wall's space
    private void CheckForAdjacentPits(Color[] surroundingColors, int type)
    {
        bool[] directions = new bool[9];
        bool hori = false, vert = false, corner = false;

        // Normal walls
        if (type.Equals(0))
        {
            if (surroundingColors[3].Equals(spaceColors[2]) || surroundingColors[5].Equals(spaceColors[2]))
                hori = true;

            else if (surroundingColors[1].Equals(spaceColors[2]) || surroundingColors[7].Equals(spaceColors[2]))
                hori = true;
        }

        // Inward walls
        else if (type.Equals(1))
        {
            if (surroundingColors[0].Equals(spaceColors[2]) || surroundingColors[2].Equals(spaceColors[2])
            || surroundingColors[6].Equals(spaceColors[2]) || surroundingColors[8].Equals(spaceColors[2]))
                vert = hori = true;
        }
        
        // Outward walls
        else if (type.Equals(2))
        {
            if (surroundingColors[0].Equals(spaceColors[2]) || surroundingColors[2].Equals(spaceColors[2])
            || surroundingColors[6].Equals(spaceColors[2]) || surroundingColors[8].Equals(spaceColors[2]))
                corner = true;
        }

        // Unseen part of the outward walls
        else if (type.Equals(3))
        {
            if (surroundingColors[3].Equals(spaceColors[2]) || surroundingColors[5].Equals(spaceColors[2]))
                vert = true;

            else if (surroundingColors[1].Equals(spaceColors[2]) || surroundingColors[7].Equals(spaceColors[2]))
                hori = true;
        }

        if (hori || vert || corner)
            SetUpWallPit(hori, vert, corner);

        else
            for (int i = 2; i < 6; ++i)
                transform.GetChild(0).GetChild(i).gameObject.SetActive(true);
    }

    // Moves the blocks adjacent to a wall to create a pit, if possible
    private void SetUpWallPit(bool hori, bool vert, bool corner)
    {
        // Destroys the box and replaces it with a pit
        DestroyImmediate(transform.GetChild(0).gameObject);
        GameObject g = Instantiate(pit, new Vector3(coordinates.x, -1, coordinates.y), Quaternion.identity, transform);

        if (!corner)
        {
            if (hori)
                g.transform.GetChild(1).gameObject.SetActive(true);

            if (vert)
                g.transform.GetChild(2).gameObject.SetActive(true);
        }
        else
            g.transform.GetChild(3).gameObject.SetActive(true);
    }

    #endregion

    #region Doorway Creation
    private void CheckSurroundingDoors(Color[] surroundingColors)
    {
        // Top, Down, Left, Right
        bool[] directionBools = new bool[9];

        // Checks the surrounding tiles for whether or not they contain walls
        for (int i = 0; i < directionBools.Length; ++i)
        {
            if (surroundingColors[i] == spaceColors[6])
                directionBools[i] = true;
            else
                directionBools[i] = false;
        }

        // Sets up more information for the walls
        SetUpDoorway(directionBools, surroundingColors);
    }

    private void SetUpDoorway(bool[] directions, Color[] surroundingColors)
    {
        // Prevents creation of a "Triple" door
        if ((directions[1] && directions[7]) || (directions[3] && directions[5]))
            Debug.LogError("CANNOT CREATE TWO ADJACENT DOORS. PLEASE ADD A WALL BETWEEN THEM AND REBUILD.");

        // Prevents creation of a "Diagonal" door
        else if ((directions[1] && directions[3]) || (directions[7] && directions[3])
            || (directions[1] && directions[5]) || (directions[7] && directions[5]))
            Debug.LogError("CANNOT CREATE DOORWAYS ON A SHARED AXIS.");

        // If the doorway is below its neighbor:
        else if (directions[1])
        {
            // Creates a left-facing doorway
            if (surroundingColors[3] != spaceColors[3] && (surroundingColors[5] == new Color() || surroundingColors[5] == spaceColors[3]))
                CreateDoorway(0, Quaternion.Euler(0, 90, 0));

            // Creates a right-facing doorway
            if (surroundingColors[5] != spaceColors[3] && (surroundingColors[3] == new Color() || surroundingColors[3] == spaceColors[3]))
                CreateDoorway(1, Quaternion.Euler(0, -90, 0));
        }

        // If the doorway is above its neighbor:
        else if (directions[7])
        {
            // Creates a left-facing doorway
            if (surroundingColors[3] != spaceColors[3] && (surroundingColors[5] == new Color() || surroundingColors[5] == spaceColors[3]))
                CreateDoorway(1, Quaternion.Euler(0, 90, 0));

            // Creates a right-facing doorway
            if (surroundingColors[5] != spaceColors[3] && (surroundingColors[3] == new Color() || surroundingColors[3] == spaceColors[3]))
                CreateDoorway(0, Quaternion.Euler(0, -90, 0));
        }

        // If the doorway is on the right:
        else if (directions[3])
        {
            // Creates an up-facing doorway
            if (surroundingColors[1] != spaceColors[3] && (surroundingColors[7] == new Color() || surroundingColors[7] == spaceColors[3]))
                CreateDoorway(1, Quaternion.Euler(0, 0, 0));

            // Creates a down-facing doorway
            if (surroundingColors[7] != spaceColors[3] && (surroundingColors[1] == new Color() || surroundingColors[1] == spaceColors[3]))
                CreateDoorway(0, Quaternion.Euler(0, 180, 0));
        }

        // If the doorway is on the left:
        else if (directions[5])
        {
            // Creates an up-facing doorway
            if (surroundingColors[1] != spaceColors[3] && (surroundingColors[7] == new Color() || surroundingColors[7] == spaceColors[3]))
                CreateDoorway(0, Quaternion.Euler(0, 0, 0));

            // Creates a down-facing doorway
            if (surroundingColors[7] != spaceColors[3] && (surroundingColors[1] == new Color() || surroundingColors[1] == spaceColors[3]))
                CreateDoorway(1, Quaternion.Euler(0, 180, 0));
        }

        // If the doorway is on its own:
        else
        {
            // Creates an up-facing doorway
            if (surroundingColors[1] != spaceColors[3] && (surroundingColors[7] == new Color() || surroundingColors[7] == spaceColors[3]))
                CreateDoorway(2, Quaternion.Euler(0, 0, 0));

            // Creates a down-facing doorway
            else if (surroundingColors[7] != spaceColors[3] && (surroundingColors[1] == new Color() || surroundingColors[1] == spaceColors[3]))
                CreateDoorway(2, Quaternion.Euler(0, 180, 0));

            // Creates a left-facing doorway
            else if (surroundingColors[3] != spaceColors[3] && (surroundingColors[5] == new Color() || surroundingColors[5] == spaceColors[3]))
                CreateDoorway(2, Quaternion.Euler(0, 90, 0));

            // Creates a right-facing doorway
            else if (surroundingColors[5] != spaceColors[3] && (surroundingColors[3] == new Color() || surroundingColors[3] == spaceColors[3]))
                CreateDoorway(2, Quaternion.Euler(0, -90, 0));
        }
    }

    private void CreateDoorway(int type, Quaternion angle)
    {
        GameObject g = null;

        if (type.Equals(0))
            g = Instantiate(leftDoorway, new Vector3(coordinates.x, 1, coordinates.y), leftDoorway.transform.rotation, transform);
        else if (type.Equals(1))
            g = Instantiate(rightDoorway, new Vector3(coordinates.x, 1, coordinates.y), rightDoorway.transform.rotation, transform);
        else if (type.Equals(2))
            g = Instantiate(fullDoorway, new Vector3(coordinates.x, 1, coordinates.y), fullDoorway.transform.rotation, transform);

        if (g != null)
            g.transform.localPosition = new Vector3(0, 0.5f, 0.20725f + 0.5f);

        transform.rotation = angle;
    }
    #endregion

    // Places textures on the space according to its type
    public void TextureSpace(GridTexturer gridTexturer)
    {
        if (lockTextures)
            return;

        Debug.Log(coordinates);
        switch (type)
        {
            // Applies textures to the floor objects
            case SpaceType.Ground:
                transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = gridTexturer.basicFloor;

                // If the floor object's walls are active, texture them, as well
                if (transform.GetChild(0).GetChild(2).gameObject.activeSelf)
                    for (int i = 2; i < 6; ++i)
                        transform.GetChild(0).GetChild(i).GetComponent<MeshRenderer>().material = gridTexturer.basicFloorWall;
                break;

            // Applies textures to the wall objects
            case SpaceType.Wall:

                // If the wall isn't overlooking a pit:
                if (transform.GetChild(0).gameObject.name != (wall.name + "(Clone)") && transform.childCount > 1)
                {
                    transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = gridTexturer.wallEdge180;
                    for (int i = 2; i < 6; ++i)
                        transform.GetChild(0).GetChild(i).GetComponent<MeshRenderer>().material = gridTexturer.basicFloorWall;

                    transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material = gridTexturer.bottomWall;
                    transform.GetChild(1).GetChild(1).GetComponent<MeshRenderer>().material = gridTexturer.topWall;
                    transform.GetChild(1).GetChild(2).GetComponent<MeshRenderer>().material = gridTexturer.bar;
                }

                // If the wall is overlooking a pit:
                else if (transform.GetChild(0).gameObject.name == (wall.name + "(Clone)") && transform.childCount > 1)
                {
                    // Texture the wall
                    transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = gridTexturer.bottomWall;
                    transform.GetChild(0).GetChild(1).GetComponent<MeshRenderer>().material = gridTexturer.topWall;
                    transform.GetChild(0).GetChild(2).GetComponent<MeshRenderer>().material = gridTexturer.bar;

                    // Textures the pit
                    TexturePit(1, gridTexturer);
                }

                // If the wall has a pit, but no "wall" object or box:
                else if (transform.childCount == 1 && transform.GetChild(0).name == (pit.name + "(Clone)"))
                    TexturePit(0, gridTexturer);

                // If the wall has a box, but no "wall" object or pit:
                else if (transform.childCount == 1 && transform.GetChild(0).name != (pit.name + "(Clone)"))
                {
                    transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = gridTexturer.wallEdge180;

                    for (int i = 2; i < 6; ++i)
                        transform.GetChild(0).GetChild(i).GetComponent<MeshRenderer>().material = gridTexturer.basicFloorWall;
                }

                break;

            // Applies textures to the inward-facing corner wall objects
            case SpaceType.InwardWall:

                // If the wall isn't overlooking a pit:
                if (transform.GetChild(0).gameObject.name != (inwardCornerWall.name + "(Clone)") && transform.childCount > 1)
                {
                    transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = gridTexturer.wallEdge90;
                    for (int i = 2; i < 6; ++i)
                        transform.GetChild(0).GetChild(i).GetComponent<MeshRenderer>().material = gridTexturer.basicFloorWall;

                    // Textures the outer portions of the wall
                    for (int i = 0; i < 2; ++i)
                    {
                        transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<MeshRenderer>().material = gridTexturer.bottomInnerWall;
                        transform.GetChild(1).GetChild(i).GetChild(1).GetComponent<MeshRenderer>().material = gridTexturer.topWall;
                        transform.GetChild(1).GetChild(i).GetChild(2).GetComponent<MeshRenderer>().material = gridTexturer.bar;
                    }

                    // Textures the inner portions of the wall
                    for (int i = 2; i < 4; ++i)
                    {
                        transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<MeshRenderer>().material = gridTexturer.bottomInnerWall;
                        transform.GetChild(1).GetChild(i).GetChild(1).GetComponent<MeshRenderer>().material = gridTexturer.topInnerWall;
                        transform.GetChild(1).GetChild(i).GetChild(2).GetComponent<MeshRenderer>().material = gridTexturer.bar;
                    }

                    // Textures the center bars
                    transform.GetChild(1).GetChild(4).GetComponent<MeshRenderer>().material = gridTexturer.bar;
                    transform.GetChild(1).GetChild(5).GetComponent<MeshRenderer>().material = gridTexturer.wallCornerBar;
                }

                // If the wall is overlooking a pit:
                else if (transform.GetChild(0).gameObject.name == (inwardCornerWall.name + "(Clone)") && transform.childCount > 1)
                {
                    // Textures the outer portions of the wall
                    for (int i = 0; i < 2; ++i)
                    {
                        transform.GetChild(0).GetChild(i).GetChild(0).GetComponent<MeshRenderer>().material = gridTexturer.bottomInnerWall;
                        transform.GetChild(0).GetChild(i).GetChild(1).GetComponent<MeshRenderer>().material = gridTexturer.topWall;
                        transform.GetChild(0).GetChild(i).GetChild(2).GetComponent<MeshRenderer>().material = gridTexturer.bar;
                    }

                    // Textures the inner portions of the wall
                    for (int i = 2; i < 4; ++i)
                    {
                        transform.GetChild(0).GetChild(i).GetChild(0).GetComponent<MeshRenderer>().material = gridTexturer.bottomInnerWall;
                        transform.GetChild(0).GetChild(i).GetChild(1).GetComponent<MeshRenderer>().material = gridTexturer.topInnerWall;
                        transform.GetChild(0).GetChild(i).GetChild(2).GetComponent<MeshRenderer>().material = gridTexturer.bar;
                    }

                    // Textures the center bars
                    transform.GetChild(0).GetChild(4).GetComponent<MeshRenderer>().material = gridTexturer.bar;
                    transform.GetChild(0).GetChild(5).GetComponent<MeshRenderer>().material = gridTexturer.wallCornerBar;

                    // Textures the pit
                    TexturePit(1, gridTexturer);
                }

                break;

            // Applies textures to the inward-facing corner wall objects
            case SpaceType.OutwardWall:

                // If the wall isn't overlooking a pit:
                if (transform.GetChild(0).gameObject.name != (outwardCornerWall.name + "(Clone)") && transform.childCount > 1)
                {
                    transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material = gridTexturer.wallEdge270;

                    // Textures the angled part of the wall
                    for (int i = 0; i < 2; ++i)
                    {
                        transform.GetChild(1).GetChild(i).GetChild(0).GetComponent<MeshRenderer>().material = gridTexturer.angleWall_75;
                        transform.GetChild(1).GetChild(i).GetChild(1).GetComponent<MeshRenderer>().material = gridTexturer.angleWall_5;
                        transform.GetChild(1).GetChild(i).GetChild(2).GetComponent<MeshRenderer>().material = gridTexturer.angleWall_32;
                        transform.GetChild(1).GetChild(i).GetChild(3).GetComponent<MeshRenderer>().material = gridTexturer.angleWall_1875;
                    }

                    // Textures the flat part of the wall
                    for (int i = 0; i < 4; ++i)
                        transform.GetChild(1).GetChild(2).GetChild(i).GetComponent<MeshRenderer>().material = gridTexturer.flatWall_375;
                    transform.GetChild(1).GetChild(2).GetChild(4).GetComponent<MeshRenderer>().material = gridTexturer.flatWall_75;
                    transform.GetChild(1).GetChild(2).GetChild(5).GetComponent<MeshRenderer>().material = gridTexturer.flatWall_5;
                    transform.GetChild(1).GetChild(2).GetChild(6).GetComponent<MeshRenderer>().material = gridTexturer.flatWall_32;
                    for (int i = 7; i < 9; ++i)
                        transform.GetChild(1).GetChild(2).GetChild(i).GetComponent<MeshRenderer>().material = gridTexturer.flatWall_25;

                    // Textures the center bar
                    transform.GetChild(1).GetChild(3).GetComponent<MeshRenderer>().material = gridTexturer.bar;
                }

                // If the wall is overlooking a pit:
                else if (transform.GetChild(0).gameObject.name == (outwardCornerWall.name + "(Clone)") && transform.childCount > 1)
                {
                    // Textures the angled part of the wall
                    for (int i = 0; i < 2; ++i)
                    {
                        transform.GetChild(0).GetChild(i).GetChild(0).GetComponent<MeshRenderer>().material = gridTexturer.angleWall_75;
                        transform.GetChild(0).GetChild(i).GetChild(1).GetComponent<MeshRenderer>().material = gridTexturer.angleWall_5;
                        transform.GetChild(0).GetChild(i).GetChild(2).GetComponent<MeshRenderer>().material = gridTexturer.angleWall_32;
                        transform.GetChild(0).GetChild(i).GetChild(3).GetComponent<MeshRenderer>().material = gridTexturer.angleWall_1875;
                    }

                    // Textures the flat part of the wall
                    for (int i = 0; i < 4; ++i)
                        transform.GetChild(0).GetChild(2).GetChild(i).GetComponent<MeshRenderer>().material = gridTexturer.flatWall_375;
                    transform.GetChild(0).GetChild(2).GetChild(4).GetComponent<MeshRenderer>().material = gridTexturer.flatWall_75;
                    transform.GetChild(0).GetChild(2).GetChild(5).GetComponent<MeshRenderer>().material = gridTexturer.flatWall_5;
                    transform.GetChild(0).GetChild(2).GetChild(6).GetComponent<MeshRenderer>().material = gridTexturer.flatWall_32;
                    for (int i = 7; i < 9; ++i)
                        transform.GetChild(0).GetChild(2).GetChild(i).GetComponent<MeshRenderer>().material = gridTexturer.flatWall_25;

                    // Textures the center bar
                    transform.GetChild(0).GetChild(3).GetComponent<MeshRenderer>().material = gridTexturer.bar;

                    // Textures the pit
                    TexturePit(1, gridTexturer);
                }

                break;

            // Applies textures to the ceiling objects
            case SpaceType.Ceiling:
                if (transform.childCount > 1)
                    transform.GetChild(1).GetComponent<MeshRenderer>().material = gridTexturer.basicCeiling;
                break;
        }
    }

    // Textures pit-related objects
    private void TexturePit(int pitIndex, GridTexturer gridTexturer)
    {
        // Texture the top of the pit
        transform.GetChild(pitIndex).GetChild(0).GetComponent<MeshRenderer>().material = gridTexturer.pit;

        // If the "horizontal" wall type is active:
        if (transform.GetChild(pitIndex).GetChild(1).gameObject.activeSelf)
        {
            transform.GetChild(pitIndex).GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material = gridTexturer.basicFloorWall;
            transform.GetChild(pitIndex).GetChild(1).GetChild(1).GetComponent<MeshRenderer>().material = gridTexturer.basicFloorWall;
        }

        // If the "vertical" wall type is active:
        if (transform.GetChild(pitIndex).GetChild(2).gameObject.activeSelf)
        {
            transform.GetChild(pitIndex).GetChild(2).GetChild(0).GetComponent<MeshRenderer>().material = gridTexturer.basicFloorWall;
            transform.GetChild(pitIndex).GetChild(2).GetChild(1).GetComponent<MeshRenderer>().material = gridTexturer.basicFloorWall;
        }

        // If the "corner" wall type is active:
        if (transform.GetChild(pitIndex).GetChild(3).gameObject.activeSelf)
        {
            transform.GetChild(pitIndex).GetChild(3).GetChild(0).GetComponent<MeshRenderer>().material = gridTexturer.halfFloorWall1;
            transform.GetChild(pitIndex).GetChild(3).GetChild(1).GetComponent<MeshRenderer>().material = gridTexturer.halfFloorWall2;
        }
    }
}

