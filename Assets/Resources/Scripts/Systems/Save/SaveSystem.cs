using Tcp4.Assets.Resources.Scripts.Managers;
using UnityEngine;
using System.IO;

namespace Tcp4
{
    public static class SaveSystem
    {
        private static string path = Application.persistentDataPath + "/savefile.json";

        public static void SaveGame(GameData data)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(path, json);
            Debug.Log("Salvo em: " + path);
        }

        public static GameData LoadGame()
        {
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<GameData>(json);
            }

            Debug.LogWarning("Nenhum save encontrado.");
            return null;
        }

        public static void DeleteSave()
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }
}
