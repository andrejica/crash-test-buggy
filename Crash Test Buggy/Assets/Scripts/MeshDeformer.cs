using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

//https://catlikecoding.com/unity/tutorials/mesh-deformation/
//Tipp: Could be used ANY Mesh object to use for deformation
[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{
    private Mesh _deformedMesh;
    private Vector3[] _originalVertices, _displacedVertices, _vertexVelocities;
    
    private Rigidbody _buggyRigidBody;
    private CarBehaviour _carScript;
    private Transform _buggyChassisTransform;
    
    public bool isOneTimeChange;
    
    // Percentage of kinetic energy absorbed by de car
    private const float EnergyAbsorptionPercentage = 0.7f;
    private const float ForceOffset = 0.1f;
    private const float DeformationThreshold = 0.5f;
    
    public float deformationRadius = 1.0f;
    public float maxDeformation = 0.1f;
    public bool isEllipticDeformation;
    
    // Start is called before the first frame update
    void Start()
    {
        _carScript = gameObject.GetComponent<CarBehaviour>();
        _buggyRigidBody = GetComponent<Rigidbody>();
        
        //https://forum.unity.com/threads/mesh-read-write-enable-checkbox-missing.1286540/
        //Mesh position always at origin
        //https://discussions.unity.com/t/mesh-vertices-position-not-correct/32537
        //https://stackoverflow.com/questions/49104794/modify-vertices-at-runtime
        _buggyChassisTransform = GameObject.Find("buggy").GetComponent<Transform>();
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
            _vertexVelocities = new Vector3[_originalVertices.Length];
            isOneTimeChange = false;
        }
        
        RepairBuggy();
        ResetDistance();
    }

    private void OnCollisionEnter(Collision other)
    {
        
        Vector3 contactCenterPoint = GetCenterOfContact(other.contacts);
        //Place the point a bit further away from initial point
        Vector3 collisionPoint = contactCenterPoint + contactCenterPoint.normalized * ForceOffset;
        Vector3 localCollisionPoint = _buggyChassisTransform.InverseTransformPoint(collisionPoint);
        
        float currentSpeed = _carScript.GetCurrentSpeed();
        float relativeVelocityMS = other.relativeVelocity.magnitude;
        
        //Deform mesh only if buggy drives a certain speed
        if (currentSpeed > 5.0f)
        {
            isOneTimeChange = true;
            float buggyKineticEnergy = 0.5f * _buggyRigidBody.mass * Mathf.Pow(relativeVelocityMS, 2);
            float absorbedEnergy = buggyKineticEnergy * EnergyAbsorptionPercentage;
            // Debug.Log($"Absorbed Energy: {absorbedEnergy}");
            // Debug.Log($"Relative velocity collision: {relativeVelocity}");
            
            
            if (isEllipticDeformation)
            {
                var collisionVelDir = other.relativeVelocity;
                var orthogonalToWall = Quaternion.FromToRotation(collisionVelDir, -other.transform.forward) * collisionVelDir;
                Debug.DrawRay(collisionPoint, orthogonalToWall, Color.magenta, 10.0f);
                AddEllipticDeformingForce(localCollisionPoint, absorbedEnergy, orthogonalToWall.normalized);
            }
            else
            {
                //Deform mesh in spherical direction
                Debug.DrawRay(collisionPoint, other.relativeVelocity, Color.green, 10.0f);
                AddDeformingForce(localCollisionPoint, absorbedEnergy);
            }
        }
    }
    
    /// <summary>
    /// Add deforming force of a given mesh in a spherical form from given collision point and direction force.
    /// Save calculated velocity in variable "_vertexVelocity" for later use.
    /// Altered version of this tutorial: altered version of this tutorial: https://catlikecoding.com/unity/tutorials/mesh-deformation/
    /// </summary>
    /// <param name="collisionPoint"></param>
    /// <param name="absorbedKineticForce"></param>
    private void AddDeformingForce (Vector3 collisionPoint, float absorbedKineticForce)
    {
        //Add force to vertex
        for (int i = 0; i < _displacedVertices.Length; i++)
        {
            float distance = Vector3.Distance(_displacedVertices[i], collisionPoint);
            
            if (distance < deformationRadius)
            {
                Vector3 pointToVertex = _displacedVertices[i] - collisionPoint;
                //Attenuated force (inverse-square law): Fvel = F / (1 + d^2)
                float attenuatedForce = maxDeformation * absorbedKineticForce / (1f + pointToVertex.sqrMagnitude);
                // Change formula for force as acceleration (ignore Mass -> Mass = 1): a = F / m
                // Change in Velocity: Delta-v = a * Delta-t --> Delta-v = F * Delta-t
                float velocity = attenuatedForce * Time.deltaTime;
                Vector3 effectiveVelocity = pointToVertex.normalized * velocity;
                _vertexVelocities[i] = effectiveVelocity;

                if (velocity > DeformationThreshold)
                {
                    DetachCarObjects(effectiveVelocity);
                }
            }
            
        }
    }

    /// <summary>
    /// Add deforming force of a given mesh in an elliptical form from given collision point, direction force
    /// and inverse collision direction normal vector.
    /// Save calculated velocity in variable "_vertexVelocity" for later use.
    /// </summary>
    /// <param name="collisionPoint"></param>
    /// <param name="absorbedKineticForce"></param>
    /// <param name="collisionDir"></param>
    private void AddEllipticDeformingForce(Vector3 collisionPoint, float absorbedKineticForce, Vector3 collisionDir)
    {
        //Add force to vertex
        for (int i = 0; i < _displacedVertices.Length; i++)
        {
            //Adjust deformation based on direction of impact
            float distance = Vector3.Distance(_displacedVertices[i], collisionPoint);
            Vector3 pointToVertex = _displacedVertices[i] - collisionPoint;
            float dotProduct = Vector3.Dot(collisionDir, pointToVertex.normalized);
            //dampen force of deformation if velocity direction gets more orthogonal to direction vector
            float directionalFactor = Mathf.Clamp01(dotProduct + 0.7f);
            
            if (distance < deformationRadius)
            {
                // Attenuated force (inverse-square law): Fvel = F / (1 + d^2)
                float attenuatedForce = maxDeformation * absorbedKineticForce / (1f + pointToVertex.sqrMagnitude);
                // Change formula for force as acceleration (ignore Mass -> Mass = 1): a = F / m
                // Change in Velocity: Delta-v = a * Delta-t --> Delta-v = F * Delta-t
                float velocity = attenuatedForce * Time.deltaTime;
                _vertexVelocities[i] = pointToVertex.normalized * velocity * directionalFactor;
            }
        }
    }

    #region private

    /// <summary>
    /// Restore buggy mesh to original form and reset other variables for calculation.
    /// </summary>
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

    private void ResetDistance()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            transform.position = new Vector3(150.0f, 0.5f, 100.0f);
            transform.rotation = new Quaternion();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            transform.position = new Vector3(150.0f, 0.5f, 10.0f);
            transform.rotation = new Quaternion();
        }
    }

    /// <summary>
    /// Calculate of given contact vector points the center if there is more than one point
    /// </summary>
    /// <param name="contacts"></param>
    /// <returns>
    /// Calculated average Vector3 of all points. If only one point return same point.
    /// 
    /// </returns>
    private Vector3 GetCenterOfContact(ContactPoint[] contacts)
    {
        if (contacts.Length < 1) { return new Vector3(); }
        
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

    //TODO see if other parts can be checked and detached...
    private void DetachCarObjects(Vector3 effectiveVelocity)
    {
        if (effectiveVelocity.magnitude > 0.5f)
        {
            GameObject numberPlate = GameObject.Find("NumberPlate");
            GameObject frontLamps = GameObject.Find("FrontLamps");
            Rigidbody plateRb = numberPlate.GetComponent<Rigidbody>();
            Rigidbody frontLampsRb = frontLamps.GetComponent<Rigidbody>();
            if (plateRb == null)
            {
                plateRb = numberPlate.AddComponent<Rigidbody>();
                numberPlate.AddComponent<BoxCollider>();
            }

            if (frontLampsRb == null)
            {
                frontLampsRb = frontLamps.AddComponent<Rigidbody>();
                frontLamps.AddComponent<BoxCollider>();
            }
            
            plateRb.useGravity = true;
            frontLampsRb.useGravity = true;
        }
    }
    
    #endregion
}
