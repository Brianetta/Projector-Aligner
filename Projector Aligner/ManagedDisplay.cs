using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        class ManagedDisplay
        {
            private IMyTextSurface surface;
            private RectangleF viewport;
            private MySpriteDrawFrame frame;
            private float StartHeight = 5f;
            private float HeadingHeight = 35f;
            private float LineHeight = 40f;
            private float BodyBeginsHeight = 65f; // StartHeight + HeadingHeight + 25;
            private float HeadingFontSize = 2.0f;
            private float RegularFontSize = 1.5f;
            private Vector2 Position;
            private Vector2 CursorDrawPosition;
            private int WindowSize;         // Number of lines shown on screen at once after heading
            private int WindowPosition = 0; // Number of lines scrolled away
            private int CursorMenuPosition; // Position of cursor within window
            private float Scale;

            public ManagedDisplay(IMyTextSurface surface, float scale = 1.0f)
            {
                this.surface = surface;
                this.Scale = scale;

                // Scale everything!
                StartHeight *= scale;
                HeadingHeight *= scale;
                LineHeight *= scale;
                BodyBeginsHeight *= scale;
                HeadingFontSize *= scale;
                RegularFontSize *= scale;

                surface.ContentType = ContentType.SCRIPT;
                surface.Script = "";
                surface.ScriptBackgroundColor = Color.Black;
                viewport = new RectangleF((surface.TextureSize - surface.SurfaceSize) / 2f, surface.SurfaceSize);
                WindowSize = ((int)((viewport.Height - BodyBeginsHeight - 10 * scale) / LineHeight));
            }

            private void DrawCursor()
            {
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "SquareSimple",
                    Position = CursorDrawPosition,
                    Color = Color.DarkTurquoise,
                    Size = new Vector2(viewport.Width, LineHeight)
                });
            }

            private void AddHeading(int menuLength)
            {
                Position = new Vector2(viewport.Width / 2f - LineHeight, StartHeight) + viewport.Position;
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "Projector Status",
                    Position = Position,
                    RotationOrScale = HeadingFontSize,
                    Color = Color.White,
                    Alignment = TextAlignment.CENTER /* Center the text on the position */,
                    FontId = "White"
                });
                Position = new Vector2(viewport.Width - 2 * LineHeight, LineHeight) + viewport.Position;
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "AH_BoreSight",
                    Color = (WindowPosition > 0) ? Color.DarkTurquoise : Color.Black.Alpha(0),
                    RotationOrScale = 1.5f * (float)Math.PI,
                    Size = new Vector2(LineHeight, LineHeight),
                    Position = Position,
                });
                Position += new Vector2(LineHeight, 0);
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = "AH_BoreSight",
                    Color = (WindowPosition + WindowSize < menuLength) ? Color.DarkTurquoise : Color.Black.Alpha(0),
                    RotationOrScale = 0.5f * (float)Math.PI,
                    Size = new Vector2(LineHeight, LineHeight),
                    Position = Position,
                });
                Position = new Vector2(viewport.Width / 2f - LineHeight, StartHeight + HeadingHeight) + viewport.Position;
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "----------------------------",
                    Position = Position,
                    RotationOrScale = RegularFontSize,
                    Color = Color.DarkTurquoise,
                    Alignment = TextAlignment.CENTER,
                    FontId = "White"
                });
            }

            private void AddMenuItem(MenuItem menuItem)
            {
                AddMenuItem(
                    menuText: menuItem.MenuText,
                    sprite: menuItem.Sprite,
                    spriteRotation: menuItem.SpriteRotation,
                    spriteColor: menuItem.SpriteColor,
                    textColor: menuItem.TextColor
                    );
            }

            private void AddMenuItem(string menuText, string sprite = "SquareSimple", float spriteRotation = 0, Color? spriteColor = null, Color? textColor = null)
            {
                float SpriteOffset = 25f * Scale;
                Position += new Vector2(0, LineHeight);
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXTURE,
                    Data = sprite,
                    Position = Position + new Vector2(0, SpriteOffset),
                    RotationOrScale = spriteRotation,
                    Size = new Vector2(LineHeight, LineHeight),
                    Color = spriteColor ?? Color.White,
                });
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = menuText,
                    Position = Position + new Vector2(LineHeight * 1.2f, 0),
                    RotationOrScale = RegularFontSize,
                    Color = textColor ?? Color.Gray,
                    Alignment = TextAlignment.LEFT,
                    FontId = "White"
                });
            }

            internal void RenderMenu(int selectedline, List<MenuItem> menuItems)
            {
                SetWindowPosition(selectedline);
                frame = surface.DrawFrame();
                CursorDrawPosition = new Vector2(0, BodyBeginsHeight + LineHeight + LineHeight * CursorMenuPosition) + viewport.Position;
                DrawCursor();
                AddHeading(menuItems.Count);
                Position.X = surface.TextPadding;
                int renderLineCount = 0;
                foreach (var menuItem in menuItems)
                {
                    if (renderLineCount >= WindowPosition && renderLineCount < WindowPosition + WindowSize)
                        AddMenuItem(menuItem);
                    ++renderLineCount;
                }
                frame.Dispose();
            }

            internal void RenderProjectorStatus(IMyProjector projector, string group)
            {
                frame = surface.DrawFrame();
                CursorDrawPosition = new Vector2(0, BodyBeginsHeight + LineHeight + LineHeight * CursorMenuPosition) + viewport.Position;
                AddHeading(7);
                // Projector group & name
                Position.X = viewport.Width / 2f - LineHeight;
                Position += new Vector2(0, LineHeight);
                float ColumnWidth = viewport.Width / 4;

                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "Group: " + group + "  Projector: " + projector.CustomName,
                    Position = Position,
                    RotationOrScale = RegularFontSize / 2,
                    Color = Color.Gray,
                    Alignment = TextAlignment.CENTER,
                    FontId = "White"
                });
                // Headings
                Position.X = surface.TextPadding;
                Position += new Vector2(0, LineHeight);
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "Offset",
                    Position = Position,
                    RotationOrScale = RegularFontSize,
                    Color = Color.White,
                    Alignment = TextAlignment.LEFT,
                    FontId = "White"
                });
                Position.X += ColumnWidth * 2;
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "Rotation",
                    Position = Position,
                    RotationOrScale = RegularFontSize,
                    Color = Color.White,
                    Alignment = TextAlignment.LEFT,
                    FontId = "White"
                });
                // Data line 1 of 3
                Position.X = surface.TextPadding;
                Position += new Vector2(0, LineHeight);
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "Horiz:",
                    Position = Position,
                    RotationOrScale = RegularFontSize,
                    Color = Color.Gray,
                    Alignment = TextAlignment.LEFT,
                    FontId = "White"
                });
                Position.X += ColumnWidth;
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = projector.ProjectionOffset.X.ToString(),
                    Position = Position,
                    RotationOrScale = RegularFontSize,
                    Color = Color.DarkTurquoise,
                    Alignment = TextAlignment.LEFT,
                    FontId = "White"
                });
                Position.X += ColumnWidth;
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "Pitch:",
                    Position = Position,
                    RotationOrScale = RegularFontSize,
                    Color = Color.Gray,
                    Alignment = TextAlignment.LEFT,
                    FontId = "White"
                });
                Position.X += ColumnWidth;
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = projector.ProjectionRotation.Y.ToString(),
                    Position = Position,
                    RotationOrScale = RegularFontSize,
                    Color = Color.DarkTurquoise,
                    Alignment = TextAlignment.LEFT,
                    FontId = "White"
                });               
                // Data line 2 of 3
                Position.X = surface.TextPadding;
                Position += new Vector2(0, LineHeight);
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "Vert:",
                    Position = Position,
                    RotationOrScale = RegularFontSize,
                    Color = Color.Gray,
                    Alignment = TextAlignment.LEFT,
                    FontId = "White"
                });
                Position.X += ColumnWidth;
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = projector.ProjectionOffset.Y.ToString(),
                    Position = Position,
                    RotationOrScale = RegularFontSize,
                    Color = Color.DarkTurquoise,
                    Alignment = TextAlignment.LEFT,
                    FontId = "White"
                });
                Position.X += ColumnWidth;
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "Yaw:",
                    Position = Position,
                    RotationOrScale = RegularFontSize,
                    Color = Color.Gray,
                    Alignment = TextAlignment.LEFT,
                    FontId = "White"
                });
                Position.X += ColumnWidth;
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = projector.ProjectionRotation.X.ToString(),
                    Position = Position,
                    RotationOrScale = RegularFontSize,
                    Color = Color.DarkTurquoise,
                    Alignment = TextAlignment.LEFT,
                    FontId = "White"
                });
                // Data line 3 of 3
                Position.X = surface.TextPadding;
                Position += new Vector2(0, LineHeight);
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "Forward:",
                    Position = Position,
                    RotationOrScale = RegularFontSize,
                    Color = Color.Gray,
                    Alignment = TextAlignment.LEFT,
                    FontId = "White"
                });
                Position.X += ColumnWidth;
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = projector.ProjectionOffset.Z.ToString(),
                    Position = Position,
                    RotationOrScale = RegularFontSize,
                    Color = Color.DarkTurquoise,
                    Alignment = TextAlignment.LEFT,
                    FontId = "White"
                });
                Position.X += ColumnWidth;
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = "Roll:",
                    Position = Position,
                    RotationOrScale = RegularFontSize,
                    Color = Color.Gray,
                    Alignment = TextAlignment.LEFT,
                    FontId = "White"
                });
                Position.X += ColumnWidth;
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = projector.ProjectionRotation.Z.ToString(),
                    Position = Position,
                    RotationOrScale = RegularFontSize,
                    Color = Color.DarkTurquoise,
                    Alignment = TextAlignment.LEFT,
                    FontId = "White"
                });
                // Blocks remaining
                Position.X = surface.TextPadding;
                Position += new Vector2(0, LineHeight * 1.5f);
                frame.Add(new MySprite()
                {
                    Type = SpriteType.TEXT,
                    Data = $"Blocks remaining: {projector.RemainingBlocks} (total {projector.TotalBlocks})",
                    Position = Position,
                    RotationOrScale = RegularFontSize,
                    Color = Color.Gray,
                    Alignment = TextAlignment.LEFT,
                    FontId = "White"
                });
                frame.Dispose();
            }

            private void SetWindowPosition(int selectedline)
            {
                CursorMenuPosition = selectedline - WindowPosition;
                if (CursorMenuPosition < 0)
                {
                    CursorMenuPosition = 0;
                    WindowPosition = selectedline;
                }
                if (CursorMenuPosition >= WindowSize)
                {
                    CursorMenuPosition = WindowSize - 1;
                    WindowPosition = selectedline - (WindowSize - 1);
                }
            }
        }
    }
}
