using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SpellStrike.Core
{
    [Serializable]
    public class SaveData
    {
        public List<string> UnlockedSpellIds = new List<string>();
        public int MaxLevelReached = 1;
    }

    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        public SaveData CurrentData { get; private set; }

        private string m_SavePath;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            m_SavePath = Path.Combine(Application.persistentDataPath, "SpellStrikeSaveData.json");
            LoadGame();
        }

        public void LoadGame()
        {
            if (File.Exists(m_SavePath))
            {
                try
                {
                    string json = File.ReadAllText(m_SavePath);
                    CurrentData = JsonUtility.FromJson<SaveData>(json);
                    if (CurrentData == null)
                    {
                        CurrentData = new SaveData();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("[SaveManager] Error loading save: " + e.Message);
                    CurrentData = new SaveData();
                }
            }
            else
            {
                // New game
                CurrentData = new SaveData();
                // Phép mặc định (AreaLob / Fireball)
                CurrentData.UnlockedSpellIds.Add("SPELL_FIREBALL");
            }
        }

        public void SaveGame()
        {
            try
            {
                string json = JsonUtility.ToJson(CurrentData, true);
                File.WriteAllText(m_SavePath, json);
            }
            catch (Exception e)
            {
                Debug.LogError("[SaveManager] Error saving game: " + e.Message);
            }
        }

        public void UnlockSpell(string _spellId)
        {
            if (!CurrentData.UnlockedSpellIds.Contains(_spellId))
            {
                CurrentData.UnlockedSpellIds.Add(_spellId);
                SaveGame();
            }
        }
    }
}
