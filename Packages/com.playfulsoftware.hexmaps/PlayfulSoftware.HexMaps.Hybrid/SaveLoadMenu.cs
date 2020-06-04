using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    public sealed class SaveLoadMenu : MonoBehaviour
    {
        public HexGrid hexGrid;
        public HexMapCamera hexMapCamera;

        public Text menuLabel;
        public Text actionButtonLabel;

        public InputField nameInput;
        public RectTransform listContent;
        public SaveLoadItem itemPrefab;

        private HexMapCamera m_Camera;

        private bool m_SaveMode;

        void Awake()
        {
            m_Camera = FindObjectOfType<HexMapCamera>();
        }

        public void Action()
        {
            var path = GetSelectedPath();
            if (path == null)
                return;
            if (m_SaveMode)
                Save(path);
            else
                Load(path);
            Close();
        }

        public void Open(bool saveMode)
        {
            m_SaveMode = saveMode;
            if (m_SaveMode)
            {
                if (menuLabel)
                    menuLabel.text = "Save Map";
                if (actionButtonLabel)
                    actionButtonLabel.text = "Save";
            }
            else
            {
                if (menuLabel)
                    menuLabel.text = "Load Map";
                if (actionButtonLabel)
                    actionButtonLabel.text = "Load";
            }
            FillList();
            gameObject.SetActive(true);
            if (hexMapCamera)
                hexMapCamera.Lock();
        }

        public void Close()
        {
            gameObject.SetActive(false);
            if (hexMapCamera)
                hexMapCamera.Unlock();
        }

        public void Delete()
        {
            var path = GetSelectedPath();
            if (path == null)
                return;
            if (File.Exists(path))
                File.Delete(path);
            nameInput.text = "";
            FillList();
        }

        public void SelectItem(string name)
        {
            if (nameInput)
                nameInput.text = name;
        }

        void Load(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"File {path} doesn't exist!");
                return;
            }
            using (var reader = new BinaryReader(File.OpenRead(path)))
            {
                int header = reader.ReadInt32();
                if (header <= 1)
                {
                    hexGrid.Load(reader, header);
                    if (m_Camera)
                        m_Camera.ValidatePosition();
                }
                else
                    Debug.LogWarning($"Unknown map format {header}");
            }
        }

        void Save(string path)
        {
            using (var writer = new BinaryWriter(File.Open(path, FileMode.Create)))
            {
                writer.Write(1);
                hexGrid.Save(writer);
            }
        }

        string DefaultMapPath()
        {
#if UNITY_EDITOR
            const string kPath = "Assets/GameData/Maps";
            if (!Directory.Exists(kPath))
                Directory.CreateDirectory(kPath);
            return kPath;
#else
            return Application.persistentDataPath;
#endif // UNITY_EDITOR
        }

        void FillList()
        {
            if (!listContent)
                return;
            // clear out any previous items
            for (int i = 0; i < listContent.childCount; i++)
                Destroy(listContent.GetChild(i).gameObject);

            var paths =
                Directory.GetFiles(DefaultMapPath(), "*.map");
            Array.Sort(paths);
            foreach (var path in paths)
            {
                var item = Instantiate(itemPrefab, listContent, false);
                item.menu = this;
                item.MapName = Path.GetFileNameWithoutExtension(path);
            }
        }

        string GetSelectedPath()
        {
            var name = nameInput ? nameInput.text : "";
            return name.Length == 0 ? null : Path.Combine(DefaultMapPath(), $"{name}.map");
        }
    }
}