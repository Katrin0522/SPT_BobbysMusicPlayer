using UnityEngine;

namespace BobbysMusicPlayer.Utils;

public class SmoothValue
{
    public float Current { get; private set; }
    public float Target { get; private set; }
    public float Speed { get; set; }

    public SmoothValue(float initialValue = 0f, float speed = 5f)
    {
        Current = Target = initialValue;
        Speed = speed;
    }

    public void SetTarget(float newTarget)
    {
        Target = newTarget;
    }

    public void Update(float deltaTime)
    {
        Current = Mathf.Lerp(Current, Target, deltaTime * Speed);
    }
}
