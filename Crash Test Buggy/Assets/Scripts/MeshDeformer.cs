using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//https://catlikecoding.com/unity/tutorials/mesh-deformation/
//Tipp: Could be used ANY Mesh object to use for deformation
[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{
    private Mesh _deformingMesh;
    private Vector3[] _originalVertices, _displacedVertices, _vertexVelocities;
    
    public float springForce = 20f;
    public float damping = 5f;
    public bool isOneTimeChange;
    
    // Start is called before the first frame update
    void Start()
    {
        //TODO can not access mesh of buggy-GameObject...
        //https://forum.unity.com/threads/mesh-read-write-enable-checkbox-missing.1286540/
        _deformingMesh = GameObject.Find("buggy").GetComponent<MeshFilter>().mesh;
        _originalVertices = _deformingMesh.vertices;
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
        //TODO Check if this is correct, see if needed further modification for crashes...
        if (isOneTimeChange)
        {
            for (int i = 0; i < _displacedVertices.Length; i++)
            {
                UpdateVertex(i);
            }
            
            _deformingMesh.vertices = _displacedVertices;
            _deformingMesh.RecalculateNormals();
            _vertexVelocities = new Vector3[_originalVertices.Length];
        }
    }
    
    public void AddDeformingForce (Vector3 point, float force) 
    {
        for (int i = 0; i < _displacedVertices.Length; i++)
        {
            AddForceToVertex(i, point, force);
        }
        
        // Debug.DrawLine(Camera.main.transform.position, point);
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
        isOneTimeChange = false;
    }
    
    #endregion
}
