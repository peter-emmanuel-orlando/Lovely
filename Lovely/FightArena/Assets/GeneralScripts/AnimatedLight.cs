using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AnimatedLight : MonoBehaviour
{
    [ShowOnly]
    [SerializeField]
    private Light animLight;
    [ShowOnly]
    [SerializeField]
    private float currentTime;
    [ShowOnly]
    [SerializeField]
    private float completedIterations;
    [ShowOnly]
    [SerializeField]
    protected float evaluationTime;
    [SerializeField]
    private bool reset;

    public Light AnimLight { get { return animLight; } }
    public float CurrentTime { get { return currentTime; } }
    public float NormalizedTime { get { return evaluationTime; } }
    public float CompletedIterations { get { return completedIterations; } }

    [SerializeField]
    public float lifeTime = 10;
    [SerializeField]
    public bool destroyOnComplete = false;
    [SerializeField]
    public PlayMode playMode = PlayMode.Loop;
    [SerializeField]
    public int maxIterations = 1;
    [SerializeField]
    public float maxIntensity = 5f;
    [SerializeField]
    public AnimationCurve intensity = new AnimationCurve(new Keyframe[] {new Keyframe(0, 0.5f), new Keyframe(1, 1)});
    [SerializeField]
    public float maxRange = 2f;
    [SerializeField]
    public AnimationCurve range = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0.5f), new Keyframe(1, 1) });
    [SerializeField]
    public Gradient colorGradient = new Gradient();

    bool isStarted = false;
    bool isPingPongReverse = false;

    protected virtual void Start()
    {
        if(!isStarted)
        {
            isStarted = true;
            Reset();
        }
    }

    protected virtual void Update ()
    {
        if (reset) Reset();

        if(animLight == null)
            animLight = GetComponent<Light>();

        lifeTime = Mathf.Clamp(lifeTime, 0.01f, Mathf.Infinity);
        maxIterations = Mathf.Clamp(maxIterations, -1, int.MaxValue);

        if (currentTime >= lifeTime)
        {
            completedIterations++;
            currentTime %= lifeTime;
            if (maxIterations != -1 && completedIterations >= maxIterations)
            {
                if (destroyOnComplete)
                    Destroy(gameObject);
                else
                    playMode = PlayMode.Paused;
            }
        }


        if (playMode != PlayMode.Paused)
        {
            currentTime += Time.deltaTime;

            if (playMode == PlayMode.Loop)
            {
                evaluationTime += Time.deltaTime;
                evaluationTime %= lifeTime;
            }
            else if (playMode == PlayMode.Reverse)
            {
                evaluationTime -= Time.deltaTime;
                if (evaluationTime < 0)
                    evaluationTime = lifeTime - evaluationTime;
            }
            else if (playMode == PlayMode.Pingpong)
            {
                if (isPingPongReverse)
                {
                    evaluationTime -= Time.deltaTime;
                    if (evaluationTime <= 0)
                        isPingPongReverse = false;
                }
                else
                {
                    evaluationTime += Time.deltaTime;
                    if (evaluationTime >= lifeTime)
                        isPingPongReverse = true;
                }
            }
        }
        Play();

    }

    private void Play()
    {
        var normalizedTime = evaluationTime / lifeTime;
        animLight.color = colorGradient.Evaluate(normalizedTime);
        animLight.intensity = intensity.Evaluate(normalizedTime) * maxIntensity;
        animLight.range = range.Evaluate(normalizedTime) * maxRange;
    }
    
    public virtual void Reset()
    {
        reset = false;
        animLight = GetComponentInChildren<Light>();
        if (animLight == null) throw new UnityException("an AnimatedLight needs a Light somewhere in children!");
        currentTime = 0;
        evaluationTime = 0;
        completedIterations = 0;
        animLight.color = colorGradient.Evaluate(0);
        animLight.intensity = intensity.Evaluate(0) * maxIntensity;
        animLight.range = range.Evaluate(0) * maxRange;
    }

    public enum PlayMode
    {
        Loop,
        Pingpong,
        Reverse,
        Paused
    }

    private void OnDrawGizmosSelected()
    {
        if (!isStarted) Start();
        Update();
    }
}
