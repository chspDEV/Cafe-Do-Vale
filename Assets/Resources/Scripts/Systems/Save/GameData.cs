using UnityEngine;
using System;
using System.Collections.Generic;

namespace Tcp4
{
    [System.Serializable]
    public class GameData
    {
        public int money;
        public float stars;
        public float gameTime;
        public SerializableVector3 playerPosition;
        public List<SavedSeed> seedInventoryItems = new();
        public List<SavedStorageData> storageAreaItems = new();
        public List<SavedStorageItem> playerBagItems = new();
        public List<SavedQuest> questData = new();

        // NOVO: Dados dos trabalhadores
        public List<SavedWorkerData> hiredWorkersData = new();
    }

    [System.Serializable]
    public struct SerializableVector3
    {
        public float x, y, z;

        public SerializableVector3(Vector3 vec)
        {
            x = vec.x;
            y = vec.y;
            z = vec.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

    [System.Serializable]
    public class SavedWorkerData
    {
        public string workerName;
        public WorkerType type;
        public float efficiency;
        public int dailyCost;
        public float pauseChance;
        public float workDuration;
        public float restDuration;
        public int workerID;
        public SerializableVector3 homePosition;
    }

    [System.Serializable]
    public class SavedSeed
    {
        public string productionName;
        public int quantity;
    }

    [System.Serializable]
    public class SavedStorageItem
    {
        public string productName;
        public int quantity;
    }

    [System.Serializable]
    public class SavedStorageData
    {
        public string storageID;
        public List<SavedStorageItem> items = new();
    }

    [System.Serializable]
    public class SavedQuest
    {
        public string questID;
        public bool isCompleted;
        public bool isStarted;
        public List<SavedQuestStep> steps = new();
    }

    [System.Serializable]
    public class SavedQuestStep
    {
        public int stepIndex;
        public bool isCompleted;
    }
}