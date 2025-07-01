// Componente que faz o override do fog para cada câmera
using UnityEngine;

public class FogOverride : MonoBehaviour
{
    private bool previousFogState;
    private FogMode previousFogMode;
    private Color previousFogColor;
    private float previousFogDensity;
    private float previousFogStart;
    private float previousFogEnd;

    void OnPreRender()
    {
        // Salva todos os settings atuais
        previousFogState = RenderSettings.fog;
        previousFogMode = RenderSettings.fogMode;
        previousFogColor = RenderSettings.fogColor;
        previousFogDensity = RenderSettings.fogDensity;
        previousFogStart = RenderSettings.fogStartDistance;
        previousFogEnd = RenderSettings.fogEndDistance;

        // Desativa completamente o fog
        RenderSettings.fog = false;
    }

    void OnPostRender()
    {
        // Restaura todos os settings originais
        RenderSettings.fog = previousFogState;
        RenderSettings.fogMode = previousFogMode;
        RenderSettings.fogColor = previousFogColor;
        RenderSettings.fogDensity = previousFogDensity;
        RenderSettings.fogStartDistance = previousFogStart;
        RenderSettings.fogEndDistance = previousFogEnd;
    }
}