using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public class SpawnBuggy : MonoBehaviour
{
    public GameObject buggyCrashTest;
    public GameObject buggyCrashTestPrefab;

    // private bool _oldBuggyDestroyed;
    private bool _waitForNewSpawn;
    private CarBehaviour _oldCarBehaviour;
    // Start is called before the first frame update
    void Start()
    {
        buggyCrashTest = GameObject.Find("Buggy");
        _oldCarBehaviour = buggyCrashTest.GetComponent<CarBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        RemoveBuggy();
    }
    
    private void RemoveBuggy()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameObject newBuggy = Instantiate(buggyCrashTestPrefab, new Vector3(150, 0.5f, 100), Quaternion.identity);
            _oldCarBehaviour.StopFmodEngineSound();
            Destroy(buggyCrashTest);

            buggyCrashTest = newBuggy;
            MeshDeformer meshDeformerScript = buggyCrashTest.GetComponent<MeshDeformer>();
            CarBehaviour newCarBehaviourScript = buggyCrashTest.GetComponent<CarBehaviour>();
            meshDeformerScript.SetBuggyChassisTransform(GameObject.Find("buggy").GetComponent<Transform>());
            // meshDeformerScript.buggyChassisTransform = test;
            newCarBehaviourScript.speedPointerTransform = _oldCarBehaviour.speedPointerTransform;
            newCarBehaviourScript.speedText = _oldCarBehaviour.speedText;
            buggyCrashTest.name = "Buggy";
            
            
            
            GameObject mainCamera = GameObject.Find("Main Camera");
            SmoothFollow followScript = mainCamera.GetComponent<SmoothFollow>();
            followScript.target = buggyCrashTest.transform;
        }
    }

    private void SpawnNewBuggy()
    {
        if (_waitForNewSpawn)
        {
            Instantiate(buggyCrashTest, new Vector3(150, 0.5f, 100), Quaternion.identity);

            GameObject spawnedBuggy = GameObject.Find("Buggy_Crash_Test");
            GameObject mainCamera = GameObject.Find("Main Camera");
            SmoothFollow followScript = mainCamera.GetComponent<SmoothFollow>();
            followScript.target = spawnedBuggy.transform;
            
            spawnedBuggy.name = "Buggy";
            
            // _oldBuggyDestroyed = false;
            _waitForNewSpawn = false;
        }
        else
        {
            _waitForNewSpawn = true;
        }
    }
}
