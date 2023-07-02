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
        string Version = "Version 1.2.0";
        List<IMyTerminalBlock>Blocks = new List<IMyTerminalBlock>();
        List<ProjectorController> ProjectorControllers = new List<ProjectorController>();
        Dictionary<string, ProjectorGroup> ProjectorGroups = new Dictionary<string, ProjectorGroup>();
        MyIni ini = new MyIni();
        static string iniSection = "projector";
        static string DisplaySectionPrefix = iniSection + "_display";
        MyCommandLine commandLine = new MyCommandLine();
        private List<string> SectionNames = new List<string>();
        StringBuilder SectionCandidateName = new StringBuilder();

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
            StringComparison ignoreCase = StringComparison.InvariantCultureIgnoreCase;
            IMyProjector projector = null;
            IMyShipController controller = null;
            IMyTextSurfaceProvider provider = null;
            string groupName;
            ProjectorControllers.Clear();
            ProjectorGroups.Clear();
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(Blocks, block => {
                if (!block.IsSameConstructAs(Me))
                    return false;
                ini.TryParse(block.CustomData);
                ini.GetSections(SectionNames);
                foreach (var section in SectionNames)
                {
                    Echo(String.Format("Block: {0}",block.CustomName));
                    groupName = ini.Get(section, "group").ToString("default");
                    projector = block as IMyProjector;
                    if (null != projector && section.Equals(iniSection))
                    {
                        Echo(String.Format("Projector: {0}",groupName));
                        if (!ProjectorGroups.ContainsKey(groupName))
                        {
                            ProjectorGroups.Add(groupName, new ProjectorGroup(groupName, this));
                        }
                        ProjectorGroups[groupName].Add(projector);
                    }
                    provider = block as IMyTextSurfaceProvider;
                    if (null != provider && (section.Equals(iniSection) || section.StartsWith(DisplaySectionPrefix)))
                    {
                        Echo(String.Format("Provider: {0}", groupName));
                        if (!ProjectorGroups.ContainsKey(groupName))
                        {
                            ProjectorGroups.Add(groupName, new ProjectorGroup(groupName, this));
                        }
                        int surfacenumber = -1;
                        for (int displayNumber = 0; displayNumber < provider.SurfaceCount; ++displayNumber)
                        {
                            SectionCandidateName.Clear();
                            SectionCandidateName.Append(DisplaySectionPrefix).Append(displayNumber.ToString());
                            if (section.Equals(SectionCandidateName.ToString(), ignoreCase))
                            {
                                surfacenumber = displayNumber;
                                Echo(String.Format("Display: {0}", surfacenumber));
                                addDisplay(provider, groupName, surfacenumber, section);
                            }
                        }
                        if (section == iniSection)
                        {
                            surfacenumber = ini.Get(section, "display").ToInt16(-1);
                            Echo(String.Format("Display: {0}", surfacenumber));
                            addDisplay(provider, groupName, surfacenumber, section);
                        }
                    }
                    controller = block as IMyShipController;
                    if (null != controller && section.Equals(iniSection))
                        {
                        Echo(String.Format("Controller: {0}",groupName));
                        ProjectorController projectorController = new ProjectorController(controller, this);
                        if (!ProjectorGroups.ContainsKey(groupName))
                            ProjectorGroups.Add(groupName, new ProjectorGroup(groupName, this));
                        projectorController.projectorGroup = ProjectorGroups[groupName];
                        ProjectorControllers.Add(projectorController);                      
                    }
                }
                return false;
            });
        }

        private void addDisplay(IMyTextSurfaceProvider provider, string groupName, int surfacenumber, string section)
        {                     
            if (surfacenumber >= 0 && provider.SurfaceCount > 0)
            {
                string DefaultColor = "00CED1";
                float scale = ini.Get(section, "scale").ToSingle(1.0f);
                string ColorStr = ini.Get(section, "color").ToString(DefaultColor);
                if (ColorStr.Length < 6)
                    ColorStr = DefaultColor;
                string R = ColorStr.Substring(0, 2);
                string G = ColorStr.Substring(2, 2);
                string B = ColorStr.Substring(4, 2);
                Color color = new Color() { R = byte.Parse(R, System.Globalization.NumberStyles.HexNumber), G = byte.Parse(G, System.Globalization.NumberStyles.HexNumber), B = byte.Parse(B, System.Globalization.NumberStyles.HexNumber), A = 255 };
                ProjectorGroups[groupName].Add(new ManagedDisplay(provider.GetSurface(surfacenumber), scale, color));
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
            foreach (var projectorController in ProjectorControllers)
                projectorController.UpdateKeys();
            if (commandLine.TryParse(argument))
            {
                string groupName = commandLine.Argument(1) ?? "default";
                switch (commandLine.Argument(0))
                {
                    case "up":                        
                        if (ProjectorGroups.ContainsKey(groupName))
                            ProjectorGroups[groupName].Up();
                        break;
                    case "down":
                        if (ProjectorGroups.ContainsKey(groupName))
                            ProjectorGroups[groupName].Down();
                        break;
                    case "apply":
                    case "select":
                        if (ProjectorGroups.ContainsKey(groupName))
                        {
                            ProjectorGroup group = ProjectorGroups[groupName];
                            if (!group.DisplayStatus)
                            {
                                group.Select();
                                group.UpdateDisplays();
                            }
                            group.DisplayStatus = !group.DisplayStatus;
                        }
                        break;
                    case "load":
                        if (ProjectorGroups.ContainsKey(groupName))
                            ProjectorGroups[groupName].LoadAlignment();
                        break;
                    case "save":
                        if (ProjectorGroups.ContainsKey(groupName))
                            ProjectorGroups[groupName].SaveAlignment();
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
