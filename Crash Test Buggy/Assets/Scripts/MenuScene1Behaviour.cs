using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScene1Behaviour : MonoBehaviour
{
    public WheelCollider wheelColliderFL;
    public WheelCollider wheelColliderFR;
    public WheelCollider wheelColliderRL;
    public WheelCollider wheelColliderRR;
    public GameObject buggy;
    
    public Slider sliDeformRadius;
    public Slider sliMaxDeform;
    public Toggle tglEllipticDeform;
    
    //Load skins for buggy on script directly
    public Material bfhSkin;
    public Material standardSkin;
    
    public TMP_Text txtDeformRadius;
    public TMP_Text txtMaxDeform;
    private Prefs _prefs;
    
    // Start is called before the first frame update
    void Start()
    {
        _prefs = new Prefs();
        _prefs.Load();
        
        //Set Material for buggy before SetAll()
        _prefs.bfhSkin = bfhSkin;
        _prefs.standardSkin = standardSkin;
        
        _prefs.SetAll(ref wheelColliderFL, ref wheelColliderFR, ref wheelColliderRL, ref wheelColliderRR, ref buggy);
    }
    
    public void OnSliderChangedDeformRadius(float deformRadius)
    {
        txtDeformRadius.text = sliDeformRadius.value.ToString("0.00");
        _prefs.deformRadius = sliDeformRadius.value;
        
        _prefs.SetBuggyDeformRadius(ref buggy);
    }
    
    public void OnSliderChangedMaxRadius(float maxDeform)
    {
        txtMaxDeform.text = sliMaxDeform.value.ToString("0.00");
        _prefs.maxDeform = sliMaxDeform.value;
        
        _prefs.SetBuggyMaxDeform(ref buggy);
    }
    
    public void OnCheckBoxChangedEllipticDeform(bool isEllipticDeform)
    {
        _prefs.isEllipticDeform = tglEllipticDeform.isOn;
        _prefs.SetBuggyEllipticDeformation(ref buggy);
    }
    
    public void OnBtnBackToStartMenuClick()
    {
        buggy.GetComponent<CarBehaviour>().StopFmodEngineSound();
        _prefs.Save();
        SceneManager.LoadScene("SceneMenu");
    }

    public float GetSpringForceSaved()
    {
        return _prefs.suspensionSpringForce;
    }
    
    void OnApplicationQuit() { _prefs.Save(); }
}
