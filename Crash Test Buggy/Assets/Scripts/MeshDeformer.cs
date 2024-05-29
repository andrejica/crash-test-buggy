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
        // Mesh buggyMesh = GameObject.Find("buggy").GetComponent<MeshFilter>().mesh;
        _buggyChassisTransform = GameObject.Find("buggy").GetComponent<Transform>();

        // _originalMesh = buggyMesh;
        // _deformedMesh = Instantiate(_originalMesh);
        
        
        _deformedMesh = GameObject.Find("buggy").GetComponent<MeshFilter>().mesh;
        _originalVertices = _deformedMesh.vertices;
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
            //--> Check from ChatGPT...
            for (int i = 0; i < _displacedVertices.Length; i++)
            {
                // Update Vertex
                 Vector3 velocity = _vertexVelocities[i];
                 _displacedVertices[i] += velocity * Time.deltaTime;
                
            }
            
            _deformedMesh.vertices = _displacedVertices;
            _deformedMesh.RecalculateNormals();
            // _meshToDeform.RecalculateBounds();
            _vertexVelocities = new Vector3[_originalVertices.Length];
            isOneTimeChange = false;
        }
        
        RepairBuggy();
    }

    private void FixedUpdate()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        //Calculate middle of crash of all contact points...
        ContactPoint contactPoint = other.GetContact(0);
        Vector3 localCollisionPoint = _buggyChassisTransform.InverseTransformPoint(contactPoint.point);

        // Debug.Log($"Collision world coordinates: {contactPoint.point}");
        // Debug.Log($"Collision local coordinates: {localCollisionPoint}");
        
        float currentSpeed = _carScript.GetCurrentSpeed();
        // float buggyVelocityMS = currentSpeed / 3.6f;
        float relativeVelocity = other.relativeVelocity.magnitude;
        

        if (currentSpeed > 5.0f)
        {
            isOneTimeChange = true;
            float buggyKineticEnergy = 0.5f * _buggyRigidBody.mass * Mathf.Pow(relativeVelocity, 2);
            float absorbedEnergy = buggyKineticEnergy * EnergyAbsorptionPercentage;
            // Debug.Log($"Absorbed Energy: {absorbedEnergy}");
            // Debug.Log($"Relative velocity collision: {relativeVelocity}");
            AddDeformingForce(localCollisionPoint, absorbedEnergy);
        }
    }
    
    public void AddDeformingForce (Vector3 point, float force)
    {
        float deformationRadius = 1.0f;
        float maxDeformation = 0.1f;
        
        //Add force to vertex
        for (int i = 0; i < _displacedVertices.Length; i++)
        {
            float distance = Vector3.Distance(_displacedVertices[i], point);

            
            if (distance < deformationRadius)
            {
                Vector3 pointToVertex = _displacedVertices[i] - point;
                float attenuatedForce = maxDeformation * force / (1f + pointToVertex.sqrMagnitude);
                float velocity = attenuatedForce * Time.deltaTime;
                _vertexVelocities[i] += pointToVertex.normalized * velocity;
            }
            else
            {
                _vertexVelocities[i] = new Vector3();
            }
        }
    }

    #region private

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
