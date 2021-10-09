using UnityEngine;
using Random = UnityEngine.Random;

public class DataController : MonoBehaviour
{
    private GameObject objDepart;

    private GameObject objArrive;

    private GameObject dataCenter;
    private int indexChild = 0;
    private bool direction;
    private float speed = 0.5f;
    private void Start()
    {
        var trs = transform;
        //Initial objDepart is it's parent aka HouseObject
        objDepart = trs.parent.gameObject;
        trs.position = objDepart.transform.position;
        objArrive = objDepart.GetComponent<HouseController>().connectedCable;
        dataCenter = SelectRandomDataCenter();
        Debug.Log("Iniatlization finished");
        GetComponent<SpriteRenderer>().sortingOrder = 1;
        InitializeIndex();
    }
    public void FixedUpdate()
    {
        var step = speed * Time.deltaTime;
        
        transform.position=Vector3.MoveTowards(transform.position, objArrive.transform.position, step);
        if (transform.position != objArrive.transform.position) return;
        if (direction) indexChild++;
        else indexChild--;
        if (indexChild < 0 || indexChild >= objArrive.transform.parent.childCount)
        {
            var endCable=objArrive.transform.parent.GetComponent<CableController>().connectedTo;
            if (endCable.CompareTag("Router"))
            {
                //::TODO : Implémenter la méthode du Routeur qui donne le bon bout de cable
                objDepart = endCable;
                objArrive = endCable.GetComponent<RouterController>().redirectTo;
                indexChild=InitializeIndex();
            }else if (endCable.CompareTag("DataCenter"))
            {
                Debug.Log("Winner");
            }
        }
        else
        {
            objArrive = objArrive.transform.parent.GetChild(indexChild).gameObject; 
        }
            
        

        }

    private GameObject SelectRandomDataCenter()
    {
        var dataCenters = GameObject.Find("DataCenters").transform;
        var indexDcSelected = Random.Range(0, dataCenters.childCount);
        return dataCenters.GetChild(indexDcSelected).gameObject;
    }

    private int InitializeIndex()
    {
        var parentObj = objArrive.transform.parent;
        direction = objArrive.Equals(parentObj.GetChild(0).gameObject);
        return direction ? 0 : parentObj.childCount;
    }

    public void DeleteData()
    {
        transform.parent.GetComponent<HouseController>().SetIsSatified(false);
        Destroy(gameObject);
        
    }
}