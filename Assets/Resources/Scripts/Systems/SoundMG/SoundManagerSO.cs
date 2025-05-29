using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Audio;

namespace GameResources.Project.Scripts.Utilities.Audio
{
 [CreateAssetMenu(fileName = "SoundManagerSO", menuName = "Audio/SoundManagerSO")]
 public class SoundManagerSO : ScriptableObject
 {
  [TabGroup("Mixer")]
  public AudioMixer audioMixer;
  [TabGroup("Mixer Configs")]
  public string masterVolumeParam = "MasterVolume";
  [TabGroup("Mixer Configs")]
  public string musicVolumeParam = "MusicVolume";
  [TabGroup("Mixer Configs")]
  public string sfxVolumeParam = "SFXVolume";

  [TabGroup("Paths")]
  public string musicFolderPath = "Assets/Audio/Music";
  [TabGroup("Paths")]
  public string sfxFolderPath = "Assets/Audio/SFX";

  [TabGroup("Pool")]
  public int sfxPoolSize = 10;
  [TabGroup("Pool")]
  public GameObject SFXPrefab;
 }
}
