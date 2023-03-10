using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace xlperimental_mod
{
    class UIFold
    {
        public bool active;
        public string text;

        public UIFold(string text)
        {
            this.text = text;
        }

        public void Fold(string color = "#fad390")
        {
            if (GUILayout.Button($"<b><size=14><color={color}>" + (!active ? "▶" : "▼") + "</color>" + text + "</size></b>", "Label"))
            {
                active = !active;

                if (active) UISounds.Instance.PlayOneShotSelectionChange();
                else UISounds.Instance.PlayOneShotSelectMinor();

                Main.ui.updateMainWindow();
            }
        }
    }
}
