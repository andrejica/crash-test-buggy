using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    public Transform target; // The target we are following
    public float distance = 10.0f; // The distance in x-z plane to the target
    public float height = 2.0f; // The Height of the camera above target
    public float heightDamping = 2.0f; // How much we damp in height
    public float rotationDamping= 1.0f; // How much we damp in rotation
    private Vector3 _transformPosition; // Vector to hold the transform position
    private Vector3 _targetPosition; // Vector to hold the target position
    
    private void Start() {}
    
    // Gets called after all other update methods
    void LateUpdate()
    {
        // Get the inefficient transform references once in start.
        _transformPosition = transform.position;
        _targetPosition = target.position;
        
        // Early out if we don't have a target
        if (!target) return;
        
        // Calculate the current rotation angles
        float wantedRotationAngle = target.eulerAngles.y;
        if (Input.GetKey(KeyCode.G))
        {
            wantedRotationAngle += 225.0f;
        }
        else
        {
            wantedRotationAngle += 90.0f;
        }
        
        float wantedHeight = target.position.y + height;
        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = _transformPosition.y;
        
        // Damp the rotation around the y-axis by linear interpolating the angle
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping*Time.deltaTime);
        
        // Damp the height by linear interpolating the height
        currentHeight = Mathf.Lerp (currentHeight, wantedHeight, heightDamping*Time.deltaTime);
        
        // Convert the angle into a rotation around the y-axis
        Quaternion currentRotation = Quaternion.Euler(0,currentRotationAngle,0);
        
        // Set the position of the camera on the x-z plane to:
        // distance meters behind the target
        _transformPosition = _targetPosition;
        _transformPosition -= currentRotation * Vector3.forward * distance;
        
        // Set the height of the camera to the interpolated height
        transform.position = new Vector3(_transformPosition.x, currentHeight, _transformPosition.z);
        
        // Always look at the target
        transform.LookAt(target);
    }
}
