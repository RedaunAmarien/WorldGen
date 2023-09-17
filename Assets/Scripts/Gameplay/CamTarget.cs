using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CamTarget : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float orbitSpeed;
    [SerializeField] private float zoomSpeed;
    [SerializeField] private bool invertZoom;
    [SerializeField] private float maxFOV = 130;
    [SerializeField] private float minFOV = 10;

    //[SerializeField] private Collider locationBounds;
    [SerializeField] private CinemachineVirtualCamera virtualCam;
    private GameplayManager gameplayManager;

    private float orbitInput;
    private float zoomInput;
    private Vector2 moveInput;
    [SerializeField] private Vector3 offsetFromGround;
    [SerializeField] private Vector2Int chunk;
    [SerializeField] private Vector2Int cell;
    [SerializeField] private Vector2Int tile;

    private void Start()
    {
        gameplayManager = GetComponent<GameplayManager>();
    }

    void OnOrbit(InputValue val)
    {
        orbitInput = val.Get<float>();
    }

    void OnZoom(InputValue val)
    {
        zoomInput = val.Get<float>();
        if (invertZoom)
            zoomInput = -zoomInput;
    }

    void OnMove(InputValue val)
    {
        moveInput = val.Get<Vector2>();
    }

    private void Update()
    {
        transform.Rotate(0, orbitInput * orbitSpeed * Time.deltaTime, 0);
        transform.Translate(moveInput.x * moveSpeed * Time.deltaTime, 0, moveInput.y * moveSpeed * Time.deltaTime);
        //transform.Translate(yInput * zoomSpeed * -virtualCam.transform.forward);
        //virtualCam.GetCinemachineComponent<CinemachineOrbitalTransposer>().m_FollowOffset = new Vector3(0, 15, -15) + new Vector3(0, yInput * moveSpeed, yInput * moveSpeed);
        virtualCam.m_Lens.FieldOfView += zoomInput * zoomSpeed;
        if (virtualCam.m_Lens.FieldOfView > maxFOV)
            virtualCam.m_Lens.FieldOfView = maxFOV;
        else if (virtualCam.m_Lens.FieldOfView < minFOV)
            virtualCam.m_Lens.FieldOfView = minFOV;
    }

    private void LateUpdate()
    {
        Vector2Int[] coords = gameplayManager.Position2Indices(transform.position);
        //Debug.Log(coords[0] + ", " + coords[1] + ", " + coords[2]);
        chunk = coords[0];
        cell = coords[1];
        tile = coords[2];

        transform.position = new Vector3(transform.position.x, gameplayManager.Indices2Position(chunk, cell, tile).y, transform.position.z) + offsetFromGround;

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