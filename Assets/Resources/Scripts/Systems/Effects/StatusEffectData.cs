using System;
using UnityEngine;

[Serializable]
public class StatusEffectData
{
    public string effectName;
    public StatusType statusType;
    public float effectValue;
    public float duration;
    public float interval;
    public bool isBuff;
    public bool isContinuous;
    public bool isPermanent;
    public GameObject effectPrefab;

    [NonSerialized] public float elapsedTime;
    [NonSerialized] public float startTime;

    public StatusEffectData(string name, StatusType type, float duration, float interval, GameObject prefab, float value, bool isBuff, bool isContinuous, bool isPermanent = false)
    {
        effectName = name;
        statusType = type;
        this.duration = duration;
        this.interval = interval;
        effectPrefab = prefab;
        effectValue = value;
        elapsedTime = 0;
        startTime = 0;
        this.isBuff = isBuff;
        this.isContinuous = isContinuous;
        this.isPermanent = isPermanent;
    }

    public void UpdateEffect(float deltaTime)
    {
        if (!isPermanent)
        {
            duration -= deltaTime;
        }
        if (isContinuous)
        {
            elapsedTime += deltaTime;
        }
    }

    public bool IsExpired => !isPermanent && duration <= 0;
}
