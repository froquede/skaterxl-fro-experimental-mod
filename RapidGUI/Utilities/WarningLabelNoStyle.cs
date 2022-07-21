using UnityEngine;

namespace RapidGUI
{
    public static partial class RGUI
    {
        public static string warningLabelPrefix = "<b><color=#f9ca24>";
        public static string warningLabelPostfix = "</color></b>";

        static string WarningLabelModifyLabel(string label) => warningLabelPrefix + label + warningLabelPostfix;

        public static void WarningLabel(string label)
        {
            GUILayout.Label(WarningLabelModifyLabel(label), RGUIStyle.warningLabel);
        }

        public static void WarningLabelNoStyle(string label)
        {
            GUILayout.Label(WarningLabelModifyLabel(label), RGUIStyle.warningLabelNoStyle);
        }
    }
}