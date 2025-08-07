// Configura��o para cada som ambiente
using System.Collections.Generic;
using UnityEngine;

// Enum para os diferentes ambientes
public enum AmbientType
{
    None,
    Fazenda,
    Cafeteria,
    Floresta,
}


[System.Serializable]
public class AmbientSoundConfig
{
    public AmbientType ambientType;

    [Tooltip("IDs de �udio sem prefixo (ost_ ser� adicionado automaticamente)")]
    public List<string> audioIDs = new List<string>();

    [Range(0f, 1f)]
    public float volume = 0.5f;
    [Range(-3f, 3f)]
    public float pitch = 1f;

    [Header("Transi��o")]
    [Range(0.1f, 5f)]
    public float fadeInDuration = 1f;
    [Range(0.1f, 5f)]
    public float fadeOutDuration = 1f;
}

