using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prefs
{
    public float suspensionDistance;
    public float suspensionSpringForce;
    public float suspensionDamperForce;
    
    public float forwardStiffness;
    public float sidewaysStiffness;
    public float maxTorque;
    
    public bool isRocketVisible;
    public bool isGunVisible;
    public bool isCageVisible;
    public bool isCanistersVisible;
    public bool isFrontLampsVisible;
    public bool isBackSeatVisible;
    
    public string selectedSkin;
    public Material bfhSkin;
    public Material starndardSkin;

    public float buggyHue;
    public float buggySaturation;
    public float buggyValue;
    
    public void Load()
    { 
        suspensionDistance = PlayerPrefs.GetFloat("suspensionDistance", 0.2f);
        suspensionSpringForce = PlayerPrefs.GetFloat("suspensionSpringForce", 35000f);
        suspensionDamperForce = PlayerPrefs.GetFloat("suspensionDamperForce", 4000f);
        forwardStiffness = PlayerPrefs.GetFloat("forwardStiffness", 1.0f);
        sidewaysStiffness = PlayerPrefs.GetFloat("sidewaysStiffness", 1.0f);
        maxTorque = PlayerPrefs.GetFloat("maxTorque", 1500f);

        isRocketVisible = PlayerPrefs.GetInt("isRocketVisible", 1) != 0;
        isGunVisible = PlayerPrefs.GetInt("isGunVisible", 1) != 0;
        isCageVisible = PlayerPrefs.GetInt("isCageVisible", 1) != 0;
        isCanistersVisible = PlayerPrefs.GetInt("isCanistersVisible", 1) != 0;
        isFrontLampsVisible = PlayerPrefs.GetInt("isFrontLampsVisible", 1) != 0;
        isBackSeatVisible = PlayerPrefs.GetInt("isBackSeatVisible", 1) != 0;
        
        selectedSkin = PlayerPrefs.GetString("selectedSkin", "bfhSkin");
        
        buggyHue = PlayerPrefs.GetFloat("buggyHue", 0f);
        buggySaturation = PlayerPrefs.GetFloat("buggySaturation", 0f);
        buggyValue = PlayerPrefs.GetFloat("buggyValue", 1.0f);
    }
    
    public void Save()
    { 
        PlayerPrefs.SetFloat("suspensionDistance", suspensionDistance);
        PlayerPrefs.SetFloat("suspensionSpringForce", suspensionSpringForce);
        PlayerPrefs.SetFloat("suspensionDamperForce", suspensionDamperForce);
        PlayerPrefs.SetFloat("forwardStiffness", forwardStiffness);
        PlayerPrefs.SetFloat("sidewaysStiffness", sidewaysStiffness);
        PlayerPrefs.SetFloat("maxTorque", maxTorque);

        PlayerPrefs.SetInt("isRocketVisible", isRocketVisible ? 1 : 0);
        PlayerPrefs.SetInt("isGunVisible", isGunVisible ? 1 : 0);
        PlayerPrefs.SetInt("isCageVisible", isCageVisible ? 1 : 0);
        PlayerPrefs.SetInt("isCanistersVisible", isCanistersVisible ? 1 : 0);
        PlayerPrefs.SetInt("isFrontLampsVisible", isFrontLampsVisible ? 1 : 0);
        PlayerPrefs.SetInt("isBackSeatVisible", isBackSeatVisible ? 1 : 0);
        
        PlayerPrefs.SetString("selectedSkin", selectedSkin);
        
        PlayerPrefs.SetFloat("buggyHue", buggyHue);
        PlayerPrefs.SetFloat("buggySaturation", buggySaturation);
        PlayerPrefs.SetFloat("buggyValue", buggyValue);
    }
    
    public void SetAll(
        ref WheelCollider wheelFL, 
        ref WheelCollider wheelFR, 
        ref WheelCollider wheelRL, 
        ref WheelCollider wheelRR,
        ref GameObject buggy)
    { 
        SetWheelColliderSuspension(ref wheelFL, ref wheelFR, ref wheelRL, ref wheelRR);
        SetWheelColliderSuspensionSpring(ref wheelFL, ref wheelFR, ref wheelRL, ref wheelRR);
        SetWheelColliderSuspensionDamper(ref wheelFL, ref wheelFR, ref wheelRL, ref wheelRR);
        SetWheelColliderForwardStiffness(ref wheelFL, ref wheelFR, ref wheelRL, ref wheelRR);
        SetWheelColliderSidewaysStiffness(ref wheelFL, ref wheelFR, ref wheelRL, ref wheelRR);
        SetBuggyMaxTorque(ref buggy);
        
        SetBuggyRocketsVisible(ref buggy);
        SetBuggyGunVisible(ref buggy);
        SetBuggyCageVisible(ref buggy);
        SetBuggyCanistersVisible(ref buggy);
        SetBuggyFrontLampsVisible(ref buggy);
        SetBuggyBackSeatVisible(ref buggy);
        
        SetBuggySkin(ref buggy);
        
        SetBuggyColorHSV(ref buggy);
    }

    public void SetWheelColliderSuspension(
        ref WheelCollider wheelFL,
        ref WheelCollider wheelFR,
        ref WheelCollider wheelRL,
        ref WheelCollider wheelRR)
    {
        wheelFL.suspensionDistance = suspensionDistance;
        wheelFR.suspensionDistance = suspensionDistance;
        wheelRL.suspensionDistance = suspensionDistance;
        wheelRR.suspensionDistance = suspensionDistance;
    }

    public void SetWheelColliderSuspensionSpring(
        ref WheelCollider wheelFL,
        ref WheelCollider wheelFR,
        ref WheelCollider wheelRL,
        ref WheelCollider wheelRR)
    {
        JointSpring suspSpring = wheelFL.suspensionSpring;
        suspSpring.spring = suspensionSpringForce;
        
        wheelFL.suspensionSpring = suspSpring;
        wheelFR.suspensionSpring = suspSpring;
        wheelRL.suspensionSpring = suspSpring;
        wheelRR.suspensionSpring = suspSpring;
    }
    
    public void SetWheelColliderSuspensionDamper(
        ref WheelCollider wheelFL,
        ref WheelCollider wheelFR,
        ref WheelCollider wheelRL,
        ref WheelCollider wheelRR)
    {
        JointSpring suspSpring = wheelFL.suspensionSpring;
        
        suspSpring.damper = suspensionDamperForce;
        wheelFL.suspensionSpring = suspSpring;
        wheelFR.suspensionSpring = suspSpring;
        wheelRL.suspensionSpring = suspSpring;
        wheelRR.suspensionSpring = suspSpring;
    }

    public void SetWheelColliderForwardStiffness(
        ref WheelCollider wheelFL,
        ref WheelCollider wheelFR,
        ref WheelCollider wheelRL,
        ref WheelCollider wheelRR)
    {
        WheelFrictionCurve fwWFC = wheelFL.forwardFriction;
        fwWFC.stiffness = forwardStiffness;
        
        wheelFL.forwardFriction = fwWFC;
        wheelFR.forwardFriction = fwWFC;
        wheelRL.forwardFriction = fwWFC;
        wheelRR.forwardFriction = fwWFC;
    }
    
    public void SetWheelColliderSidewaysStiffness(
        ref WheelCollider wheelFL,
        ref WheelCollider wheelFR,
        ref WheelCollider wheelRL,
        ref WheelCollider wheelRR)
    {
        WheelFrictionCurve swWFC = wheelFL.sidewaysFriction;
        swWFC.stiffness = sidewaysStiffness;
        
        wheelFL.sidewaysFriction = swWFC;
        wheelFR.sidewaysFriction = swWFC;
        wheelRL.sidewaysFriction = swWFC;
        wheelRR.sidewaysFriction = swWFC;
    }

    public void SetBuggyMaxTorque(ref GameObject buggy)
    {
        CarBehaviour carScript = buggy.GetComponent<CarBehaviour>();
        carScript.maxTorque = maxTorque;
    }
    
    public void SetBuggyRocketsVisible(ref GameObject buggy)
    {
        Transform rocketR = buggy.gameObject.transform.Find("RocketLauncherR");
        Transform rocketL = buggy.gameObject.transform.Find("RocketLauncherL");
        rocketR.GetComponent<Renderer>().enabled = isRocketVisible;
        rocketL.GetComponent<Renderer>().enabled = isRocketVisible;
    }
    
    public void SetBuggyGunVisible(ref GameObject buggy)
    {
        Transform gun = buggy.gameObject.transform.Find("RoofGun");
        Transform gunJoint = buggy.gameObject.transform.Find("RoofGunJoint");
        gun.GetComponent<Renderer>().enabled = isGunVisible;
        gunJoint.GetComponent<Renderer>().enabled = isGunVisible;
    }
    
    public void SetBuggyCageVisible(ref GameObject buggy)
    {
        Transform cage = buggy.gameObject.transform.Find("Cage");
        cage.GetComponent<Renderer>().enabled = isCageVisible;
    }
    
    public void SetBuggyCanistersVisible(ref GameObject buggy)
    {
        Transform gun = buggy.gameObject.transform.Find("Canisters");
        gun.GetComponent<Renderer>().enabled = isCanistersVisible;
    }
    
    public void SetBuggyFrontLampsVisible(ref GameObject buggy)
    {
        Transform gun = buggy.gameObject.transform.Find("FrontLamps");
        gun.GetComponent<Renderer>().enabled = isFrontLampsVisible;
    }
    
    public void SetBuggyBackSeatVisible(ref GameObject buggy)
    {
        Transform gun = buggy.gameObject.transform.Find("BackSeat");
        gun.GetComponent<Renderer>().enabled = isBackSeatVisible;
    }
    
    public void SetBuggySkin(ref GameObject buggy)
    {
        Transform buggyTransform = buggy.gameObject.transform.Find("buggy");
        Renderer buggyRenderer = buggyTransform.GetComponent<Renderer>();

        switch (selectedSkin)
        {
            case "bfhSkin":
                buggyRenderer.material = bfhSkin;
                break;
            case "standardSkin":
                buggyRenderer.material = starndardSkin;
                break;
        }

    }
    
    public void SetBuggyColorHSV(ref GameObject buggy)
    {
        Transform buggyGameObject = buggy.gameObject.transform.Find("buggy");
        Renderer buggyRenderer = buggyGameObject.GetComponent<Renderer>();

        buggyRenderer.material.color = Color.HSVToRGB(buggyHue, buggySaturation, buggyValue);
    }
}
