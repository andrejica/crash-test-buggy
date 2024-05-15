using UnityEngine;

public class WheelBehaviour : MonoBehaviour
{
    public WheelCollider wheelCol;
    private float _springForce;
    private float _maxSpringForce = 100000.0f;
    private float _springForceIntensity;
    private SkidmarkBehaviour _skidmarkBehaviour; // skidmark script
    private int _skidmarkLast; // index of last skidmark
    private Vector3 _skidmarkLastPos; // position of last skidmark
    void Start ()
    {
        //TODO Check why the wrong values are being loaded too late
        // MenuScene1Behaviour menuScene1Behaviour = GameObject.Find("Main Camera").GetComponent<MenuScene1Behaviour>();
        // _springForce = menuScene1Behaviour.GetSpringForceSaved();
        _springForce = wheelCol.suspensionSpring.spring;
        _springForceIntensity = _springForce / _maxSpringForce;
        Debug.Log($"Springforce intensity: {_springForceIntensity}");
        _skidmarkBehaviour = GameObject.Find("Buggy").GetComponent<SkidmarkBehaviour>();
        _skidmarkLast = -1;
    }
    
    // Update is called once per frame
    void Update()
    {
        // Get the wheel position and rotation from the wheel collider
        wheelCol.GetWorldPose(out Vector3 position, out Quaternion rotation);
        var myTransform = transform;
        myTransform.position = position;
        myTransform.rotation = rotation;
    }
    
    // Creates skidmarks if handbraking
    public void DoSkidmarking(bool doSkidmarking)
    {
        if (doSkidmarking)
        {
            // do nothing if the wheel isn't touching the ground
            if(!wheelCol.GetGroundHit(out WheelHit hit)) return;
            // absolute velocity at wheel in world space
            Vector3 wheelVelo =
                wheelCol.attachedRigidbody.GetPointVelocity(hit.point);
            if (Vector3.Distance(_skidmarkLastPos, hit.point) > 0.1f)
            { _skidmarkLast =
                    _skidmarkBehaviour.Add(hit.point + wheelVelo*Time.deltaTime,
                        hit.normal,
                        _springForceIntensity,
                        _skidmarkLast);
                _skidmarkLastPos = hit.point;
            }
        } else _skidmarkLast = -1;
    }
}
