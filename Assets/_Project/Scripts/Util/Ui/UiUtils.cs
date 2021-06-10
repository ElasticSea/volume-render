using System;
using System.Collections.Generic;
using ElasticSea.Framework.Extensions;
using UnityEngine;
using UnityEngine.UIElements;

namespace Util.Ui
{
    public static class UiUtils
    {
        public static void EnumField<T>(IEnumerable<T> getEnumValues,T defaultValue, VisualElement container, Action<T> callback)
        {
            var buttons = new Dictionary<T, Button>();

            void Select(Button btn)
            {
                foreach (var (key, value) in buttons)
                {
                    var selectedColor = new Color(.5f, .5f, .5f, 1f);
                    var defaultColor = new Color(.894f, .894f, .894f, 1f);
                    var b1 = Equals(buttons[key], btn);
                    var color = b1 ? selectedColor : defaultColor;
                    value.style.backgroundColor = new StyleColor(color);
                }
            }

            foreach (var channelDepth in getEnumValues)
            {
                var button = new Button();
                button.text = channelDepth.ToString();
                button.clicked += () =>
                {
                    Select(button);

                    callback(channelDepth);
                };
                container.Add(button);
                buttons[channelDepth] = button;
            }

            Select(buttons[defaultValue]);
        }
        
        public static void EnumField<T>(IEnumerable<T> getEnumValues, VisualElement container, Action<T> callback)
        {
            var buttons = new Dictionary<T, Button>();

            foreach (var channelDepth in getEnumValues)
            {
                var button = new Button();
                button.text = channelDepth.ToString();
                button.clicked += () =>
                {
                    callback(channelDepth);
                };
                container.Add(button);
                buttons[channelDepth] = button;
            }
        }
    }
}