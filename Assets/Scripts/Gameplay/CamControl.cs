using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CamControl : MonoBehaviour
{
    [SerializeField] private float xInput;
    [SerializeField] private float yInput;

    void OnOrbit(InputValue val)
    {
        xInput = val.Get<float>();
    }

    void OnZoom(InputValue val)
    {
        yInput = val.Get<float>();
    }

    private void Update()
    {

    }
}
