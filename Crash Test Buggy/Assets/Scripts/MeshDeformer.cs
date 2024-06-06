using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//https://catlikecoding.com/unity/tutorials/mesh-deformation/
//Tipp: Could be used ANY Mesh object to use for deformation
[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{
    private Mesh _deformedMesh, _deformedFrontFrameMesh;
    private Vector3[] _originalVertices, _displacedVertices, _vertexVelocities;
    private Vector3[] _originalFrameVertices, _displacedFrameVertices, _vertexFrameVelocities;
    
    private Rigidbody _buggyRigidBody;
    private CarBehaviour _carScript;
    private Transform _buggyChassisTransform, _frontChassisFrameTransform;
    
    public bool isOneTimeChange;
    public bool isFrontFrameChange;
    
    // Percentage of kinetic energy absorbed by de car
    private const float EnergyAbsorptionPercentage = 0.7f;
    private const float ForceOffset = 0.1f;
    // private const float DeformationThreshold = 10.0f;
    
    public float deformationRadius = 1.0f;
    public float maxDeformation = 0.1f;
    public bool isEllipticDeformation;
    
    // Start is called before the first frame update
    void Start()
    {
        _carScript = gameObject.GetComponent<CarBehaviour>();
        _buggyRigidBody = GetComponent<Rigidbody>();
        _buggyChassisTransform = GameObject.Find("buggy").GetComponent<Transform>();
        _frontChassisFrameTransform = GameObject.Find("FrontChassisFrame").GetComponent<Transform>();
        
        //Mesh position always at origin
        //https://forum.unity.com/threads/mesh-read-write-enable-checkbox-missing.1286540/
        //https://discussions.unity.com/t/mesh-vertices-position-not-correct/32537
        //https://stackoverflow.com/questions/49104794/modify-vertices-at-runtime
        //Get Mesh of buggy chassis game object
        _deformedMesh = GameObject.Find("buggy").GetComponent<MeshFilter>().mesh;
        _originalVertices = _deformedMesh.vertices;
        _displacedVertices = new Vector3[_originalVertices.Length];
        
        for (int i = 0; i < _originalVertices.Length; i++)
        {
            _displacedVertices[i] = _originalVertices[i];
        }
        
        _vertexVelocities = new Vector3[_originalVertices.Length];
        
        //Get Mesh of front chassis frame game object
        _deformedFrontFrameMesh = GameObject.Find("FrontChassisFrame").GetComponent<MeshFilter>().mesh;
        _originalFrameVertices = _deformedFrontFrameMesh.vertices;
        _displacedFrameVertices = new Vector3[_originalFrameVertices.Length];
        
        for (int i = 0; i < _originalFrameVertices.Length; i++)
        {
            _displacedFrameVertices[i] = _originalFrameVertices[i];
        }

        _vertexFrameVelocities = new Vector3[_originalFrameVertices.Length];

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

            if (isFrontFrameChange)
            {
                for (int i = 0; i < _displacedFrameVertices.Length; i++)
                {
                    // Update Vertex
                    Vector3 velocity = _vertexFrameVelocities[i];
                    _displacedFrameVertices[i] += velocity * Time.deltaTime;
                }
                
                Debug.DrawRay(_originalFrameVertices[0], _vertexFrameVelocities[0], Color.red, 15.0f);
                _deformedFrontFrameMesh.vertices = _displacedFrameVertices;
                _deformedFrontFrameMesh.RecalculateNormals();
                _vertexFrameVelocities = new Vector3[_originalFrameVertices.Length];
                isFrontFrameChange = false;
            }
            
            isOneTimeChange = false;
        }
        
        RepairBuggyChassis();
        ResetDistance();
    }

    private void OnCollisionEnter(Collision other)
    {
        var contactPoints = other.contacts.Select(cPoint => cPoint.point).ToArray();
        Vector3 contactCenterPoint = GetCenterOfVectors(contactPoints);
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
    /// Altered version of this tutorial: https://catlikecoding.com/unity/tutorials/mesh-deformation/
    /// </summary>
    /// <param name="collisionPoint"></param>
    /// <param name="absorbedKineticForce"></param>
    private void AddDeformingForce (Vector3 collisionPoint, float absorbedKineticForce)
    {
        var velocityInRadius = new List<Vector3>();
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
                velocityInRadius.Add(effectiveVelocity);
            }
        }
        
        DetachCarObjects(velocityInRadius.ToArray());
        
        // if (velocity > DeformationThreshold)
        // {
            // isFrontFrameChange = true;
            // DetachCarObjects();
        // }
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
    private void RepairBuggyChassis()
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
    /// <param name="vectors"></param>
    /// <returns>
    /// Calculated average Vector3 of all points. If only one point return same point.
    /// </returns>
    private Vector3 GetCenterOfVectors(Vector3[] vectors)
    {
        if (vectors.Length < 1) { return new Vector3(); }
        
        if (vectors.Length > 1)
        {
            float totalX = 0.0f, totalY = 0.0f, totalZ = 0.0f;
            
            foreach (var vec in vectors)
            {
                totalX += vec.x;
                totalY += vec.y;
                totalZ += vec.z;
            }

            float centerX = totalX / vectors.Length;
            float centerY = totalY / vectors.Length;
            float centerZ = totalZ / vectors.Length;

            return new Vector3(centerX, centerY, centerZ);
        }
        
        return vectors[0];
    }
    
    private void DetachCarObjects(Vector3[] velocities)
    {
        isFrontFrameChange = true;
        Vector3 averageVertexVelocity = GetCenterOfVectors(velocities);
        float velocity = averageVertexVelocity.magnitude;
        
        if (velocity > 20.0f)
        {
            // Vector3 directionBuggy = GameObject.Find("Buggy").transform.forward;
            
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

            if (velocity > 80.0f)
            {
                GameObject rocketLauncherL = GameObject.Find("RocketLauncherL");
                GameObject rocketLauncherR = GameObject.Find("RocketLauncherR");
                Rigidbody rocketLRb = rocketLauncherL.GetComponent<Rigidbody>();
                Rigidbody rocketRRb = rocketLauncherR.GetComponent<Rigidbody>();
                
                if (rocketLRb == null)
                {
                    rocketLRb = rocketLauncherL.AddComponent<Rigidbody>();
                    rocketLauncherL.AddComponent<BoxCollider>();
                }
            
                if (rocketRRb == null)
                {
                    rocketRRb = rocketLauncherR.AddComponent<Rigidbody>();
                    rocketLauncherR.AddComponent<BoxCollider>();
                }
                
                rocketLRb.useGravity = true;
                rocketRRb.useGravity = true;

                //Todo set front wheels in a slight angle (ca. 10-15 degree)
                // var wheelFl = GameObject.Find("WheelFL");
                // var testCol = Quaternion.Euler(0, 0, 90);
                // var testWheel = Quaternion.Euler(0, 90, 0);
                // var testR = _carScript.wheelColliderFL.transform.rotation;
                // var testRotWheel = wheelFl.transform.rotation;
                // testR *= testCol;
                // testRotWheel *= testWheel;
                // _carScript.wheelColliderFL.transform.rotation = testR;
                // wheelFl.transform.rotation = testRotWheel;
            }

            if (velocity > 130.0f)
            {
                GameObject frontFrame = GameObject.Find("FrontChassisFrame");
                GameObject wheelFl = GameObject.Find("WheelFL");
                GameObject wheelFr = GameObject.Find("WheelFR");
                Rigidbody frontFrameRb = frontFrame.GetComponent<Rigidbody>();
                Rigidbody wheelFlRb = wheelFl.GetComponent<Rigidbody>();
                Rigidbody wheelFrRb = wheelFr.GetComponent<Rigidbody>();
                
                if (frontFrameRb == null)
                {
                    frontFrameRb = frontFrame.AddComponent<Rigidbody>();
                    frontFrame.AddComponent<BoxCollider>();
                }
                
                if (wheelFlRb == null)
                {
                    wheelFlRb = wheelFl.AddComponent<Rigidbody>();
                }
                
                if (wheelFrRb == null)
                {
                    wheelFrRb = wheelFr.AddComponent<Rigidbody>();
                }
            
                frontFrameRb.useGravity = true;
                wheelFlRb.useGravity = true;
                wheelFrRb.useGravity = true;

                _carScript.wheelColliderFL.enabled = false;
                _carScript.wheelColliderFR.enabled = false;
                _carScript.StopFmodEngineSound();
            }
            
            // DeformFrontChassisFrame(localPointFrontFrame, force);

        }
    }

    private Vector3 AddTwoVector(Vector3 vectorDirOne, Vector3 vectorDirTwo)
    {
        return (vectorDirOne + vectorDirTwo).normalized;
    }
    
    //TODO deform front chassis frame according of force of initial impact...
    private void DeformFrontChassisFrame(Vector3 localFrontFramePoint, float force)
    {
        // var testPoint = transformFrame.InverseTransformPoint(transformFrame.localPosition);
        // var testPoint2 = _frontChassisFrameTransform.localPosition;
        Vector3 directionOne = (_frontChassisFrameTransform.right + -_frontChassisFrameTransform.up).normalized;
        var forceDirection = localFrontFramePoint * force;
        // Vector3 directionTwo = (-transformFrame.right + transformFrame.forward).normalized;
        
        for (int i = 0; i < _displacedFrameVertices.Length; i++)
        {
            float distance = Vector3.Distance(_displacedFrameVertices[i], localFrontFramePoint);
            _vertexFrameVelocities[i] += directionOne * force * 0.1f;
        }
    }
    
    #endregion
}
