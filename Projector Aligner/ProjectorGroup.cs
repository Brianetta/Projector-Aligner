using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        class ProjectorGroup
        {
            private List<IMyProjector> projectors = new List<IMyProjector>();
            private List<ManagedDisplay> displays = new List<ManagedDisplay>();
            private List<MenuItem> ProjectorMenu = new List<MenuItem>();
            private int SelectedLine = 0;
            private int currentProjectorIndex = 0;
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

            public ProjectorGroup(string customName)
            {
                this.CustomName = customName;
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
                if (projectors.Count == 0)
                {
                    this.currentProjector = projector;
                }
                if (!projectors.Contains(projector))
                {
                    projectors.Add(projector);
                }
                ProjectorMenu.Add(new MenuItem() {
                    Sprite = "Construction",
                    TextColor = Color.White,
                    MenuText = projector.CustomName,
                    Action = () => { currentProjector = projector; currentProjectorIndex = projectors.Count - 1; }
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
            public void Select(int projectorIndex)
            {
                if (projectorIndex < 0 || projectorIndex > projectors.Count - 1)
                    return;
                CurrentProjector = projectors[projectorIndex];
                currentProjectorIndex = projectorIndex;
                UpdateDisplays();
            }
            public void Up()
            {
                if (SelectedLine > 0)
                    --SelectedLine;
                UpdateDisplays();
            }
            public void Down()
            {
                if (SelectedLine < projectors.Count - 1)
                    ++SelectedLine;
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
        }
    }
}
