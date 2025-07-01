using UnityEngine;
using System.Collections.Generic;

public class DisableFogCamera : MonoBehaviour
{
    [Tooltip("Lista de câmeras que não devem renderizar fog")]
    public List<Camera> camerasWithoutFog = new List<Camera>();

    private Dictionary<Camera, FogOverride> cameraOverrides = new Dictionary<Camera, FogOverride>();
    private bool globalFogState;

    void Start()
    {
        // Salva o estado global inicial do fog
        globalFogState = RenderSettings.fog;
        InitializeFogOverrides();
    }

    void InitializeFogOverrides()
    {
        foreach (var camera in camerasWithoutFog)
        {
            if (camera != null && !cameraOverrides.ContainsKey(camera))
            {
                var fogOverride = camera.gameObject.AddComponent<FogOverride>();
                cameraOverrides[camera] = fogOverride;
            }
        }
    }

    void OnDestroy()
    {
        CleanupOverrides();
    }

    void CleanupOverrides()
    {
        foreach (var pair in cameraOverrides)
        {
            if (pair.Value != null)
            {
                Destroy(pair.Value);
            }
        }
        cameraOverrides.Clear();

        // Restaura o estado global do fog
        RenderSettings.fog = globalFogState;
    }

    // Método para adicionar câmeras dinamicamente
    public void AddCamera(Camera camera)
    {
        if (!camerasWithoutFog.Contains(camera))
        {
            camerasWithoutFog.Add(camera);
            if (!cameraOverrides.ContainsKey(camera))
            {
                var fogOverride = camera.gameObject.AddComponent<FogOverride>();
                cameraOverrides[camera] = fogOverride;
            }
        }
    }

    // Método para remover câmeras
    public void RemoveCamera(Camera camera)
    {
        if (camerasWithoutFog.Contains(camera))
        {
            camerasWithoutFog.Remove(camera);
            if (cameraOverrides.TryGetValue(camera, out var overrideComp))
            {
                Destroy(overrideComp);
                cameraOverrides.Remove(camera);
            }
        }
    }
}