using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.IO;

public class Grid : MonoBehaviour
{
    private GridSpace[,] grid;
    public GameObject gridSpace;
    public Texture2D levelImage;

    public static Grid _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(_instance);
        else
            _instance = this;

        //GenerateGrid();
    }

    // Start is called before the first frame update
    void Start()
    {
        //GenerateGrid();
    }

    // Generates a basic grid setup using an outside file
    // Users can update the created grid using other objects, tiles, etc.
    public void GenerateGrid()
    {
        Color[] pixels = GetPixels();
        grid = new GridSpace[levelImage.width, levelImage.height];

        for (int i = 0; i < levelImage.height; ++i)
            for (int j = 0; j < levelImage.width; ++j)
                CreateGridSpace(GetSurroundingPixels(new Vector2Int(j, i), pixels), 
                    pixels[j + (i * levelImage.width)], new Vector2Int(j, i));
    }

    private Color[] GetPixels()
    {
        int size = levelImage.width * levelImage.height;
        Color[] pixels = new Color[size];

        for (int i = 0; i < levelImage.height; ++i)
            for (int j = 0; j < levelImage.width; ++j)
                pixels[j + (i * levelImage.width)] = levelImage.GetPixel(j, i);

        return pixels;
    }

    // Generate a single grid space
    private void CreateGridSpace(Color[] surroundingPixels, Color pixelColor, Vector2Int coordinates)
    {
        GameObject g = Instantiate(gridSpace, new Vector3Int(coordinates.x, 0, coordinates.y), Quaternion.identity, transform);
        grid[coordinates.x, coordinates.y] = g.GetComponent<GridSpace>();
        
        g.GetComponent<GridSpace>().Instantiate(pixelColor, surroundingPixels, coordinates);
    }

    private Color[] GetSurroundingPixels(Vector2Int coordinates, Color[] pixels)
    {
        int index = coordinates.x + (coordinates.y * levelImage.width);
        Color[] surroundingColors = new Color[4];
        if (index - levelImage.width >= 0)
            surroundingColors[0] = pixels[index - levelImage.width];
        if (index + levelImage.width < pixels.Length)
            surroundingColors[1] = pixels[index + levelImage.width];
        if (coordinates.x - 1 >= 0)
            surroundingColors[2] = pixels[index - 1];
        if (coordinates.x + 1 < (pixels.Length / levelImage.height))
            surroundingColors[3] = pixels[index + 1];

        return surroundingColors;
    }

    // Returns a grid space's type to determine whether it is passable or impassable
    public SpaceType GetSpaceType(Vector2Int c)
    {
        return grid[c.x, c.y].type;
    }
}