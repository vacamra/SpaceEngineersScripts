using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRage.UserInterface.Media;
using VRageMath;

namespace SpaceEngineersScripts.BatteryStatus
{
    public class Program: MyGridProgram
    {
        /*
            This script will render a simple battery onto a screen, the battery display its % charge and change color 
            Green at >= 50%, Yellow at >= 20%, Red otherwise
            
            Configure this script by modifying the 2 parameters below:
        */

        string targetBlock = "LCD Panel";      // Name of the block where the battery is to be rendered
        int screenIndex = 0;                           // 0-based index of the screen on target block (e.g. cockpit has 4 screens 0->3)
        int textOffset_X = 0;                           // Offset of the charge% text
        int textOffset_Y = 0;                           // Offset of the charge% text

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        private void Status(bool success, string status)
        {            
            var surface = Me.GetSurface(0);
            surface.ContentType = ContentType.TEXT_AND_IMAGE;
            surface.BackgroundColor = success ? Color.Blue : Color.Red;
            surface.FontColor = Color.White;
            surface.FontSize = 2;
            surface.WriteText($"Battery display\n" +
                $"Status: \n" +
                $"{status}", false);
        }

        public void Main(string argument)
        {
           
            var screenBlock = GridTerminalSystem.GetBlockWithName(targetBlock) as IMyTextSurfaceProvider;
            if (screenBlock == null)
            {
                Status(false, "Block not found");
                return;
            }
                        
            var surface = screenBlock.GetSurface(screenIndex);
            if (surface == null)
            {
                Status(false, "Invalid index");
                return;
            }

            surface.ContentType = ContentType.SCRIPT;
            List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType(batteries, battery => battery.IsSameConstructAs(Me));

            float maxEnergy = batteries.Sum(battery => battery.MaxStoredPower);
            float currentEnergy = batteries.Sum(battery => battery.CurrentStoredPower);                        
            
            var frame = surface.DrawFrame();
            var viewPort = new RectangleF((surface.TextureSize - surface.SurfaceSize) / 2f, surface.SurfaceSize);
            viewPort.Position.X += viewPort.Width * 0.1f;
            viewPort.Width *= 0.8f;
            viewPort.Position.Y += viewPort.Height * 0.2f;
            viewPort.Height *= 0.6f;
            DrawBattery(ref frame, viewPort, currentEnergy / maxEnergy);
            
            frame.Dispose();

            Status(true, "OK");
        }

        private void DrawBattery(ref MySpriteDrawFrame frame, RectangleF viewport, float chargeLevel)
        {
            var borderColor = Color.White;
            var textColor = Color.Black;
            var chargeLevelInt = Math.Round(chargeLevel * 100);
            var batteryColor = chargeLevelInt >= 50 ? Color.Green : chargeLevelInt >= 20 ? Color.Yellow : Color.Red;

            // Battery outline
            frame.Add(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Alignment = TextAlignment.LEFT,
                Data = "SquareHollow",
                Position = new Vector2(viewport.X, viewport.Center.Y),
                Size = viewport.Size * new Vector2(0.9f, 1f),
                Color = borderColor,
                RotationOrScale = 0f
            });
            // Battery right end
            frame.Add(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Alignment = TextAlignment.LEFT,
                Data = "SquareSimple",
                Position = new Vector2(viewport.X + viewport.Width * 0.9f, viewport.Center.Y),
                Size = viewport.Size * new Vector2(0.1f, 0.3f),
                Color = borderColor,
                RotationOrScale = 0f
            }); 
            frame.Add(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Alignment = TextAlignment.LEFT,
                Data = "SquareSimple",
                Position = new Vector2(viewport.X + 5, viewport.Center.Y),
                Size = (viewport.Size - new Vector2(10, 10)) * new Vector2(0.9f * chargeLevel, 1f),
                Color = batteryColor,
                RotationOrScale = 0f
            }); // Battery level           
            
            frame.Add(new MySprite()
            {
                Type = SpriteType.TEXT,
                Alignment = TextAlignment.CENTER,
                Data = string.Format("{0}%", chargeLevelInt),
                Position = viewport.Center + new Vector2(textOffset_X, textOffset_Y),
                Color = textColor,
                FontId = "Debug",
                RotationOrScale = 2f
            }); // Charge text
        }
    }
}
