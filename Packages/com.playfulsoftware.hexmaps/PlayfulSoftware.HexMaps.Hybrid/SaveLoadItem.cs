using UnityEngine;
using UnityEngine.UI;

namespace PlayfulSoftware.HexMaps.Hybrid
{
    public sealed class SaveLoadItem : MonoBehaviour
    {
        private string m_MapName;

        public SaveLoadMenu menu;

        public string MapName
        {
            get => m_MapName;
            set
            {
                m_MapName = value;
                transform.GetChild(0).GetComponent<Text>().text = value;
            }
        }

        public void Select()
        {
            if (menu)
                menu.SelectItem(m_MapName);
        }
    }
}