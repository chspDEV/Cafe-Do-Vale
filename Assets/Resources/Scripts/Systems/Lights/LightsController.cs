using NUnit.Framework;
using System;
using System.Collections.Generic;
using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine;

public class LightsController : MonoBehaviour
{
    public List<Light> lights;
    public float turnOffHour = 7f;
    public float turnOnHour = 17f;

    private void Start()
    {
        TimeManager.Instance.OnTimeChanged += CheckActiveLights;

        ActivateLights();
    }

    private void CheckActiveLights(float time)
    {
        if(time == turnOnHour) ActivateLights();
        else if (time == turnOffHour) DeactivateLights();
    }

    public void ActivateLights()
    {
        foreach (var item in lights)
        {
            item.enabled = true;
        }
    }

    public void DeactivateLights()
    {
        foreach (var item in lights)
        {
            item.enabled = false;
        }
    }
}