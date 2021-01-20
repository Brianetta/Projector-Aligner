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
                if (!projectors.Contains(projector)) {
                    projectors.Add(projector);
                }
            }
            public void Clear()
            {
                projectors.Clear();
            }
        }
    }
}
