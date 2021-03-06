using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Grid<TGridObject>
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] public TGridObject[,] gridArray;

    [SerializeField] private float cellSize;
    [SerializeField] private Vector3 originPosition;

    private TextMesh[,] debugTextArray;

    [SerializeField] float androidCoeff = 1f;

    [SerializeField] private int nbEmptyCells;

    public Grid(int width, int height, float cellSize, Vector3 originPosition, bool isCenter = false, float androidCoeff = 1f)
    {

#if UNITY_ANDROID || UNITY_IOS
        this.androidCoeff = androidCoeff; // disable AndroidCoeff
#endif
        this.cellSize = cellSize * this.androidCoeff;
        this.width = width;
        this.height = height;
        SetWithHeightFromScreenSizeAndCellSize();// rewrite width/height
        //this.width = Mathf.FloorToInt(this.width / this.androidCoeff);
        //this.height = Mathf.FloorToInt(this.height / this.androidCoeff);

        if (isCenter)
            this.originPosition = originPosition - new Vector3(this.width / 2f, this.height / 2f, 0) * this.cellSize;
        else
            this.originPosition = originPosition;

        gridArray = new TGridObject[this.width, this.height];
        debugTextArray = new TextMesh[this.width, this.height];

        this.nbEmptyCells = gridArray.Length;
#if UNITY_EDITOR
        for (int x = 0; x < gridArray.GetLength(0); x++)
        {
            for (int y = 0; y < gridArray.GetLength(1); y++)
            {
                //debugTextArray[x, y] = CreateWorldText (gridArray[x, y].ToString (), null, GetWorldPosition (x, y) + new Vector3 (cellSize, cellSize) * 0.5f, 20, Color.white, TextAnchor.MiddleCenter);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.gray, 100f);
                Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.gray, 100f);
            }
        }
        Debug.DrawLine(GetWorldPosition(0, this.height), GetWorldPosition(this.width, this.height), Color.gray, 100f);
        Debug.DrawLine(GetWorldPosition(this.width, 0), GetWorldPosition(this.width, this.height), Color.gray, 100f);
#endif
    }
    private void SetWithHeightFromScreenSizeAndCellSize()
    {
        try { 
            Camera.main.GetComponent<CameraMovements>().ResetCam();
            this.height = (int)((Camera.main.orthographicSize * 2f - 5f /this.androidCoeff) / this.cellSize);
            this.width = (int)((Camera.main.orthographicSize * ((float)Screen.width / Screen.height) * 2f - 5f / this.androidCoeff) / this.cellSize);
            if (this.height < 5) this.height = 5;
            if (this.width < 5) this.width = 5;
        }
        catch
        {
            if (this.height < 5) this.height = 5;
            if (this.width < 5) this.width = 5;
            return;
        }
    }

    public Vector3 GetWorldPosition(float x, float y)
    {
        return new Vector3(x, y) * cellSize + originPosition;
    }

    private void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);

    }

    public Vector3 GetXY(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        int y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);

        return new Vector3(x, y, 0);
    }

    public Vector3 GetGridPosition(Vector3 position)
    {
        int x, y;
        GetXY(position, out x, out y);

        Vector3 result = new Vector3(
            (float)x * cellSize,
            (float)y * cellSize,
            0) + new Vector3(cellSize, cellSize) * 0.5f;

        result += originPosition;

        return result;
    }

    public void SetValue(Vector3 worldPosition, TGridObject value)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        this.SetValue(x, y, value);
    }

    public void SetValue(int x, int y, TGridObject value)
    {
        if (IsInGrid(x, y))
        {
            this.gridArray[x, y] = value;
            this.nbEmptyCells--;
        }
        else
        {
            Debug.LogError("INDEX OUT OF GRID ARRAY, U RETARDED (set value)");
        }

    }

    public TGridObject GetValue(int x, int y)
    {
        if (IsInGrid(x, y))
        {
            return this.gridArray[x, y];
        }
        else
        {
            Debug.LogError("INDEX OUT OF GRID ARRAY, U RETARDED (get value)");
            return default(TGridObject);
        }
    }

    public bool IsInGrid(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return this.IsInGrid(x, y);
    }

    public bool IsInGrid(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    public TGridObject GetValue(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return this.GetValue(x, y);
    }

    public int GetWidth()
    {
        return this.width;
    }

    public int GetHeight()
    {
        return this.height;
    }

    public Vector3 GetOriginPosition()
    {
        return this.originPosition;
    }

    public float GetCellSize()
    {
        return this.cellSize;
    }

    public bool IsFull()
    {
        return this.nbEmptyCells <= 0;
    }


    public static TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3), int fontSize = 40, Color? color = null, TextAnchor textAnchor = TextAnchor.UpperLeft, TextAlignment textAlignment = TextAlignment.Left, int sortingOrder = 0)
    {
        if (color == null) color = Color.white;
        return CreateWorldText(parent, text, localPosition, fontSize, (Color)color, textAnchor, textAlignment, sortingOrder);
    }

    // Create Text in the World
    public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
    {
        GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localPosition;
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
        return textMesh;
    }

}