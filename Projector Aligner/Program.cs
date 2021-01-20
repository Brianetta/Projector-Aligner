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
            string groupName;
            string projectorName;
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Blocks, block => {
                projector = block as IMyProjector;                
                if (null != projector)
                {
                    //Echo($"Found projector: {projector.CustomName}");
                    if (MyIni.HasSection(projector.CustomData, iniSection) && ini.TryParse(projector.CustomData))
                    {
                        groupName = ini.Get(iniSection, "group").ToString("default");
                        projectorName = ini.Get(iniSection, "name").ToString("default");
                        if (!ProjectorGroups.Keys.Contains(groupName))
                        {
                            ProjectorGroups.Add(groupName, new ProjectorGroup(groupName));
                            //Echo($"New P group: {groupName}");
                        }
                        ProjectorGroups[groupName].Add(projector);
                        //Echo($"New projector: {projector.CustomName}");
                    }
                }
                controller = block as IMyShipController;
                if (null != controller)
                {
                    //Echo($"Found controller: {controller.CustomName}");
                    if (MyIni.HasSection(controller.CustomData, iniSection) && ini.TryParse(controller.CustomData))
                    {
                        groupName = ini.Get(iniSection, "group").ToString("default");
                        projectorName = ini.Get(iniSection, "projector").ToString("default");
                        if (!ProjectorControllers.Keys.Contains(groupName))
                        {
                            ProjectorControllers.Add(groupName, new ProjectorController(controller, this));
                            //Echo($"New Controller: {groupName}:{controller.CustomName}");
                        }
                    }
                }
                return false; 
            });
            foreach (var name in ProjectorControllers.Keys)
            {
                if (ProjectorGroups.ContainsKey(name))
                {
                    Echo($"Found projector(s) for {name}");
                    ProjectorControllers[name].projectorGroup = ProjectorGroups[name];
                }
                else
                {
                    Echo($"Did not find projector(s) for {name}");
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
        }
    }
}
