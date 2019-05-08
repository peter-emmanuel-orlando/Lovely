using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CelestialBody : MonoBehaviour
{
    private static HashSet<CelestialBody> cBodies = new HashSet<CelestialBody>();
    private static Color averageSky = Color.black;
    private static float averageSkyExponent = 0f;
    private static Color averageHorizonColor = Color.black;
    private static Color averageGroundColor = Color.black;
    private static float averageGroundExponent = 0f;
    private static int lastUpdated = -1;
    private static void GlobalLateUpdate()
    {
        if(Time.frameCount > lastUpdated)
        {
            float count = cBodies.Count;
            //averageSky /= count;
            averageSkyExponent /= count;
            //averageHorizonColor /= count;
            //averageGroundColor /= count;
            averageGroundExponent /= count;

            var skyMat = RenderSettings.skybox;
            skyMat.SetColor("_SkyColor1", averageSky);
            skyMat.SetFloat("_SkyExponent1", averageSkyExponent);
            skyMat.SetColor("_SkyColor2", averageHorizonColor);
            skyMat.SetColor("_SkyColor3", averageGroundColor);
            skyMat.SetFloat("_SkyExponent2", averageGroundExponent);

            float saturation = 0.5f;
            saturation = Mathf.Clamp01(saturation);

            RenderSettings.ambientSkyColor = averageSky*saturation + averageSky.grayscale*(1f-saturation)*Color.white;
            RenderSettings.ambientEquatorColor = averageHorizonColor * saturation + averageHorizonColor.grayscale * (1f - saturation) * Color.white;
            RenderSettings.ambientGroundColor = averageGroundColor * saturation + averageGroundColor.grayscale * (1f - saturation) * Color.white;
            RenderSettings.fogColor = averageHorizonColor * 0.1f;

            averageSky = Color.black;
            averageSkyExponent = 0f;
            averageHorizonColor = Color.black;
            averageGroundColor = Color.black;
            averageGroundExponent = 0f;
        }

        lastUpdated = Time.frameCount;
    }

    //lunar time is based on solar time, but a "lunar day" is shorter or longer than a normal day
    //-1 to +1 
    [SerializeField]
    private float startPointOffset = 0.3f;
    [SerializeField]
    private float dayLengthMultiplier = 0.3f;
    [SerializeField]
    private float celestialDistance = 1000f;
    //-----------------------------------------------
    [ShowOnly]
    [SerializeField]
    Transform celestialLightTransform;
    [ShowOnly]
    [SerializeField]
    private Light celestialLight;
    [SerializeField]
    private AnimationCurve lightIntensity = new AnimationCurve(new Keyframe(0, 1));
    [SerializeField]
    [GradientHDR]
    private Gradient lightColor = new Gradient();
    //-----------------------------------------------
    [SerializeField]
    [GradientHDR]
    private Gradient skyColor = new Gradient();
    [SerializeField]
    private AnimationCurve skyExponent = new AnimationCurve(new Keyframe(0, 1));
    [SerializeField]
    [GradientHDR]
    private Gradient horizonColor = new Gradient();
    [SerializeField]
    [GradientHDR]
    private Gradient groundColor = new Gradient();
    [SerializeField]
    private AnimationCurve groundExponent = new AnimationCurve(new Keyframe(0, 1));

    private Quaternion celestialRotation { get { return Quaternion.Euler(90 + 360 - 360 * ((GameTime.elapsedGameTime * GameTime.SecondsToMinuites * GameTime.MinuitesToHours + startPointOffset*GameTime.DaysToHours*dayLengthMultiplier) % (GameTime.DaysToHours * dayLengthMultiplier)) / (GameTime.DaysToHours * dayLengthMultiplier), 90, 0); } }//euler angles from origin. +z is north, +x iseast    
    //distance * vector * rotation = direction ray. This plus origin position vector (0, 0, 0) = lunar position;
    private Vector3 celestialPosition { get { return celestialRotation * Vector3.back * celestialDistance; } }

    private void Awake()
    {
        cBodies.Add(this);


        if (celestialLightTransform == null)
            celestialLightTransform = transform.Find("CelestialLight");
        if (celestialLightTransform == null)
            celestialLightTransform = new GameObject("CelestialLight").transform;
        celestialLightTransform.parent = this.transform;
        celestialLightTransform.localPosition = Vector3.zero;


        var light = celestialLightTransform.GetComponent<Light>();
        if (!light)
            light = celestialLightTransform.gameObject.AddComponent<Light>();
        light.type = LightType.Directional;
        celestialLight = light;
        light.hideFlags = HideFlags.NotEditable;
    }

    private void Update()
    {
        var norm = (GetHeightNormalized(GameTime.elapsedGameTime) + 1f)/2f;

        transform.position = celestialPosition;
        celestialLightTransform.rotation = celestialRotation;
        celestialLight.color = lightColor.Evaluate(norm);
        celestialLight.intensity = lightIntensity.Evaluate(norm);
        //gameObject.DisplayTextComponent("GetHeightNormalized " + GetHeightNormalized(GameTime.elapsedGameTime), this);

        averageSky += skyColor.Evaluate(norm);
        averageSkyExponent += skyExponent.Evaluate(norm);
        averageHorizonColor += horizonColor.Evaluate(norm);
        averageGroundColor += groundColor.Evaluate(norm);
        averageGroundExponent += groundExponent.Evaluate(norm);
    }
    
    private void LateUpdate()
    {
        GlobalLateUpdate();
    }

    private void OnDestroy()
    {
        cBodies.Remove(this);
    }

    public float GetHeightNormalized(float time)
    {
        //cos function with a period of how many hours in a day(scaled by lunar factor), starting at high lunar noon (0)
        return Mathf.Cos(((2 * Mathf.PI) / (GameTime.DaysToHours * dayLengthMultiplier)) * ((GameTime.elapsedGameTime * GameTime.SecondsToMinuites * GameTime.MinuitesToHours) + startPointOffset * GameTime.DaysToHours * dayLengthMultiplier/*starting point offset*/));
    }

}
