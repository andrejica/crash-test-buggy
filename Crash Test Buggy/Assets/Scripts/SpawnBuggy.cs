using UnityEngine;

public class SpawnBuggy : MonoBehaviour
{
    public GameObject buggyCrashTest;
    public GameObject buggyCrashTestPrefab;
    public GameObject mainCamera;
    
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
        RespawnBuggy();
    }
    
    private void RespawnBuggy()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Instantiate new buggy and remove old one
            GameObject newBuggy = Instantiate(buggyCrashTestPrefab, new Vector3(150, 0.5f, 100), Quaternion.identity);
            _oldCarBehaviour.StopFmodEngineSound();
            Destroy(buggyCrashTest);

            //Setup new buggy
            buggyCrashTest = newBuggy;
            MeshDeformer meshDeformerScript = buggyCrashTest.GetComponent<MeshDeformer>();
            CarBehaviour newCarBehaviourScript = buggyCrashTest.GetComponent<CarBehaviour>();
            meshDeformerScript.SetBuggyChassisTransform(GameObject.Find("buggy").GetComponent<Transform>());
            // meshDeformerScript.buggyChassisTransform = test;
            newCarBehaviourScript.speedPointerTransform = _oldCarBehaviour.speedPointerTransform;
            newCarBehaviourScript.speedText = _oldCarBehaviour.speedText;
            buggyCrashTest.name = "Buggy";
            
            //Setup camera values
            SmoothFollow followScript = mainCamera.GetComponent<SmoothFollow>();
            followScript.target = buggyCrashTest.transform;
            MenuScene1Behaviour menuBehaviour = mainCamera.GetComponent<MenuScene1Behaviour>();
            menuBehaviour.buggy = buggyCrashTest;
        }
    }
}
