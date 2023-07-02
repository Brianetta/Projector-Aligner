using Sandbox.Game.Screens.Helpers.RadialMenuActions;
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        class ProjectorGroup
        {
            private Program program;
            private List<IMyProjector> projectors = new List<IMyProjector>();
            private List<ManagedDisplay> displays = new List<ManagedDisplay>();
            private List<MenuItem> ProjectorMenu = new List<MenuItem>();
            private int SelectedLine = 0;
            private int SelectedProjector = 0;
            private int UILines = 3; // Number of additional menu lines
            public bool DisplayStatus
            {
                get
                {
                    return displayStatus;
                }
                set
                {
                    displayStatus = value;
                    UpdateDisplays();
                }
            }
            public string CustomName
            {
                get;
                set;
            }
            private IMyProjector currentProjector;
            private bool displayStatus;

            public ProjectorGroup(string customName,Program program)
            {
                this.CustomName = customName;
                this.program = program;
                DisplayStatus = true;
            }

            public IMyProjector CurrentProjector
            {
                get
                {
                    return currentProjector;
                }
                set
                {
                    if (projectors.Contains(value))
                    {
                        this.currentProjector = value;
                        foreach (var projector in projectors)
                        {
                            if (projector.Equals(value))
                            {
                                projector.Enabled = true;
                            }
                            else
                            {
                                projector.Enabled = false;
                            }
                        }
                    }
                    else
                    {
                        this.currentProjector = null;
                    }
                }
            }

            public void Add(IMyProjector projector)
            {
                if (projectors.Count == 0 || projector.Enabled)
                {
                    this.currentProjector = projector;
                    this.SelectedLine = projectors.Count;
                }
                if (!projectors.Contains(projector))
                {
                    projectors.Add(projector);
                }
                ProjectorMenu.Add(new MenuItem() {
                    Sprite = "Construction",
                    SpriteColor = Color.Yellow,
                    TextColor = Color.White,
                    MenuText = projector.CustomName,
                    Action = () => { currentProjector = projector; }
                });
            }
            public void Add(ManagedDisplay surface)
            {
                if (!displays.Contains(surface))
                {
                    displays.Add(surface);
                }
            }
            public void Clear()
            {
                projectors.Clear();
                displays.Clear();
            }
            public void Select()
            {
                this.Select(SelectedLine);
            }
            public void Select(int menuIndex)
            {                
                if (menuIndex < 0 || menuIndex > projectors.Count + UILines - 1)
                    return;
                if (menuIndex > projectors.Count - 1)
                {
                    switch (menuIndex - projectors.Count)
                    {
                        case 0:
                            SaveAlignment();
                            SelectedLine = SelectedProjector;
                            break;
                        case 1:
                            LoadAlignment();
                            SelectedLine = SelectedProjector;
                            break;
                        case 2:
                        default:
                            TogglePower();
                            SelectedLine = SelectedProjector;
                            break;
                    }
                }
                else
                {
                    SelectedProjector = SelectedLine;
                    CurrentProjector = projectors[SelectedProjector];
                    SelectedLine = menuIndex;
                }
            }
            public void Up()
            {
                if (SelectedLine > 0)
                    --SelectedLine;
                if (DisplayStatus)
                    Select();
                UpdateDisplays();
            }
            public void Down()
            {
                ++SelectedLine;
                if (DisplayStatus)
                {
                    if (SelectedLine >= projectors.Count)
                        --SelectedLine;
                    Select();
                }
                else
                {
                    if (SelectedLine >= projectors.Count + UILines)
                        --SelectedLine;
                }
                UpdateDisplays();
            }
            public void UpdateDisplays()
            {
                foreach (var display in displays)
                {
                    if (DisplayStatus)
                        display.RenderProjectorStatus(currentProjector, CustomName);
                    else
                        display.RenderMenu(SelectedLine, ProjectorMenu);
                }
            }

            internal void LoadAlignment()
            {
                var ini = program.ini;
                var iniSection = Program.iniSection;
                ini.TryParse(CurrentProjector.CustomData);
                Vector3I ProjectionRotation = new Vector3I(
                    ini.Get(iniSection, "RotationX").ToInt32(),
                    ini.Get(iniSection, "RotationY").ToInt32(),
                    ini.Get(iniSection, "RotationZ").ToInt32()
                );
                Vector3I ProjectionOffset = new Vector3I(
                    ini.Get(iniSection, "OffsetX").ToInt32(),
                    ini.Get(iniSection, "OffsetY").ToInt32(),
                    ini.Get(iniSection, "OffsetZ").ToInt32()
                );
                CurrentProjector.ProjectionRotation = ProjectionRotation;
                CurrentProjector.ProjectionOffset = ProjectionOffset;
            }

            internal void SaveAlignment()
            {
                MyIni ini = program.ini;
                ini.TryParse(CurrentProjector.CustomData);
                ini.Set(Program.iniSection, "RotationX", CurrentProjector.ProjectionRotation.X);
                ini.Set(Program.iniSection, "RotationY", CurrentProjector.ProjectionRotation.Y);
                ini.Set(Program.iniSection, "RotationZ", CurrentProjector.ProjectionRotation.Z);
                ini.Set(Program.iniSection, "OffsetX", CurrentProjector.ProjectionOffset.X);
                ini.Set(Program.iniSection, "OffsetY", CurrentProjector.ProjectionOffset.Y);
                ini.Set(Program.iniSection, "OffsetZ", CurrentProjector.ProjectionOffset.Z);
                CurrentProjector.CustomData = ini.ToString();
            }
            internal void TogglePower()
            {
                CurrentProjector.Enabled = !CurrentProjector.Enabled;
            }
        }
    }
}
