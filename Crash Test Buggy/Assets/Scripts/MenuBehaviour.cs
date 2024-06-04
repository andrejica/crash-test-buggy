using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuBehaviour : MonoBehaviour
{
    public WheelCollider wheelColliderFL;
    public WheelCollider wheelColliderFR;
    public WheelCollider wheelColliderRL;
    public WheelCollider wheelColliderRR;
    public GameObject buggy;
    
    public Slider sliSuspDistance;
    public Slider sliSuspSpringForce;
    public Slider sliSuspDamperForce;
    
    public Slider sliFrictForwForce;
    public Slider sliFrictSideForce;
    public Slider sliMaxTorqueForce;
    
    public Toggle tglRocketsVisible;
    public Toggle tglGunVisible;
    public Toggle tglCageVisible;
    public Toggle tglCanistersVisible;
    public Toggle tglFrontLampsVisible;
    public Toggle tglBackSeatVisible;
    
    //Load Skin on script directly
    public Material bfhSkin;
    public Material standardSkin;
    
    public Slider sliColorHue;
    public Slider sliColorSaturation;
    public Slider sliColorValue;
    
    public TMP_Text txtDistanceNum;
    public TMP_Text txtSpringNum;
    public TMP_Text txtDamperNum;
    public TMP_Text txtFrictForwNum;
    public TMP_Text txtFrictSideNum;
    public TMP_Text txtMaxTorqueNum;
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
        sliSuspDistance.value = _prefs.suspensionDistance;
        sliSuspSpringForce.value = _prefs.suspensionSpringForce;
        sliSuspDamperForce.value = _prefs.suspensionDamperForce;
        
        sliFrictForwForce.value = _prefs.forwardStiffness;
        sliFrictSideForce.value = _prefs.sidewaysStiffness;
        sliMaxTorqueForce.value = _prefs.maxTorque;
        
        tglRocketsVisible.isOn = _prefs.isRocketVisible;
        tglGunVisible.isOn = _prefs.isGunVisible;
        tglCageVisible.isOn = _prefs.isCageVisible;
        tglCanistersVisible.isOn = _prefs.isCanistersVisible;
        tglFrontLampsVisible.isOn = _prefs.isFrontLampsVisible;
        tglBackSeatVisible.isOn = _prefs.isBackSeatVisible;
        
        sliColorHue.value = _prefs.buggyHue;
        sliColorSaturation.value = _prefs.buggySaturation;
        sliColorValue.value = _prefs.buggyValue;
        
        txtDistanceNum.text = sliSuspDistance.value.ToString("0.00");
        txtSpringNum.text = sliSuspSpringForce.value.ToString("0");
        txtDamperNum.text = sliSuspDamperForce.value.ToString("0");
        txtFrictForwNum.text = sliFrictForwForce.value.ToString("0.0");
        txtFrictSideNum.text = sliFrictSideForce.value.ToString("0.0");
        txtMaxTorqueNum.text = sliMaxTorqueForce.value.ToString("0");
    }
    
    public void OnSliderChangedSuspDistance(float suspDistance)
    {
        txtDistanceNum.text = sliSuspDistance.value.ToString("0.00");
        
        _prefs.suspensionDistance = sliSuspDistance.value;
        
        _prefs.SetWheelColliderSuspension(ref wheelColliderFL, ref wheelColliderFR, ref wheelColliderRL, ref wheelColliderRR);
    }

    public void OnSliderChangedSuspSpring(float suspSpring)
    {
        txtSpringNum.text = sliSuspSpringForce.value.ToString("0");
        _prefs.suspensionSpringForce = sliSuspSpringForce.value;
        
        _prefs.SetWheelColliderSuspensionSpring(ref wheelColliderFL, ref wheelColliderFR, ref wheelColliderRL, ref wheelColliderRR);
    }
    
    public void OnSliderChangedSuspDamper(float suspDamper)
    {
        txtDamperNum.text = sliSuspDamperForce.value.ToString("0");
        _prefs.suspensionDamperForce = sliSuspDamperForce.value;
        
        _prefs.SetWheelColliderSuspensionDamper(ref wheelColliderFL, ref wheelColliderFR, ref wheelColliderRL, ref wheelColliderRR);
    }
    
    public void OnSliderChangedForwardStiffness(float forwardStiffness)
    {
        txtFrictForwNum.text = sliFrictForwForce.value.ToString("0.0");
        _prefs.forwardStiffness = sliFrictForwForce.value;
        
        _prefs.SetWheelColliderForwardStiffness(ref wheelColliderFL, ref wheelColliderFR, ref wheelColliderRL, ref wheelColliderRR);
    }
    
    public void OnSliderChangedSidewaysStiffness(float sidewaysStiffness)
    {
        txtFrictSideNum.text = sliFrictSideForce.value.ToString("0.0");
        _prefs.sidewaysStiffness = sliFrictSideForce.value;
        
        _prefs.SetWheelColliderSidewaysStiffness(ref wheelColliderFL, ref wheelColliderFR, ref wheelColliderRL, ref wheelColliderRR);
    }
    
    public void OnSliderChangedMaxTorque(float maxTorque)
    {
        txtMaxTorqueNum.text = sliMaxTorqueForce.value.ToString("0");
        _prefs.maxTorque = sliMaxTorqueForce.value;
        
        _prefs.SetBuggyMaxTorque(ref buggy);
    }
    
    public void OnCheckBoxChangedFeatureRockets(bool isRocketsVisible)
    {
        _prefs.isRocketVisible = tglRocketsVisible.isOn;
        _prefs.SetBuggyRocketsVisible(ref buggy);
    }
    
    public void OnCheckBoxChangedFeatureGun(bool isGunVisible)
    {
        _prefs.isGunVisible = tglGunVisible.isOn;
        _prefs.SetBuggyGunVisible(ref buggy);
    }
    
    public void OnCheckBoxChangedFeatureCage(bool isCageVisible)
    {
        _prefs.isCageVisible = tglCageVisible.isOn;
        _prefs.SetBuggyCageVisible(ref buggy);
    }
    
    public void OnCheckBoxChangedFeatureCanisters(bool isCanistersVisible)
    {
        _prefs.isCanistersVisible = tglCanistersVisible.isOn;
        _prefs.SetBuggyCanistersVisible(ref buggy);
    }
    
    public void OnCheckBoxChangedFeatureFrontLamps(bool isFrontLampsVisible)
    {
        _prefs.isFrontLampsVisible = tglFrontLampsVisible.isOn;
        _prefs.SetBuggyFrontLampsVisible(ref buggy);
    }
    
    public void OnCheckBoxChangedFeatureBackSeat(bool isBackSeatVisible)
    {
        _prefs.isBackSeatVisible = tglBackSeatVisible.isOn;
        _prefs.SetBuggyBackSeatVisible(ref buggy);
    }
    
    public void OnButtonPressedBfhBuggySkin()
    {
        _prefs.selectedSkin = "bfhSkin";
        _prefs.SetBuggySkin(ref buggy);
    }
    
    public void OnButtonPressedStandardSkin()
    {
        _prefs.selectedSkin = "standardSkin";
        _prefs.SetBuggySkin(ref buggy);
    }
    
    public void OnSliderChangedColorHue(float colorHue)
    {
        _prefs.buggyHue = sliColorHue.value;
        _prefs.SetBuggyColorHSV(ref buggy);
    }
    
    public void OnSliderChangedColorSaturation(float colorSaturation)
    {
        _prefs.buggySaturation = sliColorSaturation.value;
        _prefs.SetBuggyColorHSV(ref buggy);
    }
    
    public void OnSliderChangedColorValue(float colorValue)
    {
        _prefs.buggyValue = sliColorValue.value;
        _prefs.SetBuggyColorHSV(ref buggy);
    }
    
    public void OnBtnStartClick()
    {
        _prefs.Save();
        SceneManager.LoadScene("SceneCrashTest");
    }
    
    void OnApplicationQuit() { _prefs.Save(); }
}
