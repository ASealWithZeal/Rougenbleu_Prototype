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
    public GameObject wall;
    public GameObject inwardCornerWall;
    public GameObject outwardCornerWall;

    public bool isSwitch = false;

    // Instantiates grid space values
    public void Instantiate(Color pixelColor, Color[] surroundingPixels, Vector2Int newCoordinates)
    {
        // Sets coordinate information
        coordinates = newCoordinates;

        // Determines the space's type based on the pixel's color
        if (pixelColor.Equals(spaceColors[0])) { type = SpaceType.Ground; }// GetComponent<Renderer>().material = baseMaterials[0]; }

        else if (pixelColor.Equals(spaceColors[1]))
        {
            type = SpaceType.Wall;
            CheckSurroundingWalls(surroundingPixels);
            //GetComponent<Renderer>().material = baseMaterials[0];
            //
            //for (int i = 0; i < transform.childCount; ++i)
            //    transform.GetChild(i).GetComponent<Renderer>().material = baseMaterials[1];
        }

        else if (pixelColor.Equals(spaceColors[2]))
        {
            type = SpaceType.Pit;
            transform.position = new Vector3(coordinates.x, -1, coordinates.y);
            transform.GetChild(0).GetComponent<Renderer>().material = baseMaterials[2];
        }

        else { type = SpaceType.Ground; }
    }

    private void CheckSurroundingWalls(Color[] surroundingColors)
    {
        // Top, Down, Left, Right
        bool[] directionBools = { false, false, false, false };

        // Checks the surrounding tiles for whether or not they contain walls
        if (surroundingColors[0] == spaceColors[1])
            directionBools[0] = true;
        if (surroundingColors[1] == spaceColors[1])
            directionBools[1] = true;
        if (surroundingColors[2] == spaceColors[1])
            directionBools[2] = true;
        if (surroundingColors[3] == spaceColors[1])
            directionBools[3] = true;

        // Sets up more information for the walls
        SetUpWall(directionBools, surroundingColors);
    }

    private void SetUpWall(bool[] directions, Color[] surroundingColors)
    {
        // HERE WE GO
        int numWalls = 0;
        for (int i = 0; i < directions.Length; ++i)
        {
            if (directions[i])
                numWalls++;
        }

        // Checks if there are walls above and below the current one, but NOT to either side
        if (directions[0] && directions[1] && !directions[2] && !directions[3])
        {
            // Creates a left-facing wall
            if (surroundingColors[2] != spaceColors[3] && (surroundingColors[3] == new Color() || surroundingColors[3] == spaceColors[3]))
                CreateWall(true, 0, Quaternion.Euler(0, 90, 0));

            // Creates a right-facing wall
            else if (surroundingColors[3] != spaceColors[3] && (surroundingColors[2] == new Color() || surroundingColors[2] == spaceColors[3]))
                CreateWall(true, 0, Quaternion.Euler(0, -90, 0));

            // Creates a straight wall
            else
                CreateWall(false, 0, Quaternion.Euler(0, 0, 0));
        }

        // Checks if there are walls to the left and right of the current one, but NOT above or below
        else if (!directions[0] && !directions[1] && directions[2] && directions[3])
        {
            // Creates an up-facing wall
            if (surroundingColors[0] != null && (surroundingColors[1] == new Color() || surroundingColors[1] == spaceColors[3]))
                CreateWall(true, 0, Quaternion.Euler(0, 0, 0));

            // Creates a down-facing wall
            else if (surroundingColors[1] != null && (surroundingColors[0] == new Color() || surroundingColors[0] == spaceColors[3]))
                CreateWall(true, 0, Quaternion.Euler(0, 180, 0));

            // Creates a straight wall
            else
                CreateWall(false, 0, Quaternion.Euler(0, 0, 0));
        }

        // If there are two walls, check to determine their setup
        else if (numWalls == 2)
        {
            bool borderedByCeiling = false;
            for (int i = 0; i < surroundingColors.Length; ++i)
                if (surroundingColors[i].Equals(spaceColors[3]) || surroundingColors[i].Equals(new Color()))
                {
                    borderedByCeiling = true;
                    continue;
                }

            if (borderedByCeiling)
                CreateWall(true, 1, DetermineAngleFromSurroundings(1, surroundingColors));

            else
                CreateWall(true, 2, DetermineAngleFromSurroundings(2, surroundingColors));
        }
    }

    private Quaternion DetermineAngleFromSurroundings(int type, Color[] surroundingColors)
    {
        if (type.Equals(1))
        {
            // Bordered on Top-Left
            if ((surroundingColors[0].Equals(spaceColors[3]) || surroundingColors[0].Equals(new Color()))
                && (surroundingColors[2].Equals(spaceColors[3]) || surroundingColors[2].Equals(new Color())))
                return Quaternion.Euler(0, 0, 0);

            // Bordered on Top-Right
            else if ((surroundingColors[0].Equals(spaceColors[3]) || surroundingColors[0].Equals(new Color()))
                && (surroundingColors[3].Equals(spaceColors[3]) || surroundingColors[3].Equals(new Color())))
                return Quaternion.Euler(0, -90, 0);

            // Bordered on Bottom-Left
            else if ((surroundingColors[1].Equals(spaceColors[3]) || surroundingColors[1].Equals(new Color()))
                && (surroundingColors[2].Equals(spaceColors[3]) || surroundingColors[2].Equals(new Color())))
                return Quaternion.Euler(0, 90, 0);

            // Bordered on Bottom-Right
            else if ((surroundingColors[1].Equals(spaceColors[3]) || surroundingColors[1].Equals(new Color()))
                && (surroundingColors[3].Equals(spaceColors[3]) || surroundingColors[3].Equals(new Color())))
                return Quaternion.Euler(0, 180, 0);
        }

        else if (type.Equals(2))
        {
            // Bordered on Top-Left
            if (surroundingColors[1] != spaceColors[1] && surroundingColors[3] != spaceColors[1])
                return Quaternion.Euler(0, -90, 0);

            // Bordered on Top-Right
            else if (surroundingColors[1] != spaceColors[1] && surroundingColors[2] != spaceColors[1])
                return Quaternion.Euler(0, 180, 0);

            // Bordered on Bottom-Left
            else if (surroundingColors[0] != spaceColors[1] && surroundingColors[3] != spaceColors[1])
                return Quaternion.Euler(0, 0, 0);

            // Bordered on Bottom-Right
            else if (surroundingColors[0] != spaceColors[1] && surroundingColors[2] != spaceColors[1])
                return Quaternion.Euler(0, 90, 0);
        }

        Debug.Log("HERE");
        return Quaternion.Euler(0, 0, 0);
    }

    private void CreateWall(bool angled, int type, Quaternion angle)
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
                g = Instantiate(inwardCornerWall, new Vector3(coordinates.x - 0.5f, 0, coordinates.y - 0.5f), inwardCornerWall.transform.rotation, transform);

            // Inward-facing corner wall
            else if (type.Equals(2))
                g = Instantiate(outwardCornerWall, new Vector3(coordinates.x - 0.5f, 0, coordinates.y - 0.5f), outwardCornerWall.transform.rotation, transform);

            transform.rotation = angle;
        }

        else
            g = Instantiate(wall, new Vector3(coordinates.x, 1, coordinates.y), Quaternion.identity, transform);
    }
}

