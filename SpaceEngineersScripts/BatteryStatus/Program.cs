using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace SpaceEngineersScripts.BatteryStatus
{
    public class Program: MyGridProgram
    {
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Main(string argument)
        {
            List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType(batteries, battery => battery.IsSameConstructAs(Me));

            float maxEnergy = batteries.Sum(battery => battery.MaxStoredPower);
            float currentEnergy = batteries.Sum(battery => battery.CurrentStoredPower);

            var surface = Me.GetSurface(0);

            var frame = surface.DrawFrame();
            DrawBattery(ref frame, new RectangleF((surface.TextureSize - surface.SurfaceSize) / 2f, surface.SurfaceSize), currentEnergy / maxEnergy);
            
            frame.Dispose();
        }

        private void DrawBattery(ref MySpriteDrawFrame frame, RectangleF viewport, float chargeLevel)
        {
            // Battery outline
            frame.Add(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Alignment = TextAlignment.LEFT,
                Data = "SquareHollow",
                Position = viewport.Position + 0.1f * viewport.Size,
                Size = viewport.Size * 0.8f,
                Color = new Color(255,255,255,255),
                RotationOrScale = 0f
            });
            // Battery right end
            frame.Add(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Alignment = TextAlignment.LEFT,
                Data = "SquareSimple",
                Position = viewport.Position + 0.9f * viewport.Size - new Vector2(0, viewport.Center.Y),
                Size = new Vector2(10f,30f)*scale,
                Color = new Color(255,255,255,255),
                RotationOrScale = 0f
            }); 
            frame.Add(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Alignment = TextAlignment.CENTER,
                Data = "SquareSimple",
                Position = new Vector2(-27f,0f)*scale+centerPos,
                Size = new Vector2(100f,80f)*scale,
                Color = new Color(255,255,0,255),
                RotationOrScale = 0f
            }); // Battery level
            
            
            frame.Add(new MySprite()
            {
                Type = SpriteType.TEXT,
                Alignment = TextAlignment.LEFT,
                Data = "48%",
                Position = new Vector2(-50f,-31f)*scale+centerPos,
                Color = new Color(0,0,0,255),
                FontId = "Debug",
                RotationOrScale = 2f*scale
            }); // Charge text
        }
        // generated
        public void DrawSprites(MySpriteDrawFrame frame, Vector2 centerPos, float scale = 1f)
        {
            frame.Add(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Alignment = TextAlignment.CENTER,
                Data = "SquareSimple",
                Position = new Vector2(-27f,0f)*scale+centerPos,
                Size = new Vector2(100f,80f)*scale,
                Color = new Color(255,255,0,255),
                RotationOrScale = 0f
            }); // Battery level
            frame.Add(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Alignment = TextAlignment.CENTER,
                Data = "SquareSimple",
                Position = new Vector2(95f,0f)*scale+centerPos,
                Size = new Vector2(10f,30f)*scale,
                Color = new Color(255,255,255,255),
                RotationOrScale = 0f
            }); // Battery right end
            frame.Add(new MySprite()
            {
                Type = SpriteType.TEXTURE,
                Alignment = TextAlignment.CENTER,
                Data = "SquareHollow",
                Position = new Vector2(0f,0f)*scale+centerPos,
                Size = new Vector2(180f,100f)*scale,
                Color = new Color(255,255,255,255),
                RotationOrScale = 0f
            }); // Battery outline
            frame.Add(new MySprite()
            {
                Type = SpriteType.TEXT,
                Alignment = TextAlignment.LEFT,
                Data = "48%",
                Position = new Vector2(-50f,-31f)*scale+centerPos,
                Color = new Color(0,0,0,255),
                FontId = "Debug",
                RotationOrScale = 2f*scale
            }); // Charge text
        }
    }
}
