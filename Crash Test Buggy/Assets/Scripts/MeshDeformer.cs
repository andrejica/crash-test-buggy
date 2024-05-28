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
    private Mesh _deformedMesh;
    private Mesh _originalMesh;
    private Mesh _buggyMesh;
    private Vector3[] _originalVertices, _displacedVertices, _vertexVelocities;
    
    private Rigidbody _buggyRigidBody;
    private CarBehaviour _carScript;
    private Transform _buggyChassisTransform;
    
    public float springForce = 20f;
    public float damping = 5f;
    public bool isOneTimeChange;

    // Percentage of kinetic energy absorbed by de car
    private const float EnergyAbsorptionPercentage = 0.7f;
    
    // Start is called before the first frame update
    void Start()
    {
        _carScript = gameObject.GetComponent<CarBehaviour>();
        _buggyRigidBody = GetComponent<Rigidbody>();
        

        //https://forum.unity.com/threads/mesh-read-write-enable-checkbox-missing.1286540/
        //Mesh position always at origin
        //https://discussions.unity.com/t/mesh-vertices-position-not-correct/32537
        //https://stackoverflow.com/questions/49104794/modify-vertices-at-runtime
        Mesh buggyMesh = GameObject.Find("buggy").GetComponent<MeshFilter>().mesh;
        _buggyChassisTransform = GameObject.Find("buggy").GetComponent<Transform>();

        _originalMesh = buggyMesh;
        _deformedMesh = Instantiate(_originalMesh);
        


        // _originalVertices = _meshToDeform.vertices;
        // _displacedVertices = Instantiate(_originalVertices); //new Vector3[_originalVertices.Length];
        // for (int i = 0; i < _originalVertices.Length; i++)
        // {
        //     _displacedVertices[i] = _originalVertices[i];
        // }

        // _vertexVelocities = new Vector3[_originalVertices.Length];
    }

    // Update is called once per frame
    void Update()
    {
        // if (isOneTimeChange)
        // {
        //     //TODO add here the check for the radius of the crash energy?
        //     //--> Check from ChatGPT...
        //     for (int i = 0; i < _displacedVertices.Length; i++)
        //     {
        //         //Update Vertex
        //         Vector3 velocity = _vertexVelocities[i];
        //         _vertexVelocities[i] = velocity;
        //         _displacedVertices[i] += velocity * Time.deltaTime;
        //     }
        //     
        //     _meshToDeform.vertices = _displacedVertices;
        //     _meshToDeform.RecalculateNormals();
        //     _meshToDeform.RecalculateBounds();
        //     _vertexVelocities = new Vector3[_originalVertices.Length];
        //     isOneTimeChange = false;
        // }
        
        RepairBuggy();
    }

    private void FixedUpdate()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        ContactPoint contactPoint = other.GetContact(0);
        Vector3 localCollisionPoint = _buggyChassisTransform.InverseTransformPoint(contactPoint.point);

        // Debug.Log($"Collision world coordinates: {contactPoint.point}");
        // Debug.Log($"Collision local coordinates: {localCollisionPoint}");
        
        float currentSpeed = _carScript.GetCurrentSpeed();
        float buggyVelocityMS = currentSpeed / 3.6f;
        

        if (currentSpeed > 5.0f)
        {
            isOneTimeChange = true;
            float buggyKineticEnergy = _buggyRigidBody.mass * Mathf.Pow(buggyVelocityMS, 2) * 0.5f;
            AddDeformingForce(localCollisionPoint, buggyKineticEnergy * EnergyAbsorptionPercentage);
            // DeformMesh(localCollisionPoint, buggyKineticEnergy * EnergyAbsorptionPercentage);
        }
    }
    
    public void AddDeformingForce (Vector3 point, float force) 
    {
        //Add force to vertex
        Vector3[] vertices = _deformedMesh.vertices;
        float deformationRadius = 1.0f;
        float maxDeformation = 0.1f;
        
        for (int i = 0; i < vertices.Length; i++)
        {
            // Vector3 pointToVertex = _displacedVertices[i] - point;
            // float attenuatedForce = force / (1.0f + pointToVertex.sqrMagnitude);
            //
            // float velocity = attenuatedForce * Time.deltaTime;

            float distance = Vector3.Distance(vertices[i], point);

            if (distance < deformationRadius)
            {
                //TODO try to use "attenuatedForce" for deformation length...
                // float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
                float deformation = Mathf.Lerp(maxDeformation * force, 0, distance / deformationRadius);
                vertices[i] -= point.normalized * (deformation * Time.deltaTime);
            }
            
            _deformedMesh.vertices = vertices;
            _deformedMesh.RecalculateNormals();
            // _deformedMesh.RecalculateBounds();
        }
        
        // Debug.DrawLine(Camera.main.transform.position, point);
        // Debug.DrawLine(transform.position, point);
    }

    #region private
    // void AddForceToVertex (int i, Vector3 point, float kineticEnergy)
    // {
    //     float deformationRadius = 1.0f;
    //     float maxDeformation = 0.1f;
    //     
    //     Vector3 pointToVertex = _displacedVertices[i] - point;
    //     float attenuatedForce = kineticEnergy / (1.0f + pointToVertex.sqrMagnitude);
    //     
    //     float velocity = attenuatedForce * Time.deltaTime;
    //     // float deformation = Mathf.Lerp(maxDeformation * kineticEnergy, 0, )
    //     _vertexVelocities[i] += pointToVertex.normalized * (velocity * maxDeformation);
    // }

    // private void DeformMesh(Vector3 localCollisionPoint, float kineticEnergy)
    // {
    //     float deformationRadius = 1.0f;
    //     float maxDeformation = 0.1f;
    //     
    //     //Visualize local collision point
    //     Debug.DrawLine(transform.position, transform.TransformPoint(localCollisionPoint), Color.red, 2.0f);
    //
    //     for (int i = 0; i < _displacedVertices.Length; i++)
    //     {
    //         float distance = Vector3.Distance(_displacedVertices[i], localCollisionPoint);
    //
    //         if (distance < deformationRadius)
    //         {
    //             float deformation = Mathf.Lerp(maxDeformation * kineticEnergy, 0, distance / deformationRadius);
    //             _displacedVertices[i] -= localCollisionPoint.normalized * deformation;
    //         }
    //     }
    //
    //     _meshToDeform.vertices = _displacedVertices;
    //     _meshToDeform.RecalculateNormals();
    // }

    private void RepairBuggy()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            _deformedMesh.vertices = _originalVertices;
            _deformedMesh.RecalculateNormals();
        }
    }
    
    #endregion
}
