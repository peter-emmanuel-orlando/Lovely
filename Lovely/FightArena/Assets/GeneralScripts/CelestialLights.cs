using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CelestialLights : _MasterComponent<CelestialLights>
{
    [ShowOnly]
    [SerializeField]
    Transform CelestialLightTransform;

    float lunarDayMultiplier = 0.8f; //the moon and sun dont move in synch, this is how much faster or slower the lunar day is as opposed to the solar day

    public static Light celestialLight
    {
        get
        {
            return RenderSettings.sun;
        }
        set
        {
            RenderSettings.sun = value;
        }
    }
    // Use this for initialization
    void Start()
    {
        if (CelestialLightTransform == null)
            CelestialLightTransform = transform.Find("CelestialLight");
        if (CelestialLightTransform == null)
            CelestialLightTransform = new GameObject("CelestialLight").transform;
        CelestialLightTransform.parent = this.transform;


        var light = CelestialLightTransform.GetComponent<Light>();
        if (!light)
            light = CelestialLightTransform.gameObject.AddComponent<Light>();
        light.type = LightType.Directional;
        celestialLight = light;
    }

    // Update is called once per frame
    void Update()
    {
        //set sun position based on time
        //set moon position based on its own parameters of moonrise & moonset
        //get cloud coverage
        //calculate color temperature and intensity of lighting based on these things
        //galculate ground, below, and sky color for skybox
        var timeOfDay = GameTime.Hour;
        var hoursInDay = GameTime.DaysToHours;
        //  take total elapsed time
    }
    
   

}
