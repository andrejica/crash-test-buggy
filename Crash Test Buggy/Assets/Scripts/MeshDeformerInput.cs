using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDeformerInput : MonoBehaviour
{
    public float force = 100f;
    public float forceOffset = 0.1f;
    
    // Start is called before the first frame update
    // void Start()
    // {
    //     
    // }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0)) 
        {
            HandleInput();
        }
    }
    
    private void HandleInput () 
    {
        RaycastHit hit;
        
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(inputRay, out hit)) 
        {
            MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();
            if (deformer) {
                Vector3 point = hit.point;
                point += hit.normal * forceOffset;
                deformer.isOneTimeChange = true;
                // deformer.AddDeformingForce(point, force);
            }
        }
    }
}