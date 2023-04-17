using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obscurable : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
      // get all renderers in this object and its children:
      Renderer[] rends = gameObject.GetComponentsInChildren<Renderer>();
      foreach (Renderer rendr in rends){
        rendr.material.renderQueue = 3002; // set their renderQueue
      }
    }
}
