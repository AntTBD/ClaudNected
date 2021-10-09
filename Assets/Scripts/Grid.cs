using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid<TGridObject> {
    private int width;
    private int height;
    private TGridObject[, ] gridArray;

    private float cellSize;
    private Vector3 originPosition;

    private TextMesh[, ] debugTextArray;

    public Grid (int width, int height, float cellSize, Vector3 originPosition) {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];
        debugTextArray = new TextMesh[width, height];

        //Debug.Log (this.width + " " + this.height);
        bool showDebug = true;

        if (showDebug) {
            for (int x = 0; x < gridArray.GetLength (0); x++) {
                for (int y = 0; y < gridArray.GetLength (1); y++) {
                    //debugTextArray[x, y] = CreateWorldText (gridArray[x, y].ToString (), null, GetWorldPosition (x, y) + new Vector3 (cellSize, cellSize) * 0.5f, 20, Color.white, TextAnchor.MiddleCenter);
                    Debug.DrawLine (GetWorldPosition (x, y), GetWorldPosition (x, y + 1), Color.white, 100f);
                    Debug.DrawLine (GetWorldPosition (x, y), GetWorldPosition (x + 1, y), Color.white, 100f);
                }
            }
            Debug.DrawLine (GetWorldPosition (0, height), GetWorldPosition (width, height), Color.white, 100f);
            Debug.DrawLine (GetWorldPosition (width, 0), GetWorldPosition (width, height), Color.white, 100f);
        }
    }

    public Vector3 GetWorldPosition (int x, int y) {
        return new Vector3 (x, y) * cellSize + originPosition;
    }

    private void GetXY (Vector3 worldPosition, out int x, out int y) {
        x = Mathf.FloorToInt ((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt ((worldPosition - originPosition).y / cellSize);

    }

    public Vector3 GetGridPosition(Vector3 position)
    {
        position -= originPosition;

        int xCount = Mathf.RoundToInt(position.x / cellSize);
        int yCount = Mathf.RoundToInt(position.y / cellSize);

        Vector3 result = new Vector3(
            (float)xCount * cellSize,
            (float)yCount * cellSize,
           0);

        result += originPosition;

        return result;
    }

    public void SetValue (Vector3 worldPosition, TGridObject value) {
        int x, y;
        GetXY (worldPosition, out x, out y);
        this.SetValue (x, y, value);
    }

    public void SetValue (int x, int y, TGridObject value) {
        if (x >= 0 && y >= 0 && x < width && y < height) {
            this.gridArray[x, y] = value;
            this.debugTextArray[x, y].text = gridArray[x, y].ToString ();
        } else {
            Debug.LogError ("INDEX OUT OF GRID ARRAY, U RETARDED (set value)");
        }

    }

    public TGridObject GetValue (int x, int y) {
        if (x >= 0 && y >= 0 && x < width && y < height) {
            return this.gridArray[x, y];
        } else {
            Debug.LogError ("INDEX OUT OF GRID ARRAY, U RETARDED (get value)");
            return default (TGridObject);
        }
    }

    public TGridObject GetValue (Vector3 worldPosition) {
        int x, y;
        GetXY (worldPosition, out x, out y);
        return this.GetValue (x, y);
    }

    public int GetWidth () {
        return this.width;
    }

    public int GetHeight () {
        return this.height;
    }

    public Vector3 GetOriginPosition () {
        return this.originPosition;
    }

    public static TextMesh CreateWorldText (string text, Transform parent = null, Vector3 localPosition = default (Vector3), int fontSize = 40, Color? color = null, TextAnchor textAnchor = TextAnchor.UpperLeft, TextAlignment textAlignment = TextAlignment.Left, int sortingOrder = 0) {
        if (color == null) color = Color.white;
        return CreateWorldText (parent, text, localPosition, fontSize, (Color) color, textAnchor, textAlignment, sortingOrder);
    }

    // Create Text in the World
    public static TextMesh CreateWorldText (Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder) {
        GameObject gameObject = new GameObject ("World_Text", typeof (TextMesh));
        Transform transform = gameObject.transform;
        transform.SetParent (parent, false);
        transform.localPosition = localPosition;
        TextMesh textMesh = gameObject.GetComponent<TextMesh> ();
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.GetComponent<MeshRenderer> ().sortingOrder = sortingOrder;
        return textMesh;
    }

}