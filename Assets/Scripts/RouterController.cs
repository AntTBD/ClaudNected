using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouterController : MonoBehaviour
{

    [Serializable]
    public class Route
    {
        public Route(GameObject port, float cout)
        {
            Port = port;
            Cout = cout;
            listPossibleCableController = new List<float>();
        }
        [field: SerializeField] public GameObject Port;// { get; private set; }
        [field: SerializeField] public float Cout;// { get; private set; }
        [field: SerializeField] public List<float> listPossibleCableController;
    }
    [SerializeField] private List<GameObject> _ports = new List<GameObject>(4);
    [SerializeField] private List<Route> _routingTable=new List<Route>();
    private GameObject _datacenters;
    // Start is called before the first frame update
    void Awake()
    {
        _datacenters = GameObject.Find("DataCenters");
        _routingTable = new List<Route>(_datacenters.transform.childCount);
        foreach (Transform unused in _datacenters.transform)
        {
            _routingTable.Add(new Route(null, float.PositiveInfinity));
        }
    }

    void Start()
    {
        name = "Router " + UnityEngine.Random.Range(0, 1000).ToString();
        StartCoroutine(AutoUpdate());
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            UpdateTable();
        }
    }

    public void UpdateTable()
    {
        _routingTable.Clear();
        foreach (Transform unused in _datacenters.transform)
        {
            _routingTable.Add(new Route(null, float.PositiveInfinity));
        }
        foreach (GameObject cable in _ports)
        {
            RouterController routerController = null;
            GameObject datacenter = null;
            string portTargetTag;
            CableController cableController = cable.GetComponent<CableController>();
            if (cableController.GetBegin() == gameObject)
            {
                portTargetTag = cableController.GetEnd().tag;
                if (portTargetTag.Equals("Router"))
                {
                    routerController = cableController.GetEnd().GetComponent<RouterController>();
                }
                else if (portTargetTag.Equals("DataCenter"))
                {
                    datacenter = cableController.GetEnd();
                }
            }
            else
            {
                portTargetTag = cableController.GetBegin().tag;
                if (portTargetTag.Equals("Router"))
                {
                    routerController = cableController.GetBegin().GetComponent<RouterController>();
                }
                else if (portTargetTag.Equals("DataCenter"))
                {
                    datacenter = cableController.GetBegin();
                }
            }
            //if(datacenter!=null) Debug.LogWarning("datacenter on cable : " + datacenter.name);

            if (portTargetTag.Equals("DataCenter") && datacenter != null)
            {
                int datacenterID = GetDataCenterIdFromGameObject(datacenter);
                if (datacenterID == -1)
                    continue;
                if (_routingTable[datacenterID].Port == null || cableController.GetWeight() <= _routingTable[datacenterID].Cout)
                {
                    _routingTable[datacenterID] = new Route(cable, cableController.GetWeight());
                }
            }
            else if (portTargetTag.Equals("Router") && routerController != null)
            {
                List<Route> routerPath = routerController.GetTable();
                for (int j = 0; j < _routingTable.Count; j++)
                {
                    if (j < routerPath.Count)
                    {
                        if (_routingTable[j].Port == null || routerPath[j].Cout + cableController.GetWeight() <= _routingTable[j].Cout)
                        {
                            _routingTable[j] = new Route(cable, routerPath[j].Cout + cableController.GetWeight());
                        }
                    }
                }
            }
        }
        foreach (Route route in _routingTable)
        {
            route.listPossibleCableController.Clear();
            foreach (GameObject port in _ports)
            {
                route.listPossibleCableController.Add(route.Cout + port.GetComponent<CableController>().GetWeight());
            }
        }
        /*
        int i = 0;
        foreach (Route route in _routingTable)
        {
            if (route.Port != null)
                Debug.LogWarning("Route [" + i + "]: " + route.Port.name + " Cout : " + route.Cout);
            else Debug.LogWarning("Route Cout : " + route.Cout);
            i++;
        }
        */
    }
    private IEnumerator AutoUpdate()
    {
        while (true)
        {
            UpdateTable();
            yield return new WaitForSeconds(1f);
        }
        // ReSharper disable once IteratorNeverReturns
    }

    public List<Route> GetTable()
    {
        return _routingTable;
    }
    public GameObject GetShortestPath(int datacenterID)
    {
        GameObject cable = null;
        if (datacenterID < _routingTable.Count)
            cable = _routingTable[datacenterID].Port;
        if (cable == null)
            return null;
        /*CableController destinationCable = cable.GetComponent<CableController>();
        if (destinationCable.GetBegin() == gameObject)
        {
            if (cable.transform.childCount == 0)
                return destinationCable.GetEnd();
            return cable.transform.GetChild(0).gameObject;
        }
        if (cable.transform.childCount == 0)
            return destinationCable.GetBegin();
        else
            return cable.transform.GetChild(cable.transform.childCount - 1).gameObject;*/
        return cable;
    }
    public GameObject GetShortestPath(GameObject datacenter)
    {
        int id = GetDataCenterIdFromGameObject(datacenter);
        if (id == -1)
        {
            Debug.LogError("Can't find datacenter in datacenters");
            return null;
        }
        return GetShortestPath(id);
    }
    static int GetDataCenterIdFromGameObject(GameObject datacenter)
    {
        int i = 0;
        int datacenterID = -1;
        Transform allDatacenters = GameObject.Find("DataCenters").transform;

        foreach (Transform oneDatacenter in allDatacenters)
        {
            if (oneDatacenter.gameObject == datacenter)
            {
                datacenterID = i;
                break;
            }
            i++;
        }
        return datacenterID;
    }

    public void addPort(GameObject port)
    {
        _ports.Add(port);
        UpdateTable();
    }

    public void removePort(GameObject port)
    {
        _ports.Remove(port);
        UpdateTable();
    }

    private void OnDestroy()
    {
        StopCoroutine(AutoUpdate());
    }
}
