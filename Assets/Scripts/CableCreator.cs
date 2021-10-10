using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;

public class CableCreator : MonoBehaviour {

    private Vector2 mousePos;
    private Grid<GameObject> grid;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private GameObject prefabCables;
    public GameObject pieces;
    private GameObject currentFather;
    private GameObject lastDrawn;
    private CableController _cableController;
    public Sprite straightCable;
    public Sprite cornerCable;
    private GameObject depart = null;
    private GameObject arrivee = null;
    
    void Start () {
        mousePos = GetMouseWorldPosition ();
        grid = gridManager.GetGrid ();
        //InitialPlace(mousePos, Quaternion.identity, new Vector2 (3, 3), maisonTest);
    }

    // Update is called once per frame
    void Update () {
        mousePos = GetMouseWorldPosition ();

        if (depart != null) {
            //Si on est sur point de départ recevable
            if (Input.GetMouseButton(0) && grid.IsInGrid(mousePos)) 
            {
                DrawPointsPath();
            }
        } 
        else if (Input.GetMouseButtonDown (0)) 
        {
            //On récupère le point de départ du potentiel tuyau
            Vector3 gridPosition = grid.GetXY (mousePos);
            depart = grid.gridArray[(int) gridPosition.x, (int) gridPosition.y];
            lastDrawn = depart;

            //Si on est sur un point de départ recevable alors on crée un tuyau
            if (depart != null && grid.IsInGrid (mousePos))
            {
                currentFather = Instantiate(prefabCables,Vector3.zero, Quaternion.identity);
                _cableController = currentFather.GetComponent<CableController>();
                _cableController.SetBegin(depart);
            }
        }

        if (Input.GetMouseButtonUp (0)) {
            
            Vector3 gridPosition = grid.GetXY (mousePos);            
            
            if( grid.IsInGrid (mousePos) )
            {
                arrivee = grid.gridArray[(int) gridPosition.x, (int) gridPosition.y];
            } 
            else
            {
                arrivee = null;
            }

            if(arrivee != null && canDraw())
            {
                _cableController.SetEnd(arrivee);
                Debug.Log(arrivee.tag);
                switch (arrivee.tag)
                {
                    case "Maison":
                    {   
                        HouseController myTargetHouseController= arrivee.GetComponent<HouseController>();

                        if ( !depart.CompareTag("Maison") && myTargetHouseController.GetConnectedCable()==null )
                        {
                            SetUpStartCable();
                            myTargetHouseController.ConnectTo(currentFather.transform.GetChild(currentFather.transform.childCount-1).gameObject);
                        }
                        else
                        {
                            Destroy(currentFather);
                        }
                        break;
                    }
                    case "Router":
                    {
                        SetUpStartCable();
                        arrivee.GetComponent<RouterController>().addPort(currentFather);  
                        break;
                    }

                    case "DataCenter":
                    {
                        SetUpStartCable();
                        arrivee.GetComponent<DatacenterController>().ConnectNewCable(_cableController);
                        break;
                    }

                    case "Cable" :                        
                        if(arrivee != currentFather.transform.GetChild( currentFather.transform.childCount - 1 ).gameObject)
                        {
                            SetUpStartCable();
                        }
                        else
                        {
                            Destroy(currentFather);
                        }
                        break;

                    default:
                        Destroy(currentFather);
                        break;
                }
                DrawCable();
            }
            else
            {
                Destroy(currentFather);
            }

            depart = null;
            currentFather = null;
        }
    }

    public void DrawCable()
    {
        //Dessin
        for(int i = 0 ; i < currentFather.transform.childCount ; i++)
        {
            Vector3 posPrev;
            Vector3 posCurrent;
            Vector3 posNext;

            if(i == 0)
            {
                posPrev = depart.transform.position;
                posCurrent = currentFather.transform.GetChild(i).position;

                if(currentFather.transform.childCount == 1)
                {
                    Debug.Log("JE SUIS UN CABLE A UN SEUL TILE");
                    posNext = arrivee.transform.position;
                } 
                else
                {
                    posNext = currentFather.transform.GetChild(i+1).position;
                }
            }
            else if(i == currentFather.transform.childCount - 1)
            {
                posPrev = currentFather.transform.GetChild(i-1).position;
                posCurrent = currentFather.transform.GetChild(i).position;
                posNext = arrivee.transform.position;
            }
            else
            {
                posPrev = currentFather.transform.GetChild(i-1).position;
                posCurrent = currentFather.transform.GetChild(i).position;
                posNext = currentFather.transform.GetChild(i+1).position;
            }

            ChooseSprite(posPrev, posCurrent, posNext, i);
         }
        
    }

    public void ChooseSprite(Vector3 prev, Vector3 current, Vector3 next, int index)
    {
        string dir = "";
        SpriteRenderer sr = currentFather.transform.GetChild(index).gameObject.GetComponent<SpriteRenderer>();
        sr.color = new Color(0,0,0);

        prev = Vector3Int.RoundToInt(prev);
        next = Vector3Int.RoundToInt(next);

        if( prev.x == current.x)
        {
            dir += prev.y > next.y ? "S" : prev.y < next.y ? "N" : "";
            dir += prev.x > next.x ? "O" : prev.x < next.x ? "E" : "";
        }
        else
        {
            Debug.Log("JAJAJAJAJA");
            dir += prev.x > next.x ? "O" : prev.x < next.x ? "E" : "";
            dir += prev.y > next.y ? "S" : prev.y < next.y ? "N" : "";
        }

        Debug.Log(dir);
        Debug.Log(prev.x + " " + prev.y + " / " + next.x + " " + next.y);
        switch(dir)
        {
            case "NE" :
            case "OS" :
                sr.sprite = cornerCable;
                currentFather.transform.GetChild(index).rotation = Quaternion.Euler( new Vector3(0, 0, -90));
                break;
            
            case "SE" :
            case "ON" :
                sr.sprite = cornerCable;
                currentFather.transform.GetChild(index).rotation = Quaternion.Euler( new Vector3(0, 0, 0));
                break;

            case "SO" :
            case "EN" :
                sr.sprite = cornerCable;
                currentFather.transform.GetChild(index).rotation = Quaternion.Euler( new Vector3(0, 0, 90));
                break;

            case "NO" :
            case "ES" :
                sr.sprite = cornerCable;
                currentFather.transform.GetChild(index).rotation = Quaternion.Euler( new Vector3(0, 0, 180));
                break;

            case "O" :
            case "E" :
                sr.sprite = straightCable;
                break;

            case "N" :
            case "S" :
                sr.sprite = straightCable;
                currentFather.transform.GetChild(index).rotation = Quaternion.Euler(0, 0, 90);
                break;
        }
    }

    public void DrawPointsPath()
    {
        //On teste si la case est libre        
        if(canDraw())
        {
            PlaceObject(pieces);
        }
    }

    public bool isAdjacent(int mouseX, int mouseY, int lastDrawnX, int lastDrawnY)
    {
        //Debug.Log(Mathf.Abs(mouseX-lastDrawnX) + Mathf.Abs(mouseY-lastDrawnY));
        if(Mathf.Abs(mouseX-lastDrawnX) + Mathf.Abs(mouseY-lastDrawnY) < 2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public GameObject CheckNeighbors(Vector3 pos)
    {
        Vector3 gridPos = grid.GetXY(pos);
        float x = gridPos.x;
        float y = gridPos.y;

        return null;
    }

    public bool canDraw()
    {
        int lastDrawnX, lastDrawnY;

        Vector3 gridPosition = grid.GetXY (mousePos);
        int mouseX = (int) gridPosition.x;
        int mouseY = (int) gridPosition.y;

        if(currentFather.transform.childCount != 0)
        {
            Transform lastDrawnTransform = currentFather.transform.GetChild(currentFather.transform.childCount - 1);
            Vector3 lastDrawnPosition = lastDrawnTransform.position;
            Vector3 lastDrawnGridPosition = grid.GetXY (lastDrawnPosition);
            lastDrawnX = (int) lastDrawnGridPosition.x;
            lastDrawnY = (int) lastDrawnGridPosition.y;
        }
        else
        {
            Vector3 lastDrawnGridPosition = grid.GetXY (depart.transform.position);
            lastDrawnX = (int) lastDrawnGridPosition.x;
            lastDrawnY = (int) lastDrawnGridPosition.y;
        }

        return isAdjacent(mouseX,mouseY,lastDrawnX,lastDrawnY);
    }

    private void SetUpStartCable()
    {
        switch (depart.tag)
        {
            case "Maison":
            {   
                HouseController myTargetHouseController = depart.GetComponent<HouseController>();

                if ( !arrivee.CompareTag("Maison") && myTargetHouseController.GetConnectedCable()==null )
                {
                    myTargetHouseController.ConnectTo(currentFather.transform.GetChild(0).gameObject);
                }
                else
                {
                    Destroy(currentFather);
                }
                break;
            }
            case "Router":
            {
                depart.GetComponent<RouterController>().addPort(currentFather);  
                break;
            }

            case "DataCenter":
            {
                depart.GetComponent<DatacenterController>().ConnectNewCable(_cableController);
                break;
            };

            default:
                Destroy(currentFather);
                break;
        }
        
    }
    /*private void DestroyObject(Vector2 gridPos)
    {
        int x = (int)gridPos.x;
        int y = (int)gridPos.y;

        if (grid.GetValue(x, y) != null)
        {
            Destroy(grid.gridArray[x, y]);
            grid.SetValue(x, y, null);

            for (int i = 1; i < 5; i++)
            {
                Vector2 checkPos;

                switch (i)
                {
                    default:
                    case 1:
                        // down
                        checkPos = new Vector2(x, y - 1);
                        break;
                    case 2:
                        // right
                        checkPos = new Vector2(x + 1, y);
                        break;
                    case 3:
                        // up
                        checkPos = new Vector2(x, y + 1);
                        break;
                    case 4:
                        // left
                        checkPos = new Vector2(x - 1, y);
                        break;
                }

                int checkX = (int)checkPos.x;
                int checkY = (int)checkPos.y;

                if (grid.GetValue(checkX, checkY) != null)
                {
                    if (grid.GetValue(checkX, checkY).name.Contains("Cable"))
                    {
                        Vector3 thisPlacePos = grid.GetWorldPosition(checkX, checkY) + new Vector3(grid.GetCellSize(), 0, grid.GetCellSize()) * 0.5f;
                        GameObject thisObjectToPlace = GetCableType(checkX, checkY);

                        Place(thisPlacePos, Quaternion.Euler(0, grid.GetRotationAngle(dir), 0), checkPos, thisObjectToPlace);
                    }
                }
            }
        }
    }*/

    private void PlaceObject (GameObject objectToPlace) {
        Vector3 posXY = grid.GetXY (mousePos);
        int x = (int) posXY.x;
        int y = (int) posXY.y;

        if (grid.GetValue (mousePos) == null) {
            Place (mousePos, Quaternion.identity, new Vector2 (x, y), objectToPlace);
        }
    }

    private void InitialPlace (Vector3 placePos, Quaternion placeRot, Vector2 gridPos, GameObject objectToPlace) {
        GameObject placedObject = Instantiate (objectToPlace, grid.GetGridPosition(placePos), placeRot);

        //Adapte la taille du sprite aux cases
        placedObject.transform.localScale = new Vector3 (grid.GetCellSize () * 100 / 512, grid.GetCellSize () * 100 / 512, grid.GetCellSize () * 100 / 512);

        grid.SetValue (placePos, placedObject);
    }

    private void Place (Vector3 placePos, Quaternion placeRot, Vector2 gridPos, GameObject objectToPlace) {
        GameObject placedObject = Instantiate (objectToPlace, grid.GetGridPosition (mousePos), placeRot);

        //Adapte la taille du sprite aux cases
        placedObject.transform.localScale = new Vector3 (grid.GetCellSize () * 100 / 512, grid.GetCellSize () * 100 / 512, grid.GetCellSize () * 100 / 512);

        placedObject.transform.parent = currentFather.transform;
        grid.SetValue (placePos, placedObject);
    }

}