using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;
using System;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        string Version = "Version 1.0.4";
        List<IMyTerminalBlock>Blocks = new List<IMyTerminalBlock>();
        List<ProjectorController> ProjectorControllers = new List<ProjectorController>();
        Dictionary<string, ProjectorGroup> ProjectorGroups = new Dictionary<string, ProjectorGroup>();
        MyIni ini = new MyIni();
        string iniSection = "projector";
        MyCommandLine commandLine = new MyCommandLine();

        struct MenuItem
        {
            public string Sprite;
            public float SpriteRotation;
            public Color SpriteColor;
            public Color TextColor;
            public string MenuText;
            public Action Action;
        }

        public void Build()
        {
            IMyProjector projector = null;
            IMyShipController controller = null;
            IMyTextSurfaceProvider provider = null;
            string groupName;
            ProjectorControllers.Clear();
            ProjectorGroups.Clear();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Blocks, block => {
                projector = block as IMyProjector;
                if (null != projector)
                {
                    if (MyIni.HasSection(projector.CustomData, iniSection) && ini.TryParse(projector.CustomData))
                    {
                        groupName = ini.Get(iniSection, "group").ToString("default");
                        if (!ProjectorGroups.Keys.Contains(groupName))
                        {
                            ProjectorGroups.Add(groupName, new ProjectorGroup(groupName));
                        }
                        ProjectorGroups[groupName].Add(projector);
                    }
                }
                provider = block as IMyTextSurfaceProvider;
                if (null != provider)
                {
                    if (MyIni.HasSection(block.CustomData, iniSection) && ini.TryParse(block.CustomData))
                    {
                        groupName = ini.Get(iniSection, "group").ToString("default");
                        if (!ProjectorGroups.Keys.Contains(groupName))
                        {
                            ProjectorGroups.Add(groupName, new ProjectorGroup(groupName));
                        }
                        int surfacenumber = ini.Get(iniSection, "display").ToInt16(-1);
                        float scale = ini.Get(iniSection, "scale").ToSingle(1.0f);
                        if (surfacenumber >= 0 && provider.SurfaceCount > 0)
                        {
                            string DefaultColor = "00CED1";
                            string ColorStr = ini.Get(iniSection, "color").ToString(DefaultColor);
                            if (ColorStr.Length < 6) 
                                ColorStr = DefaultColor;
                            string R = ColorStr.Substring(0, 2);
                            string G = ColorStr.Substring(2, 2);
                            string B = ColorStr.Substring(4, 2);
                            Color color = new Color() { R = byte.Parse(R, System.Globalization.NumberStyles.HexNumber), G = byte.Parse(G, System.Globalization.NumberStyles.HexNumber), B = byte.Parse(B, System.Globalization.NumberStyles.HexNumber), A = 255 };
                            ProjectorGroups[groupName].Add(new ManagedDisplay(provider.GetSurface(surfacenumber),scale, color));
                            
                        }
                    }
                }
                controller = block as IMyShipController;
                if (null != controller)
                {
                    if (MyIni.HasSection(controller.CustomData, iniSection) && ini.TryParse(controller.CustomData))
                    {
                        groupName = ini.Get(iniSection, "group").ToString("default");
                        ProjectorController projectorController = new ProjectorController(controller, this);
                        if (!ProjectorGroups.Keys.Contains(groupName))
                            ProjectorGroups.Add(groupName, new ProjectorGroup(groupName));
                        projectorController.projectorGroup = ProjectorGroups[groupName];
                        ProjectorControllers.Add(projectorController);                        
                    }
                }
                return false; 
            });
        }

        public Program()
        {
            Echo(Version);
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            Build();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            foreach (var projectorController in ProjectorControllers)
                projectorController.UpdateKeys();
            if (commandLine.TryParse(argument))
            {
                string groupName = commandLine.Argument(1) ?? "default";
                switch (commandLine.Argument(0))
                {
                    case "up":                        
                        if (ProjectorGroups.Keys.Contains(groupName))
                            ProjectorGroups[groupName].Up();
                        break;
                    case "down":
                        if (ProjectorGroups.Keys.Contains(groupName))
                            ProjectorGroups[groupName].Down();
                        break;
                    case "apply":
                    case "select":
                        if (ProjectorGroups.Keys.Contains(groupName))
                        {
                            ProjectorGroup group = ProjectorGroups[groupName];
                            if (!group.DisplayStatus)
                                group.Select();
                            group.DisplayStatus = !group.DisplayStatus;
                        }
                        break;
                    case "build":
                    case "rebuild":
                        Build();
                        break;
                    default:
                        break;
                }
            }
            if ((updateSource & UpdateType.Update100) != 0)
            {
                foreach (var projectorGroup in ProjectorGroups.Values)
                    projectorGroup.UpdateDisplays();
            }
        }
    }
}
