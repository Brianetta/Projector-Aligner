using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program
    {
        class ProjectorGroup
        {
            private List<IMyProjector> projectors = new List<IMyProjector>();
            private List<IMyTextSurface> displays = new List<IMyTextSurface>();
            public string CustomName
            {
                get;
                set;
            }
            private IMyProjector _currentProjector;

            public ProjectorGroup(string customName)
            {
                this.CustomName = customName;
            }

            public IMyProjector CurrentProjector
            {
                get
                {
                    return _currentProjector;
                }
                set
                {
                    if (projectors.Contains(value))
                    {
                        this._currentProjector = value;
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
                        this._currentProjector = null;
                    }
                }
            }
            public void Add(IMyProjector projector)
            {
                if (projectors.Count == 0)
                {
                    this._currentProjector = projector;
                }
                if (!projectors.Contains(projector))
                {
                    projectors.Add(projector);
                }
            }
            public void Add(IMyTextSurface surface)
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
            public void UpdateDisplays()
            {
                foreach (var surface in displays)
                {
                    surface.WriteText($"{this.CustomName}:{_currentProjector.CustomName}\n");
                    surface.WriteText($"Horizontal Offset: {_currentProjector.ProjectionOffset.X}  Pitch: {_currentProjector.ProjectionRotation.Y}\n",true);
                    surface.WriteText($"Vertical Offset: {_currentProjector.ProjectionOffset.Y}  Yaw: {_currentProjector.ProjectionRotation.X}\n",true);
                    surface.WriteText($"Forward Offset: {_currentProjector.ProjectionOffset.Z}  Roll: {_currentProjector.ProjectionRotation.Z}\n",true);
                }
            }
        }
    }
}
