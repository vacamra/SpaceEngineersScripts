using System.Diagnostics;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.Voxels;
using VRage.UserInterface.Media;
using VRageMath;

namespace SpaceEngineersScripts.DamageStatus
{
    public class Program: MyGridProgram
    {
        /*
            
        */
        // Configuration
     


        // Script start

        private void Status(bool success, string status)
        {            
            var surface = Me.GetSurface(0);
            surface.ContentType = ContentType.TEXT_AND_IMAGE;
            surface.BackgroundColor = success ? Color.Blue : Color.Red;
            surface.FontColor = Color.White;
            surface.FontSize = 2;
            surface.WriteText($"Extract ship\n" +
                $"Status: \n" +
                $"{status}", false);
        }

        public void Main(string argument)
        {
            List<IMyTerminalBlock> allDamagedBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType(allDamagedBlocks, b => b.IsSameConstructAs(Me) && b.)
        }
    }
}
