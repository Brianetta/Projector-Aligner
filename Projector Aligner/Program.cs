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
        string Version = "Version 1.0.0";
        List<IMyTerminalBlock>Blocks = new List<IMyTerminalBlock>();
        Dictionary<string, ProjectorController> ProjectorControllers = new Dictionary<string, ProjectorController>();
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
                            ProjectorGroups[groupName].Add(new ManagedDisplay(provider.GetSurface(surfacenumber),scale));
                        }
                    }
                }
                controller = block as IMyShipController;
                if (null != controller)
                {
                    if (MyIni.HasSection(controller.CustomData, iniSection) && ini.TryParse(controller.CustomData))
                    {
                        groupName = ini.Get(iniSection, "group").ToString("default");
                        if (!ProjectorControllers.Keys.Contains(groupName))
                        {
                            ProjectorControllers.Add(groupName, new ProjectorController(controller, this));
                        }
                    }
                }
                return false; 
            });
            foreach (var name in ProjectorControllers.Keys)
            {
                if (ProjectorGroups.ContainsKey(name))
                {
                    ProjectorControllers[name].projectorGroup = ProjectorGroups[name];
                }
                else
                {
                    ProjectorGroups.Remove(name);
                }
            }
        }

        public Program()
        {
            Echo(Version);
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            Build();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            foreach (var projectorController in ProjectorControllers.Values)
                projectorController.UpdateKeys();
            if (commandLine.TryParse(argument))
            {
                string groupName = (commandLine.Argument(1) == null) ? "default" : commandLine.Argument(1);
                switch (commandLine.Argument(0))
                {
                    case "up":                        
                        if (ProjectorGroups.Keys.Contains(groupName) && !ProjectorGroups[groupName].DisplayStatus)
                            ProjectorGroups[groupName].Up();
                        break;
                    case "down":
                        if (ProjectorGroups.Keys.Contains(groupName) && !ProjectorGroups[groupName].DisplayStatus)
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
