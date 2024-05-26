using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

//https://catlikecoding.com/unity/tutorials/mesh-deformation/
//Tipp: Could be used ANY Mesh object to use for deformation
[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{
    private Mesh _meshToDeform;
    private Mesh _buggyMesh;
    private Vector3[] _originalVertices, _displacedVertices, _vertexVelocities;
    
    private Rigidbody _buggyRigidBody;
    private CarBehaviour _carScript;
    
    public float springForce = 20f;
    public float damping = 5f;
    public bool isOneTimeChange;
    
    // Start is called before the first frame update
    void Start()
    {
        _carScript = gameObject.GetComponent<CarBehaviour>();
        _buggyRigidBody = GetComponent<Rigidbody>();
        
        //TODO Why are origin vertices at position (0, 0, 0)???
        //https://forum.unity.com/threads/mesh-read-write-enable-checkbox-missing.1286540/
        //TODO try to get vertices to be in runtime position where they are needed...
        //Mesh position always at origin
        //https://discussions.unity.com/t/mesh-vertices-position-not-correct/32537
        //https://stackoverflow.com/questions/49104794/modify-vertices-at-runtime
        _meshToDeform = GameObject.Find("buggy").GetComponent<MeshFilter>().mesh;
        
        _originalVertices = _meshToDeform.vertices;
        _displacedVertices = new Vector3[_originalVertices.Length];
        for (int i = 0; i < _originalVertices.Length; i++)
        {
            _displacedVertices[i] = _originalVertices[i];
        }
        
        _vertexVelocities = new Vector3[_originalVertices.Length];
    }

    // Update is called once per frame
    void Update()
    {
        if (isOneTimeChange)
        {
            for (int i = 0; i < _displacedVertices.Length; i++)
            {
                UpdateVertex(i);
            }
            
            _meshToDeform.vertices = _displacedVertices;
            _meshToDeform.RecalculateNormals();
            // _meshToDeform.RecalculateBounds();
            _vertexVelocities = new Vector3[_originalVertices.Length];
            isOneTimeChange = false;
        }
    }

    private void FixedUpdate()
    {
        
    }

    public void AddDeformingForce (Vector3 point, float force) 
    {
        for (int i = 0; i < _displacedVertices.Length; i++)
        {
            AddForceToVertex(i, point, force);
        }
        
        // Debug.DrawLine(Camera.main.transform.position, point);
        Debug.DrawLine(point, _displacedVertices[0]);
    }

    private void OnCollisionEnter(Collision other)
    {
        
        ContactPoint contactPoint = other.GetContact(0);
        // Vector3 collisionPoint = other.contacts[0].point;
        Vector3 localCollisionPoint = transform.InverseTransformPoint(contactPoint.point);
        
        float currentSpeed = _carScript.GetCurrentSpeed();
        float buggyVelocityMS = currentSpeed / 3.6f;
        

        if (currentSpeed > 5.0f)
        {
            isOneTimeChange = true;
            float impactForce = _buggyRigidBody.mass * (float)Math.Pow(buggyVelocityMS, 2) * 0.5f;
            AddDeformingForce(localCollisionPoint, impactForce);
        }
    }

    #region private
    void AddForceToVertex (int i, Vector3 point, float force)
    {
        Vector3 pointToVertex = _displacedVertices[i] - point;
        float attenuatedForce = force / (1.0f + pointToVertex.sqrMagnitude);
        
        //Ignores mass, evt. adding mass to simulate as exactly as possible in testing...
        float velocity = attenuatedForce * Time.deltaTime;
        _vertexVelocities[i] += pointToVertex.normalized * velocity;
    }
    
    private void UpdateVertex (int i) {
        Vector3 velocity = _vertexVelocities[i];
        // Vector3 displacement = _displacedVertices[i] - _originalVertices[i];
        // velocity -= displacement * springForce * Time.deltaTime;
        // velocity *= 1f - damping * Time.deltaTime;
        _vertexVelocities[i] = velocity;
        _displacedVertices[i] += velocity * Time.deltaTime;
    }
    
    #endregion
}
