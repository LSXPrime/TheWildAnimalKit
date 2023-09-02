using System;
using System.Collections.Generic;

namespace LWOS
{
    [Serializable]
    public struct SaveData
    {
        public int Level;
        public int Wildness;
        public int Attack;
        public int Defence;
        public int ExtraAttack;
        public int ExtraDefence;
        public int Exp;
        public int ExpToNextLevel;
        public int StatusPoints;
        public int SpawnPoint;

        public float Health;
        public float Hunger;
        public float Thirst;
        public float Stamina;
        public float MaxHealth;
        public float MaxStamina;


        public int PackLevel;
        public int PackExp;
        public int PackMaxCount;
        public List<AISaveData> PackData;
    }

    [Serializable]
    public struct AISaveData
    {
        public int PackID;
        public float Health;
        public float Hunger;
        public float Thirst;
        public float Stamina;
    }
}
