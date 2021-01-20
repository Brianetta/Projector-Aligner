using Sandbox.ModAPI.Ingame;
using System;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        class ProjectorController
        {
            private const int Key_Down = 1;
            private const int Key_Up = 2;
            private const int Key_Left = 4;
            private const int Key_Right = 8;
            private const int Key_E = 16;
            private const int Key_Q = 32;
            private const int Key_D = 64;
            private const int Key_A = 128;
            private const int Key_Space = 256;
            private const int Key_C = 512;
            private const int Key_S = 1024;
            private const int Key_W = 2048;

            private Program program;

            private IMyShipController controller;
            public ProjectorGroup projectorGroup
            {
                get; set;
            }
            private int previousKeys;

            public ProjectorController(IMyShipController controller, Program program)
            {
                this.controller = controller;
                this.program = program;
            }

            public int GetKeysPressed()
            {
                if (null == controller || !controller.IsUnderControl)
                    return 0;
                const float RotationThreshold = 8.0f;
                const float RollThreshold = 0.5f;
                const float MoveThreshold = 0.5f;
                int keysPressed = 0;
                if (controller.RotationIndicator.X > RotationThreshold)
                    keysPressed |= Key_Down;
                if (controller.RotationIndicator.X < -RotationThreshold)
                    keysPressed |= Key_Up;
                if (controller.RotationIndicator.Y > RotationThreshold)
                    keysPressed |= Key_Left;
                if (controller.RotationIndicator.Y < -RotationThreshold)
                    keysPressed |= Key_Right;
                if (controller.RollIndicator > RollThreshold)
                    keysPressed |= Key_E;
                if (controller.RollIndicator < -RollThreshold)
                    keysPressed |= Key_Q;
                if (controller.MoveIndicator.X > MoveThreshold)
                    keysPressed |= Key_D;
                if (controller.MoveIndicator.X < -MoveThreshold)
                    keysPressed |= Key_A;
                if (controller.MoveIndicator.Y > MoveThreshold)
                    keysPressed |= Key_Space;
                if (controller.MoveIndicator.Y < -MoveThreshold)
                    keysPressed |= Key_C;
                if (controller.MoveIndicator.Z > MoveThreshold)
                    keysPressed |= Key_S;
                if (controller.MoveIndicator.Z < -MoveThreshold)
                    keysPressed |= Key_W;
                int retVal = (previousKeys | keysPressed) ^ previousKeys;
                previousKeys = keysPressed;
                program.Runtime.UpdateFrequency |= UpdateFrequency.Once;
                return retVal;
            }

            public void UpdateKeys()
            {
                int keys = GetKeysPressed();
                var projector = projectorGroup.CurrentProjector;
                if ((keys & Key_Up) == Key_Up)
                {
                    projector.ProjectionRotation = new Vector3I(projector.ProjectionRotation.X >= 3 ? 0 : projector.ProjectionRotation.X + 1, projector.ProjectionRotation.Y, projector.ProjectionRotation.Z);
                    projector.UpdateOffsetAndRotation();
                }
                if ((keys & Key_Down) == Key_Down)
                {
                    projector.ProjectionRotation = new Vector3I(projector.ProjectionRotation.X == 0 ? 3 : projector.ProjectionRotation.X - 1, projector.ProjectionRotation.Y, projector.ProjectionRotation.Z);
                    projector.UpdateOffsetAndRotation();
                }
                if ((keys & Key_Left) == Key_Left)
                {
                    projector.ProjectionRotation = new Vector3I(projector.ProjectionRotation.X, projector.ProjectionRotation.Y >= 3 ? 0 : projector.ProjectionRotation.Y + 1, projector.ProjectionRotation.Z);
                    projector.UpdateOffsetAndRotation();
                }
                if ((keys & Key_Right) == Key_Right)
                {
                    projector.ProjectionRotation = new Vector3I(projector.ProjectionRotation.X, projector.ProjectionRotation.Y == 0 ? 3 : projector.ProjectionRotation.Y - 1, projector.ProjectionRotation.Z);
                    projector.UpdateOffsetAndRotation();
                }
                if ((keys & Key_Q) == Key_Q)
                {
                    projector.ProjectionRotation = new Vector3I(projector.ProjectionRotation.X, projector.ProjectionRotation.Y, projector.ProjectionRotation.Z >= 3 ? 0 : projector.ProjectionRotation.Z + 1);
                    projector.UpdateOffsetAndRotation();
                }
                if ((keys & Key_E) == Key_E)
                {
                    projector.ProjectionRotation = new Vector3I(projector.ProjectionRotation.X, projector.ProjectionRotation.Y, projector.ProjectionRotation.Z == 0 ? 3 : projector.ProjectionRotation.Z - 1);
                    projector.UpdateOffsetAndRotation();
                }
                if ((keys & Key_A) == Key_A)
                {
                    projector.ProjectionOffset = new Vector3I(projector.ProjectionOffset.X + 1, projector.ProjectionOffset.Y, projector.ProjectionOffset.Z);
                    projector.UpdateOffsetAndRotation();
                }
                if ((keys & Key_D) == Key_D)
                {
                    projector.ProjectionOffset = new Vector3I(projector.ProjectionOffset.X - 1, projector.ProjectionOffset.Y, projector.ProjectionOffset.Z);
                    projector.UpdateOffsetAndRotation();
                }
                if ((keys & Key_Space) == Key_Space)
                {
                    projector.ProjectionOffset = new Vector3I(projector.ProjectionOffset.X, projector.ProjectionOffset.Y + 1, projector.ProjectionOffset.Z);
                    projector.UpdateOffsetAndRotation();
                }
                if ((keys & Key_C) == Key_C)
                {
                    projector.ProjectionOffset = new Vector3I(projector.ProjectionOffset.X, projector.ProjectionOffset.Y - 1, projector.ProjectionOffset.Z);
                    projector.UpdateOffsetAndRotation();
                }
                if ((keys & Key_S) == Key_S)
                {
                    projector.ProjectionOffset = new Vector3I(projector.ProjectionOffset.X, projector.ProjectionOffset.Y, projector.ProjectionOffset.Z - 1);
                    projector.UpdateOffsetAndRotation();
                }
                if ((keys & Key_W) == Key_W)
                {
                    projector.ProjectionOffset = new Vector3I(projector.ProjectionOffset.X, projector.ProjectionOffset.Y, projector.ProjectionOffset.Z + 1);
                    projector.UpdateOffsetAndRotation();
                }
            }
        }
    }
}
