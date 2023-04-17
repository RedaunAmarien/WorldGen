using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CamTarget : MonoBehaviour
{
    [SerializeField] private float orbitSpeed;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private float moveSpeed;
    [SerializeField] private bool invertZoom;

    //[SerializeField] private Collider locationBounds;
    [SerializeField] private Transform camControl;

    private float xInput;
    private float yInput;
    private Vector2 moveInput;

    void OnOrbit(InputValue val)
    {
        xInput = val.Get<float>();
    }

    void OnZoom(InputValue val)
    {
        yInput = val.Get<float>();
        if (invertZoom)
            yInput = -yInput;
    }

    void OnMove(InputValue val)
    {
        moveInput = val.Get<Vector2>();
    }

    private void Update()
    {
        transform.Rotate(0, xInput * orbitSpeed * Time.deltaTime, 0);
        transform.Translate(moveInput.x * moveSpeed * Time.deltaTime, 0, moveInput.y * moveSpeed * Time.deltaTime);
        transform.Translate(yInput * zoomSpeed * -camControl.forward);

        //if (transform.position.x > locationBounds.bounds.max.x)
        //    transform.position = new Vector3(locationBounds.bounds.max.x, transform.position.y, transform.position.z);
        //else if (transform.position.x < locationBounds.bounds.min.x)
        //    transform.position = new Vector3(locationBounds.bounds.min.x, transform.position.y, transform.position.z);

        //if (transform.position.y > locationBounds.bounds.max.y)
        //    transform.position = new Vector3(transform.position.x, locationBounds.bounds.max.y, transform.position.z);
        //else if (transform.position.y < locationBounds.bounds.min.y)
        //    transform.position = new Vector3(transform.position.x, locationBounds.bounds.min.y, transform.position.z);

        //if (transform.position.z > locationBounds.bounds.max.z)
        //    transform.position = new Vector3(transform.position.x, transform.position.y, locationBounds.bounds.max.z);
        //else if(transform.position.z < locationBounds.bounds.min.z)
        //    transform.position = new Vector3(transform.position.x, transform.position.y, locationBounds.bounds.min.z);
    }
}