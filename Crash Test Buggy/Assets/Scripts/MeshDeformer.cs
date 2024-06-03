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
            for (int i = 0; i < _displacedVertices.Length; i++)
            {
                // Update Vertex
                 Vector3 velocity = _vertexVelocities[i];
                 _displacedVertices[i] += velocity * Time.deltaTime;

            }
            
            _deformedMesh.vertices = _displacedVertices;
            _deformedMesh.RecalculateNormals();
            // _deformedMesh.RecalculateBounds();
            _vertexVelocities = new Vector3[_originalVertices.Length];
            isOneTimeChange = false;
        }
        
        RepairBuggy();
    }

    private void OnCollisionEnter(Collision other)
    {
        
        Vector3 collectionPoint = GetCenterOfContact(other.contacts);
        ContactPoint contactPoint = other.GetContact(0);
        Vector3 point = contactPoint.point + contactPoint.point.normalized * 0.1f;
        // Debug.DrawRay(collectionPoint, other.relativeVelocity, Color.red, 30.0f);
        Debug.DrawRay(point, other.relativeVelocity, Color.green, 10.0f);
        Vector3 localCollisionPoint = _buggyChassisTransform.InverseTransformPoint(point);

        // Debug.Log($"Collision world coordinates: {contactPoint.point}");
        // Debug.Log($"Collision local coordinates: {localCollisionPoint}");
        
        float currentSpeed = _carScript.GetCurrentSpeed();
        float relativeVelocityMS = other.relativeVelocity.magnitude;
        

        if (currentSpeed > 5.0f)
        {
            isOneTimeChange = true;
            float buggyKineticEnergy = 0.5f * _buggyRigidBody.mass * Mathf.Pow(relativeVelocityMS, 2);
            float absorbedEnergy = buggyKineticEnergy * EnergyAbsorptionPercentage;
            // Debug.Log($"Absorbed Energy: {absorbedEnergy}");
            // Debug.Log($"Relative velocity collision: {relativeVelocity}");
            var collisionVelDir = other.relativeVelocity;
            var orthogonalToWall = Quaternion.FromToRotation(collisionVelDir, -other.transform.forward) * collisionVelDir;
            Debug.DrawRay(point, orthogonalToWall, Color.magenta, 10.0f);
            AddDeformingForce(localCollisionPoint, absorbedEnergy, orthogonalToWall.normalized);
            // DeformingMesh(localCollisionPoint, absorbedEnergy);
        }
    }
    
    public void AddDeformingForce (Vector3 collisionPoint, float absorbedKineticForce, Vector3 collisionDir)
    {
        float deformationRadius = 0.7f;
        float maxDeformation = 0.05f;
        // float startDistance = 0.0f;
        // float distance;
        
        // Debug.DrawRay(point, _displacedVertices[0], Color.red, 10.0f);
        //Add force to vertex
        for (int i = 0; i < 1; i++)
        {
            for (int j = 0; j < _displacedVertices.Length; j++)
            {
                float distance = Vector3.Distance(_displacedVertices[j], collisionPoint);
                
                if (distance < deformationRadius)
                {
                    Vector3 pointToVertex = _displacedVertices[j] - collisionPoint;
                    //Attenuated force (inverse-square law): Fvel = F / (1 + d^2)
                    float attenuatedForce = maxDeformation * absorbedKineticForce / (1f + pointToVertex.sqrMagnitude);
                    float velocity = attenuatedForce * Time.deltaTime;
                    _vertexVelocities[j] = pointToVertex.normalized * velocity; //* directionalFactor;
                }
                
            }
        }
    }

    #region private

    private void RepairBuggy()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            _deformedMesh.vertices = _originalVertices;
            
            for (int i = 0; i < _originalVertices.Length; i++)
            {
                _displacedVertices[i] = _originalVertices[i];
            }
            
            _vertexVelocities = new Vector3[_originalVertices.Length];
            
            _deformedMesh.RecalculateNormals();
        }
    }

    private Vector3 GetCenterOfContact(ContactPoint[] contacts)
    {
        if (contacts.Length > 1)
        {
            float totalX = 0.0f, totalY = 0.0f, totalZ = 0.0f;
            
            foreach (var contact in contacts)
            {
                totalX += contact.point.x;
                totalY += contact.point.y;
                totalZ += contact.point.z;
            }

            float centerX = totalX / contacts.Length;
            float centerY = totalY / contacts.Length;
            float centerZ = totalZ / contacts.Length;

            return new Vector3(centerX, centerY, centerZ);
        }
        
        return contacts[0].point;
    }
    
    #endregion
}
