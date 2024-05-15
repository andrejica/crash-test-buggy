using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CarBehaviour : MonoBehaviour
{
    public WheelCollider wheelColliderFL;
    public WheelCollider wheelColliderFR;
    public WheelCollider wheelColliderRL;
    public WheelCollider wheelColliderRR;
    private readonly float _antiRoll = 5000;
    
    public Transform steeringWheel;
    private float _maxSteeringWheelAngle = 90;
    private float _steerWheelXPos;
    private float _steerWheelZPos;
    public float maxTorque = 1500;
    public float maxSteerAngle = 45;
    private float _testAngle;
    public float sidewaysStiffness = 1.5f;
    public float forewardStiffness = 1.5f;

    public float maxSpeedKMH = 150;
    public float maxSpeedBackwardKMH = 30;
    private float _currentSpeedKMH = 0f;

    public GameObject thirdPersonCamera;
    public GameObject firstPersonCamera;
    private bool _isFirstPerson = false;

    public RectTransform speedPointerTransform;
    public TMP_Text speedText;
    private float _tachOnZeroSpeedDeg = -34;
    private float _tachoMaxDeg = -292;

    private TimingBehaviour _timerScript;
    private List<GameObject> _checkPoints;
    private int _passedCheckpoints = 0;

    public Transform centerOfMass;
    private Transform _transform;
    private Rigidbody _rigidBody;
    
    public AudioClip engineSingleRpmSoundClip;
    private AudioSource _engineAudioSource;
    private FMODUnity.StudioEventEmitter _engineEventEmitter;
    public bool useFMODEngineSound = true;
    
    // Full breaking and skidmarking
    public float fullBrakeTorque = 5000;
    public float maxBrakeTorque = 10000;
    public AudioClip brakeAudioClip;
    private bool _doSkidmarking;
    private bool _carIsNotOnSand;
    private bool _carIsSliding;
    private AudioSource _brakeAudioSource;
    public WheelBehaviour[] wheelBehaviours = new WheelBehaviour[4];
    
    public ParticleSystem smokeL;
    public ParticleSystem smokeR;
    public ParticleSystem dustFL;
    public ParticleSystem dustFR;
    public ParticleSystem dustRL;
    public ParticleSystem dustRR;
    private Transform _dustFlTransform;
    private Transform _dustFrTransform;
    private Transform _dustRlTransform;
    private Transform _dustRrTransform;
    private ParticleSystem.EmissionModule _smokeLEmission;
    private ParticleSystem.EmissionModule _smokeREmission;
    private ParticleSystem.EmissionModule _dustFLEmission;
    private ParticleSystem.EmissionModule _dustFREmission;
    private ParticleSystem.EmissionModule _dustRLEmission;
    private ParticleSystem.EmissionModule _dustRREmission;
    
    private bool _carIsOnDrySand;
    private string _groundTagFL;
    private string _groundTagFR;
    private int _groundTextureFL;
    private int _groundTextureFR;

    public bool thrustEnabled;

    class Gear
    {
        public Gear(float minKMH, float minRPM, float maxKMH, float maxRPM)
        { _minRPM = minRPM;
            _minKMH = minKMH;
            _maxRPM = maxRPM;
            _maxKMH = maxKMH;
        }
        private float _minRPM;
        private float _minKMH;
        private float _maxRPM;
        private float _maxKMH;
        
        public bool SpeedFits(float kmh)
        {
            return kmh >= _minKMH && kmh <= _maxKMH;
        }
        
        public float Interpolate(float kmh)
        {
            float currentRpm;
            
             currentRpm = kmh / _maxKMH * _maxRPM;
             // Debug.Log($"current RPM calculated & Speed: RPM {currentRpm} / Speed {kmh}");

             if (currentRpm > _minRPM)
             {
                 return currentRpm;
             }
            
            return _minRPM;
        }
    }
    
    float KmhToRpm(float kmh, out int gearNum)
    {
        Gear[] gears =
        { new Gear( 1, 900, 12, 1400),
            new Gear( 12, 900, 25, 2000),
            new Gear( 25, 1350, 45, 2500),
            new Gear( 45, 1950, 70, 3500),
            new Gear( 70, 2500, 112, 4000),
            new Gear(112, 3100, 180, 5000)
        };
        
        for (int i=0; i< gears.Length; ++i)
        { if (gears[i].SpeedFits(kmh))
            { gearNum = i + 1;
                return gears[i].Interpolate(kmh);
            }
        }
        gearNum = 1;
        return 800;
    }
    
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _transform = GetComponent<Transform>();
        var localPositionCenterOfMass = centerOfMass.localPosition;
        _rigidBody.centerOfMass = new Vector3(localPositionCenterOfMass.x, 
            localPositionCenterOfMass.y,
            localPositionCenterOfMass.z);
        // SetWheelFrictionStiffness(forewardStiffness, sidewaysStiffness);
        
        _steerWheelXPos = steeringWheel.rotation.eulerAngles.x;
        _steerWheelZPos = steeringWheel.rotation.eulerAngles.z;
        
        if (useFMODEngineSound)
        { 
            // Setup FMOD event emitter
            _engineEventEmitter = GetComponent<FMODUnity.StudioEventEmitter>();
            _engineEventEmitter.EventInstance.setVolume(1f);
            if (SceneManager.GetActiveScene().name == "SceneMenu")
            {
                _engineEventEmitter.EventInstance.setVolume(0.1f);
            }
            _engineEventEmitter.Play();
        }
        else
        {
            // Configure AudioSource component by program
            _engineAudioSource = gameObject.AddComponent<AudioSource>();
            _engineAudioSource.clip = engineSingleRpmSoundClip;
            _engineAudioSource.loop = true;
            _engineAudioSource.volume = 0.7f;
            _engineAudioSource.playOnAwake = true;
            _engineAudioSource.enabled = false; // Bugfix
            _engineAudioSource.enabled = true; // Bugfix
        }
        
        _smokeLEmission = smokeL.emission;
        _smokeREmission = smokeR.emission;
        _smokeLEmission.enabled = true;
        _smokeREmission.enabled = true;

        _dustFlTransform = dustFL.transform;
        _dustFrTransform = dustFR.transform;
        _dustRlTransform = dustRL.transform;
        _dustRrTransform = dustRR.transform;

        _dustFLEmission = dustFL.emission;
        _dustFREmission = dustFR.emission;
        _dustRLEmission = dustRL.emission;
        _dustRREmission = dustRR.emission;

        //Get all checkpoints on the map for the buggy to drive through
        _checkPoints = GameObject.FindGameObjectsWithTag("Checkpoint").ToList();
        List<GameObject> startCheckPoints = GameObject.FindGameObjectsWithTag("StartCheckpoint").ToList();
        if (startCheckPoints.Count > 0)
        {
            _timerScript = startCheckPoints[0].GetComponent<TimingBehaviour>();
        }
        
        // Configure brake audiosource component by program
        _brakeAudioSource = gameObject.AddComponent<AudioSource>();
        _brakeAudioSource.clip = brakeAudioClip;
        _brakeAudioSource.loop = true;
        _brakeAudioSource.volume = 0.7f;
        _brakeAudioSource.playOnAwake = false;
    }
    
    void FixedUpdate ()
    {
        _currentSpeedKMH = _rigidBody.velocity.magnitude * 3.6f;
        
        // Evaluate ground under front wheels
        WheelHit hitFL = GetGroundInfos(ref wheelColliderFL, ref _groundTagFL, ref _groundTextureFL);
        WheelHit hitFR = GetGroundInfos(ref wheelColliderFR, ref _groundTagFR, ref _groundTextureFR);
        _carIsOnDrySand = _groundTagFL.CompareTo("Terrain")==0 && _groundTextureFL==0;
        _carIsNotOnSand = !(_groundTagFL.CompareTo("Terrain")==0 && _groundTextureFL is 0 or 2);
        _carIsSliding = CheckIsCarSliding();
        // Debug.Log($"Buggy is on Sand texture: {_carIsOnDrySand}");
        // Debug.Log($"Buggy left tire on texture: {_groundTextureFL}");

        StabilizeCar();
        
        //Correct angles of both front wheel according to current speed
        float steerAngleCorrection = (1 - _currentSpeedKMH / (maxSpeedKMH * 1.15f)) * maxSteerAngle;
        SetSteerAngle(steerAngleCorrection * Input.GetAxis("Horizontal"));

        BrakeBuggy();
        
        LimitToMaxSpeedBoundaries();
        
        // Debug.Log($"Buggy speed in KM/H: {_currentSpeedKMH}");
        // Debug.Log($"Buggy moves forward: {BuggyMovesForward()}");
        
        int gearNum = 0;
        float engineRpm = KmhToRpm(_currentSpeedKMH, out gearNum);
        // Debug.Log($"current gearNum / RPM / Speed: gearNum {gearNum} / RPM {engineRPM} / Speed {_currentSpeedKMH}");
        SetEngineSound(engineRpm);
        
        // Debug.Log(hitFL.collider.tag);
        SetParticleSystems(engineRpm);
    }

    // Update is called once per frame
    void Update()
    {
        ChangeBuggyCamera();

        float degAroundY = 0;
        // float test1 = steeringWheel.rotation.eulerAngles.x;
        // float test2 = steeringWheel.rotation.eulerAngles.z;
        //TODO let rotation work correctly for steering wheel...
        if (_currentSpeedKMH > 0)
        {
            degAroundY += _maxSteeringWheelAngle * Input.GetAxis("Horizontal");
        }
        // steeringWheel.SetLocalPositionAndRotation(Vector3.up, Quaternion.Euler(_steerWheelXPos,degAroundY, _steerWheelZPos));
        //); = Quaternion.Euler(_steerWheelXPos,degAroundY, _steerWheelZPos);
        steeringWheel.Rotate(Vector3.up, degAroundY);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("StartCheckpoint"))
        { 
            if (!_timerScript.GetIsStarted()) { _timerScript.StartTimer(true); }
        }
        else if (other.gameObject.CompareTag("Checkpoint"))
        {
            _passedCheckpoints += 1;
            GameObject checkPoint;
            (checkPoint = other.gameObject).SetActive(false);
            _timerScript.SaveTimeOnCheckpointPassed(checkPoint);
        }
        else if (other.gameObject.CompareTag("FinishCheckpoint"))
        {
            if (_timerScript.GetIsStarted() && _passedCheckpoints == _checkPoints.Count)
            {
                _timerScript.StartTimer(false);
                _timerScript.FinishRace(true);
                thrustEnabled = false;
                _timerScript.ShowFullRaceTime();
            }
        }
    }
    
    void OnGUI()
    {
        
        float degAroundZ = _tachOnZeroSpeedDeg;
        // Speed pointer rotation
        if (_currentSpeedKMH > 0)
        {
            degAroundZ += _tachoMaxDeg * (_currentSpeedKMH / maxSpeedKMH);
        }
  
        speedPointerTransform.rotation = Quaternion.Euler(0,0, degAroundZ); 
        
        //SpeedText show current KMH
        speedText.text = _currentSpeedKMH.ToString("0") + " km/h";
    }
    
    void SetSteerAngle(float angle)
    { 
        wheelColliderFL.steerAngle = angle;
        wheelColliderFR.steerAngle = angle;
    }
    
    void SetMotorTorque(float amount)
    { 
        wheelColliderFL.motorTorque = amount;
        wheelColliderFR.motorTorque = amount;
    }
    
    void SetWheelFrictionStiffness(float newForwardStiffness, float newSidewaysStiffness)
    {
        WheelFrictionCurve fwWFC = wheelColliderFL.forwardFriction;
        WheelFrictionCurve swWFC = wheelColliderFL.sidewaysFriction;
        fwWFC.stiffness = newForwardStiffness;
        swWFC.stiffness = newSidewaysStiffness;
        wheelColliderFL.forwardFriction = fwWFC;
        wheelColliderFL.sidewaysFriction = swWFC;
        wheelColliderFR.forwardFriction = fwWFC;
        wheelColliderFR.sidewaysFriction = swWFC;
        wheelColliderRL.forwardFriction = fwWFC;
        wheelColliderRL.sidewaysFriction = swWFC;
        wheelColliderRR.forwardFriction = fwWFC;
        wheelColliderRR.sidewaysFriction = swWFC;
    }
    
    void SetEngineSound(float engineRpm)
    {
        if (useFMODEngineSound)
        {
            _engineEventEmitter.SetParameter("RPM", engineRpm); 
        }
        else
        {
            if (ReferenceEquals(_engineAudioSource, null)) return;
            float minRPM = 800;
            float maxRPM = 8000;
            float minPitch = 0.3f;
            float maxPitch = 3.0f;

            float pitch = engineRpm / maxRPM * maxPitch;

            _engineAudioSource.pitch = pitch;
        }
    }

    public void StopFmodEngineSound()
    {
        _engineEventEmitter.Stop();
    }
    
    #region private
    void SetParticleSystems(float engineRpm)
    { 
        float smokeRate = engineRpm / 50.0f;
        _smokeLEmission.rateOverDistance = new ParticleSystem.MinMaxCurve(smokeRate);
        _smokeREmission.rateOverDistance = new ParticleSystem.MinMaxCurve(smokeRate);

        //Let dust particles come from front of tires when driving backwards.
        ChangeDustParticleDirection();
        
        // Set wheels dust
        float dustRate = 0;
        if (_currentSpeedKMH > 10.0f && _carIsOnDrySand) { dustRate = _currentSpeedKMH; }

        // Debug.Log(dustRate);
        _dustFLEmission.rateOverDistance = new ParticleSystem.MinMaxCurve(dustRate);
        _dustFREmission.rateOverDistance = new ParticleSystem.MinMaxCurve(dustRate);
        _dustRLEmission.rateOverDistance = new ParticleSystem.MinMaxCurve(dustRate);
        _dustRREmission.rateOverDistance = new ParticleSystem.MinMaxCurve(dustRate);
    }
    
    WheelHit GetGroundInfos(ref WheelCollider wheelCol, ref string groundTag, ref int groundTextureIndex)
    { 
        // Default values
        groundTag = "InTheAir";
        groundTextureIndex = -1;
        
        // Query ground by ray shoot on the front left wheel collider
        WheelHit wheelHit;
        wheelCol.GetGroundHit(out wheelHit);
        
        // If not in the air query collider
        if (wheelHit.collider)
        { 
            groundTag = wheelHit.collider.tag;
            if (wheelHit.collider.CompareTag("Terrain"))
            {
                groundTextureIndex = TerrainSurface.GetMainTexture(transform.position);
            }
        }
        
        return wheelHit;
    }

    /// <summary>
    /// toggles Buggy camera between third-person-view and first-person-view
    /// </summary>
    private void ChangeBuggyCamera()
    {
        if (!_isFirstPerson && Input.GetKeyDown(KeyCode.K))
        {
            firstPersonCamera.SetActive(true);
            //TODO Get FMOD to change to first person view camera on change...
            thirdPersonCamera.SetActive(false);
            _isFirstPerson = true;
        }
        else if (_isFirstPerson && Input.GetKeyDown(KeyCode.K))
        {
            firstPersonCamera.SetActive(false);
            thirdPersonCamera.SetActive(true);
            _isFirstPerson = false;
        }
    }

    /// <summary>
    /// Check for _currentSpeed and limit for forward and backwards max speed limits
    /// </summary>
    private void LimitToMaxSpeedBoundaries()
    {
        //Disable movement before count down
        if (!thrustEnabled)
        {
            SetMotorTorque(0);
        }
        //Stop speed increase if goes over MaxForward or MaxBackwards Speed
        else if (BuggyMovesForward() && _currentSpeedKMH <= maxSpeedKMH)
        {
            SetMotorTorque(maxTorque * Input.GetAxis("Vertical"));
        }
        else if(!BuggyMovesForward() && _currentSpeedKMH <= maxSpeedBackwardKMH)
        {
            SetMotorTorque(maxTorque * Input.GetAxis("Vertical"));
        }
        else
        {
            SetMotorTorque(0);
        }
    }
    
    private bool BuggyMovesForward()
    {
        Vector3 velocity = _rigidBody.velocity;
        Vector3 localVel = transform.InverseTransformDirection(velocity);

        return localVel.z > 0;
    }

    /// <summary>
    /// Stabilizer for buggy
    /// Used script and adapted to this project from this URL https://forum.unity.com/threads/how-to-make-a-physically-real-stable-car-with-wheelcolliders.50643/
    /// </summary>
    private void StabilizeCar()
    {
        var travelL = 1.0;
        var travelR = 1.0;
 
        var groundedL = wheelColliderFL.GetGroundHit(out var hitFl);
        if (groundedL)
            travelL = (-wheelColliderFL.transform.InverseTransformPoint(hitFl.point).y - wheelColliderFL.radius) / wheelColliderFL.suspensionDistance;
 
        var groundedR = wheelColliderFR.GetGroundHit(out var hitFr);
        if (groundedR)
            travelR = (-wheelColliderFR.transform.InverseTransformPoint(hitFr.point).y - wheelColliderFR.radius) / wheelColliderFR.suspensionDistance;
 
        var antiRollForce = (travelL - travelR) * _antiRoll;
 
        if (groundedL)
        {
            var transformWheelFl = wheelColliderFL.transform;
            _rigidBody.AddForceAtPosition(transformWheelFl.up * (float)-antiRollForce,
                transformWheelFl.position);
        }

        if (groundedR)
        {
            var transformWheelFr = wheelColliderFR.transform;
            _rigidBody.AddForceAtPosition(transformWheelFr.up * (float)antiRollForce,
                transformWheelFr.position);
        }
    }

    /// <summary>
    /// Buggy will brake and stop if pressing opposite direction of current movement
    /// </summary>
    private void BrakeBuggy()
    {
        // Determine if the cursor key input means braking
        bool doBraking = _currentSpeedKMH > 0.5f &&
                         (Input.GetAxis("Vertical") < 0 && BuggyMovesForward() ||
                          Input.GetAxis("Vertical") > 0 && !BuggyMovesForward());

        bool doFullBrake = Input.GetKey("space");
        _doSkidmarking = (_carIsNotOnSand && doFullBrake && _currentSpeedKMH > 20.0f) || (_carIsNotOnSand && _carIsSliding && _currentSpeedKMH > 20.0f);
        SetBrakeSound(_doSkidmarking);
        SetSkidmarking(_doSkidmarking);
        
        if (doBraking || doFullBrake)
        { 
            float brakeTorque = doFullBrake ? fullBrakeTorque : maxBrakeTorque;
            SetBreakTorque(brakeTorque);
            SetMotorTorque(0);
        } 
        else
        { 
            SetBreakTorque(0);
            wheelColliderFL.motorTorque = maxTorque * Input.GetAxis("Vertical");
            wheelColliderFR.motorTorque = wheelColliderFL.motorTorque;
        }
    }

    private void SetBreakTorque(float amount)
    {
        wheelColliderFL.brakeTorque = amount;
        wheelColliderFR.brakeTorque = amount;
        wheelColliderRL.brakeTorque = amount;
        wheelColliderRR.brakeTorque = amount;
    }
    
    private void SetBrakeSound(bool doBrakeSound)
    {
        if (doBrakeSound)
        { _brakeAudioSource.volume = _currentSpeedKMH/100.0f;
            _brakeAudioSource.Play();
        } else
            _brakeAudioSource.Stop();
    }
    
    // Turns skidmarking on or off on all wheels
    void SetSkidmarking(bool doSkidmarking)
    { foreach(var wheel in wheelBehaviours)
        wheel.DoSkidmarking(doSkidmarking);
    }
    
    private void ChangeDustParticleDirection()
    {
        Quaternion directionFrontTires = Quaternion.Euler(new Vector3(0f, 0f, -90));
        Quaternion directionBehindTires = Quaternion.Euler(new Vector3(-180f, 0f, -90));
        
        //When driving backwards change direction on X-Axis for +180 degrees and position in front of tires
        if (!BuggyMovesForward())
        {
            _dustFlTransform.localRotation = directionFrontTires;
            _dustFrTransform.localRotation = directionFrontTires;
            _dustRlTransform.localRotation = directionFrontTires;
            _dustRrTransform.localRotation = directionFrontTires;
        }
        //Change direction back to initial values if driving forwards
        else
        {
            _dustFlTransform.localRotation = directionBehindTires;
            _dustFrTransform.localRotation = directionBehindTires;
            _dustRlTransform.localRotation = directionBehindTires;
            _dustRrTransform.localRotation = directionBehindTires;
        }
    }

    private bool CheckIsCarSliding()
    {
        Vector3 directionBuggy = _transform.forward;
        Vector3 directionBuggyVelocity = _rigidBody.velocity;

        float slidingAngle = Math.Abs(Vector3.SignedAngle(directionBuggyVelocity, directionBuggy, Vector3.up));
        
        // Debug.Log($"Sliding angle buggy: {slidingAngle}");
        // Debug.Log($"Vector velocity buggy: {directionBuggyVelocity}");
        // Debug.Log($"Vector buggy direction: {directionBuggy}");
        
        return slidingAngle >= 35f && slidingAngle <= 100;
    }
    
    #endregion
}
