using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;

public class SpawnBuggy : MonoBehaviour
{
    public GameObject buggyCrashTest;

    private bool _oldBuggyDestroyed;
    private bool _waitForNewSpawn;
    private CarBehaviour _oldCarBehaviour;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RemoveBuggy();
        
        // if (_oldBuggyDestroyed)
        // {
        //     SpawnNewBuggy();
        // }
    }
    
    private void RemoveBuggy()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            buggyCrashTest = GameObject.Find("Buggy");
            _oldCarBehaviour = buggyCrashTest.GetComponent<CarBehaviour>();
            
            // CarBehaviour carScript = spawnedBuggy.GetComponent<CarBehaviour>();
            // RectTransform speedPointer = GameObject.Find("SpeedPointer").GetComponent<RectTransform>();
            // TextMeshPro speedText = GameObject.Find("SpeedText").GetComponent<TextMeshPro>();

            // carScript.speedPointerTransform = speedPointer;
            // carScript.speedText = speedText;
            Instantiate(buggyCrashTest, new Vector3(150, 0.5f, 100), Quaternion.identity);
            Destroy(buggyCrashTest);
            
            GameObject mainCamera = GameObject.Find("Main Camera");
            SmoothFollow followScript = mainCamera.GetComponent<SmoothFollow>();
            followScript.target = buggyCrashTest.transform;
            // _oldBuggyDestroyed = true;
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
            
            _oldBuggyDestroyed = false;
            _waitForNewSpawn = false;
        }
        else
        {
            _waitForNewSpawn = true;
        }
    }
}
