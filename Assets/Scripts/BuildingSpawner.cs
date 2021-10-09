using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BuildingSpawner : MonoBehaviour {
    [SerializeField] private Transform dataCenters;
    [SerializeField] private Transform houses;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private int minDistanceBetweenDataCenters;

    [SerializeField] private GameObject houseGO;
    [SerializeField] private GameObject datacenterGO;
    private Grid<GameObject> _grid;

    private int maxX;
    private int maxY;

    void Start () {
        _grid = gridManager.GetGrid ();
        maxX = Mathf.FloorToInt (_grid.GetWidth () / 2.0f);
        maxY = Mathf.FloorToInt (_grid.GetHeight () / 2.0f);
    }

    void Update () {
        if (Input.GetKeyUp (KeyCode.Space)) {
            //SpawnDatacenter();
            SpawnHouse ();
        }
    }
    void SpawnDatacenter () {
        for (int i = 0; i < 1000; i++) {
            Vector3 pos = new Vector3 (
                Random.Range (-maxX, maxX + 1),
                Random.Range (-maxY, maxY + 1),
                0);
            Debug.Log ("Random pos : " + pos);
            if (_grid.GetValue (pos) == null && !IsTooCloseFromDatacenters (pos)) {
                _grid.SetValue (pos, Instantiate (datacenterGO, _grid.GetGridPosition (pos), Quaternion.identity, dataCenters));
                break;
            }
        }
    }

    private bool IsTooCloseFromDatacenters (Vector3 pos) {
        foreach (Transform dataCenter in dataCenters) {
            if (Vector3.Distance (pos, dataCenter.position) < minDistanceBetweenDataCenters) {
                return true;
            }
        }
        return false;
    }

    public void SpawnHouse () {
        for (int i = 0; i < 1000; i++) {
            var datacenter = dataCenters.GetChild (Random.Range (0, dataCenters.childCount)).position;
            Vector3 pos = new Vector3 (0, 0, 0);
            pos.x = RandomFromDistribution.RandomNormalDistribution (datacenter.x, 0.99f);
            pos.y = RandomFromDistribution.RandomNormalDistribution (datacenter.y, 0.99f);
            Debug.Log ("House pos : " + pos);
            if (_grid.IsInGrid (pos) && _grid.GetValue (pos) == null) {
                _grid.SetValue (pos, Instantiate (houseGO, _grid.GetGridPosition (pos), Quaternion.identity, houses));
                break;
            }

        }
    }
}