using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class GlobeView : MonoBehaviour
{
    public GameObject planet;
    public GameObject planetParent;
    public GameObject sun;
    public GameObject sunParent;
    public float zoomSpeed;
    public float orbitSpeed;
    Vector2 inputVal;
    float zoomVal;

    void Update()
    {
        // Move view of whole system
        planetParent.transform.Rotate(new Vector3(0, orbitSpeed * Time.deltaTime * inputVal.x, 0), Space.Self);
        planetParent.transform.Rotate(new Vector3(orbitSpeed * Time.deltaTime * inputVal.y, 0, 0), Space.World);
    }

    void OnOrbit(InputValue value)
    {
        inputVal = value.Get<Vector2>();
    }

    void OnZoom(InputValue value)
    {
        zoomVal = value.Get<float>();
    }
}
