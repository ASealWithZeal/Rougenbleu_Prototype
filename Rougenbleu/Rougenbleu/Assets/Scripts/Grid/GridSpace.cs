using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SpaceType
{
    Ground = 0,
    Pit,
    Wall,
    Ceiling
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
    public GameObject inwardCornerWall;
    public GameObject outwardCornerWall;
    public GameObject switchSpace;
    public GameObject ceilingTile;
    public GameObject rightDoorway;
    public GameObject leftDoorway;

    public bool isSwitch = false;

    // Instantiates grid space values
    public void Instantiate(Color pixelColor, Color[] surroundingPixels, Vector2Int newCoordinates)
    {
        // Sets coordinate information
        coordinates = newCoordinates;

        // Determines the space's type based on the pixel's color
        if (pixelColor.Equals(spaceColors[0])) { type = SpaceType.Ground; }// GetComponent<Renderer>().material = baseMaterials[0]; }

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
            transform.position = new Vector3(coordinates.x, -1, coordinates.y);
            transform.GetChild(0).GetComponent<Renderer>().material = baseMaterials[2];
        }

        // Ceiling
        else if (pixelColor.Equals(spaceColors[3]))
        {
            type = SpaceType.Wall;

            // Checks to see whether or not the ceiling object is bordered by any walls or doors
            // Because these tiles have their own "ceiling" pieces, none should draw if they exist
            bool canDrawCeiling = true;
            for (int i = 0; i < surroundingPixels.Length; ++i)
                if (surroundingPixels[i].Equals(spaceColors[1]) || surroundingPixels[i].Equals(spaceColors[6]))
                    canDrawCeiling = false;

            // Turns all floor tiles black, for use in doors and other "void" areas
            transform.GetChild(0).GetComponent<MeshRenderer>().material = baseMaterials[2];
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
            DestroyImmediate(transform.GetChild(0).gameObject);

            if (pixelColor.Equals(spaceColors[4]))
                g.GetComponent<FloorSwitch>().InstantiateSwitch(SwitchColor.Red);
            else
                g.GetComponent<FloorSwitch>().InstantiateSwitch(SwitchColor.Blue);
        }

        // Doorway
        else if (pixelColor.Equals(spaceColors[6]))
        {
            type = SpaceType.Wall;
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
                CreateWall(true, 0, Quaternion.Euler(0, 90, 0), null);

            // Creates a right-facing wall
            else if (surroundingColors[5] != spaceColors[3] && (surroundingColors[3] == new Color() || surroundingColors[3] == spaceColors[3]))
                CreateWall(true, 0, Quaternion.Euler(0, -90, 0), null);

            // Creates a straight wall
            else
                CreateWall(false, 0, Quaternion.Euler(0, 0, 0), null);
        }

        // Checks if there are walls to the left and right of the current one, but NOT above or below
        else if (!directions[1] && directions[3] && directions[5] && !directions[7] && !CheckForDiagonalWalls(directions, surroundingColors))
        {
            // Creates an up-facing wall
            if (surroundingColors[1] != null && (surroundingColors[7] == new Color() || surroundingColors[7] == spaceColors[3]))
                CreateWall(true, 0, Quaternion.Euler(0, 0, 0), null);

            // Creates a down-facing wall
            else if (surroundingColors[7] != null && (surroundingColors[1] == new Color() || surroundingColors[1] == spaceColors[3]))
                CreateWall(true, 0, Quaternion.Euler(0, 180, 0), null);

            // Creates a straight wall
            else
                CreateWall(false, 0, Quaternion.Euler(0, 0, 0), null);
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
                CreateWall(true, 1, DetermineAngleFromSurroundings(1, surroundingColors), surroundingColors);
        
            else
                CreateWall(true, 2, DetermineAngleFromSurroundings(2, surroundingColors), null);
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
            }

            // Inward-facing corner wall
            else if (type.Equals(1))
            {
                g = Instantiate(inwardCornerWall, new Vector3(coordinates.x - 1f, 0, coordinates.y - 1.5f), inwardCornerWall.transform.rotation, transform);
                RemoveExtraWalls(g, surroundingColors);
            }

            // Outward-facing corner wall
            else if (type.Equals(2))
                g = Instantiate(outwardCornerWall, new Vector3(coordinates.x - 1f, 0, coordinates.y), outwardCornerWall.transform.rotation, transform);

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
                CreateDoorway(1, Quaternion.Euler(0, 90, 0));

            // Creates a right-facing doorway
            if (surroundingColors[5] != spaceColors[3] && (surroundingColors[3] == new Color() || surroundingColors[3] == spaceColors[3]))
                CreateDoorway(0, Quaternion.Euler(0, -90, 0));
        }

        // If the doorway is above its neighbor:
        else if (directions[7])
        {
            // Creates a left-facing doorway
            if (surroundingColors[3] != spaceColors[3] && (surroundingColors[5] == new Color() || surroundingColors[5] == spaceColors[3]))
                CreateDoorway(0, Quaternion.Euler(0, 90, 0));

            // Creates a right-facing doorway
            if (surroundingColors[5] != spaceColors[3] && (surroundingColors[3] == new Color() || surroundingColors[3] == spaceColors[3]))
                CreateDoorway(1, Quaternion.Euler(0, -90, 0));
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

        else
            CreateDoorway(0, Quaternion.Euler(0, 180, 0));
    }

    private void CreateDoorway(int type, Quaternion angle)
    {
        GameObject g = null;

        if (type.Equals(0))
            g = Instantiate(leftDoorway, new Vector3(coordinates.x, 1, coordinates.y), leftDoorway.transform.rotation, transform);
        else if (type.Equals(1))
            g = Instantiate(rightDoorway, new Vector3(coordinates.x, 1, coordinates.y), rightDoorway.transform.rotation, transform);

        if (g != null)
            g.transform.localPosition = new Vector3(0, 0.5f, 0.20725f + 0.5f);
        transform.rotation = angle;
    }
    #endregion
}

