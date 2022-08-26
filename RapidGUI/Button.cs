using UnityEngine;

namespace RapidGUI
{
    public static partial class RGUI
    {
        public static class ButtonSetting
        {
            public static float minWidth = 200f;
            public static float fieldWidth = 40f;
        }

        public static bool Button(bool v, string label, params GUILayoutOption[] options)
        {
            GUI.backgroundColor = Color.black;

            using (new GUILayout.VerticalScope(options))
            using (new GUILayout.HorizontalScope())
            {
                string text;
                GUILayout.Label($"<b><color=#ecf0f1>{label}</color></b>");

                if (v)
                {
                    text = "On";
                    GUI.backgroundColor = Color.green;
                }
                else
                {
                    text = "Off";
                    GUI.backgroundColor = Color.black;
                }

                v = GUILayout.Button($"<b><color=#ecf0f1>{text}</color></b>", RGUIStyle.button, GUILayout.Width(52f));
            }

            return v;
        }
    }
}