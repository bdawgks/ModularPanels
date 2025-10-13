using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using ModularPanels.DrawLib;

namespace ModularPanels.ButtonLib
{
    public struct JSON_Control_Lamp
    {
        public int Size { get; set; }
        public float Border { get; set; }
        public ColorJS ColorOn { get; set; }
        public ColorJS ColorOff { get; set; }
    }

    public struct JSON_Control_RotarySwitchPosition
    {
        public float Angle { get; set; }
        public float Size { get; set; }
        public bool Latching { get; set; } = true;
        public JSON_Control_Lamp? Lamp { get; set; }
        public string? Text { get; set; }
        public string? TextStyle { get; set; }

        public JSON_Control_RotarySwitchPosition() { }
    }

    public struct JSON_Control_RotarySwitch
    {
        public string Name { get; set; }
        public float Size { get; set; }
        public ColorJS PrimaryColor { get; set; }
        public ColorJS SecondaryColor { get; set; }
        public int CenterPos { get; set; }
        public List<JSON_Control_RotarySwitchPosition> Positions { get; set; }
    }

    public struct JSON_Control_Templates
    {
        public List<JSON_Control_RotarySwitch>? RotarySwitches { get; set; }

        public static void LoadTemplateFile(string path)
        {
            if (!File.Exists(path))
                return;

            string json = File.ReadAllText(path);
            JSON_Control_Templates templateData = JsonSerializer.Deserialize<JSON_Control_Templates>(json);

            if (templateData.RotarySwitches != null)
            {
                foreach (var data in templateData.RotarySwitches)
                {
                    TemplateBank<RotarySwitchTemplate>.Instance.AddItem(new(data));
                }
            }
        }
    }
}
