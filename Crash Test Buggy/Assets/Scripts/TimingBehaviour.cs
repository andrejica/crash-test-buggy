using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class TimingBehaviour : MonoBehaviour
{
    public int countMax = 3;
    private int _countDown;
    public AudioClip countDownAudioClip;
    private AudioSource _countDownAudioSource;
    public TMP_Text timeText;
    public TMP_Text startCountDownText;

    private readonly Dictionary<string, float> _timeOfCheckpoints = new();
    private float _startTimeScreen1;
    private float _pastTime = 0;
    private bool _isFinished = false;
    private bool _isStarted = false;

    private CarBehaviour _carScript;
    
    // Use this for initialization
    void Start()
    {
        _carScript = GameObject.Find("Buggy").GetComponent<CarBehaviour>();
        _carScript.thrustEnabled = false;
        
        _countDownAudioSource = gameObject.AddComponent<AudioSource>();
        _countDownAudioSource.clip = countDownAudioClip;
        _countDownAudioSource.volume = 0.45f;

        _startTimeScreen1 = Time.time;
        
        print("Begin Start:" + Time.time);
        StartCoroutine(GameStart());
        print("End Start:" + Time.time);
    }
    
    // GameStart CoRoutine
    IEnumerator GameStart()
    { 
        print(" Begin GameStart:" + Time.time);
        for(_countDown = countMax; _countDown > 0; _countDown--)
        {
            yield return new WaitForSeconds(1);
            
            _countDownAudioSource.Play();
            // startCountDownText.text = _countDown.ToString("0");
            print(" WaitForSeconds:" + Time.time);
        }
        
        //Play final countdown with higher pitch
        _countDownAudioSource.pitch = 1.5f;
        _countDownAudioSource.Play();
        startCountDownText.text = "GO";
        startCountDownText.color = Color.green;
        print(" End GameStart:" + Time.time);

        _carScript.thrustEnabled = true;
    }
    
    void OnGUI ()
    {
        if (_carScript.thrustEnabled)
        {
            if (Time.time > _startTimeScreen1 + 4 && Time.time < _startTimeScreen1 + 5) { startCountDownText.enabled = false; }
            if (_isStarted && !_isFinished) { _pastTime += Time.deltaTime; }
            timeText.text = _pastTime.ToString("0.0") + " sec.";
        }
        else
        {
            timeText.text = _countDown.ToString("0.0") + " sec.";
            startCountDownText.text = _countDown.ToString("0");
        }

        if (_isFinished)
        {
            _carScript.thrustEnabled = false;
        }
    }

    public void SaveTimeOnCheckpointPassed(GameObject checkPoint)
    {
        var checkpointName = checkPoint.transform.parent.gameObject.name;
        var checkpointNumber = Regex.Match(checkpointName, @"\d").Value;
        var cpName = "CP" + checkpointNumber;
        _timeOfCheckpoints.Add(cpName, _pastTime);
    }

    //For debugging, show race time etc. in console
    public void ShowFullRaceTime()
    {
        var listOfRaceTimes = _timeOfCheckpoints.Values.ToList();
        var listOfCheckpoints = _timeOfCheckpoints.Keys.ToList();
        Debug.Log($"Time of race: {_pastTime:0.00}");
        Debug.Log($"Time of Checkpoint-1: {listOfCheckpoints[0]} with {listOfRaceTimes[0]:0.00}");
        Debug.Log($"Time of Checkpoint-2: {listOfCheckpoints[1]} with {listOfRaceTimes[1]:0.00}");
    }

    /// <summary>
    /// Set the isStarted flag to ture or false according to given boolean value
    /// </summary>
    /// <param name="isStarted">If true value is set to true if false the value is set to false</param>
    public void StartTimer(bool isStarted)
    {
        _isStarted = isStarted;
    }

    /// <summary>
    /// Gets the isStarted value
    /// </summary>
    /// <returns>true or false</returns>
    public bool GetIsStarted()
    {
        return _isStarted;
    }

    /// <summary>
    /// Set isFinished flag to true or false according to given boolean value
    /// </summary>
    /// <param name="isFinished">If true value is set to true if false the value is set to false</param>
    public void FinishRace(bool isFinished)
    {
        _isFinished = isFinished;
    }
}
