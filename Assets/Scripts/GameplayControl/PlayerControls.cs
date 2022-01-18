using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using TMPro;

public class PlayerControls : MonoBehaviour
{
    GameObject cam;
    Camera camCam;
    UIManager uI;
    public Grid grid;

    public GameObject selectBox, selectionNormalizer;
    public GameObject roadSmallPrfb, roadMidPrfb, roadLargePrfb;
    GameObject newSelection;
    public Vector3 pos1, pos2, boxSize, boxLoc, boxRot;
    public bool selecting;
    bool casting = false;
    
    void Start()
    {
        uI = GetComponent<UIManager>();
        cam = GameObject.Find("Main Camera");
        camCam = cam.GetComponent<Camera>();
    }

    void Update()
    {
        if (selecting)
        {
            Ray ray = camCam.ScreenPointToRay(Mouse.current.position.ReadValue());
            // Vector3 camSpot = camCam.
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                pos2 = hit.point;

                boxLoc = (pos1 + pos2)/2;
                newSelection.transform.localPosition = boxLoc;

                boxSize = (pos1 - pos2);
                if (boxSize.x < 0) boxSize.x = boxSize.x * -1;
                if (boxSize.y < 0) boxSize.y = boxSize.y * -1;
                if (boxSize.z < 0) boxSize.z = boxSize.z * -1;

                newSelection.transform.localScale = new Vector3(boxSize.x, boxSize.y+2, boxSize.z);
            }
        }
    }

    void OnClick(InputValue val)
    {
        //Press
        if (val.Get<float>() > 0.5f)
        {
            selecting = true;
            Ray ray = camCam.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit) && !casting)
            {
                newSelection = Instantiate(selectBox, hit.point, Quaternion.identity, selectionNormalizer.transform);
                newSelection.transform.localScale = Vector3.up;
                pos1 = hit.point;
                casting = true;
            }
        }

        //Release
        else
        {
            
            if (Keyboard.current.shiftKey.ReadValue() < 0.5f)
            {
                uI.selectedUnits.Clear();
            }
            RaycastHit[] hits = Physics.BoxCastAll(new Vector3(boxLoc.x, boxLoc.y + 2, boxLoc.z), new Vector3(boxSize.x/2, boxSize.y, boxSize.z/2), Vector3.down, Quaternion.identity, 5);
        
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.name != "Water" && hit.collider.name != "Map")
                {
                    // Debug.DrawLine(newSelection.transform.position, hit.point, Color.red, 5);
                    uI.selectedUnits.Add(hit.collider.gameObject);
                }
            }
            selecting = false;
            casting = false;
            Destroy(newSelection);
        }
    }

    void OnPoint()
    {
        // Debug.Log("Pointing.");
    }

    void OnRightClick(InputValue val)
    {
        Ray ray = camCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.LogFormat("Right-clicked on a {0} ({1}).", hit.collider.gameObject.name, hit.collider.gameObject.tag);
            Debug.DrawRay(ray.origin, ray.direction * 20, Color.red);
        }
        if (hit.collider.name == "Map" || hit.collider.gameObject.tag == "Road")
        {
            foreach (GameObject unit in uI.selectedUnits)
            {
                if (unit.name == "Capsule" && unit.GetComponentInParent<UnitCitizen>().owner == uI.yourIndex)
                {
                    unit.GetComponentInParent<NavMeshAgent>().SetDestination(hit.point);
                }
            }
        }
    }

    void OnMiddleClick(InputValue val)
    {
        Ray ray = camCam.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.LogFormat("Middle-clicked on a {0}.", hit.collider.gameObject.name);
            Debug.DrawRay(ray.origin, ray.direction * 20, Color.red);
        }
        if (hit.collider.name == "Map")
        {
            GameObject newRoad = GameObject.Instantiate(roadSmallPrfb, grid.CellToWorld(grid.WorldToCell(hit.point)), Quaternion.identity, grid.gameObject.transform);
        }
    }
}
