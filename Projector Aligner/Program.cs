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
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        List<IMyTerminalBlock>Blocks = new List<IMyTerminalBlock>();
        Dictionary<string, ProjectorController> ProjectorControllers = new Dictionary<string, ProjectorController>();
        Dictionary<string, ProjectorGroup> ProjectorGroups = new Dictionary<string, ProjectorGroup>();
        MyIni ini = new MyIni();
        string iniSection = "projector";

        public void Build()
        {
            IMyProjector projector = null;
            IMyShipController controller = null;
            IMyTextSurfaceProvider provider = null;
            string groupName;
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
                        if (surfacenumber >= 0 && provider.SurfaceCount > 0)
                        {
                            ProjectorGroups[groupName].Add(provider.GetSurface(surfacenumber));
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
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            Build();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            foreach (var projectorController in ProjectorControllers.Values)
                projectorController.UpdateKeys();
            if ((Runtime.UpdateFrequency & UpdateFrequency.Update100) != 0)
            {
                foreach (var projectorGroup in ProjectorGroups.Values)
                    projectorGroup.UpdateDisplays();
            }
        }
    }
}
