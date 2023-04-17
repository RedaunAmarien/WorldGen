using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainCam : MonoBehaviour
{
    public Camera cam;
    public GameObject camHolder, perspectiveBorder, orthoBorder;
    public Vector3 pos = new Vector3(0, 0, 0);
    public bool orthographic;

    public Vector2 minScrollFrame;
    public Vector2 maxScrollFrame;
    public float scrollSpeed;
    public Vector2 minScrollZone, maxScrollZone;


    void Start()
    {
        cam = GetComponent<Camera>();
        camHolder = GameObject.Find("CamHolder");
    }

    public void ToggleOrtho()
    {
        Debug.Log("Toggling perspective.");
        if (orthographic) orthographic = false;
        else orthographic = true;
    }

    void LateUpdate()
    {
        if (orthographic)
        {
            orthoBorder.SetActive(true);
            perspectiveBorder.SetActive(false);
            cam.orthographic = true;
        }
        else
        {
            orthoBorder.SetActive(false);
            perspectiveBorder.SetActive(true);
            cam.orthographic = false;
        }
        Vector2 mouseNormal = cam.ScreenToViewportPoint(Mouse.current.position.ReadValue());

        if ((mouseNormal.x < minScrollFrame.x && camHolder.transform.position.x > minScrollZone.x) || camHolder.transform.position.x > maxScrollZone.x)
        {
            camHolder.transform.Translate(new Vector3(-1, 0, 1) * Time.deltaTime * scrollSpeed);
        }
        if ((mouseNormal.x > maxScrollFrame.x && camHolder.transform.position.x < maxScrollZone.x) || camHolder.transform.position.x < minScrollZone.x)
        {
            camHolder.transform.Translate(new Vector3(1, 0, -1) * Time.deltaTime * scrollSpeed);
        }
        if ((mouseNormal.y < minScrollFrame.y && camHolder.transform.position.z > minScrollZone.y) || camHolder.transform.position.z > maxScrollZone.y)
        {
            camHolder.transform.Translate(new Vector3(-1, 0, -1) * Time.deltaTime * scrollSpeed);
        }
        if ((mouseNormal.y > maxScrollFrame.y && camHolder.transform.position.z < maxScrollZone.y) || camHolder.transform.position.z < minScrollZone.y)
        {
            camHolder.transform.Translate(new Vector3(1, 0, 1) * Time.deltaTime * scrollSpeed);
        }

    }
}
