using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;
using static VRage.Game.MyObjectBuilder_SessionComponentMission;

namespace SpaceEngineersScripts.FlightData
{
    internal class Program: MyGridProgram
    {
        /*
           This script will render information regarding current vessel velocities and breaking distances

           Configure this script by modifying the parameters below:
         */
        // Configuration
        string targetBlock = "Cockpit";      // Name of the block where the battery is to be rendered
        int screenIndex = 2;                           // 0-based index of the screen on target block (e.g. cockpit has 4 screens 0->3)

        // Script start

        private void Status(bool success, string status)
        {
            var surface = Me.GetSurface(0);
            surface.ContentType = ContentType.TEXT_AND_IMAGE;
            surface.BackgroundColor = success ? Color.Blue : Color.Red;
            surface.FontColor = Color.White;
            surface.FontSize = 2;
            surface.WriteText($"Flight Data\n" +
                $"Status: \n" +
                $"{status}", false);
        }

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        private VelocityStats GetVelocityStats(Vector3D velocities, Vector3D direction, float mass, Vector3D gravity, Base6Directions.Direction positive, Base6Directions.Direction negative)
        {
            var directionComponent = Vector3D.ProjectOnVector(ref velocities, ref direction);
            var velocity = (float)directionComponent.Length();
            var velocityAngle = Vector3D.Angle(directionComponent, direction);
            

            var dir = velocity >= 0 ? negative : positive;
            List<IMyThrust> thrusters = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType(thrusters, t => t.IsSameConstructAs(Me) && t.Orientation.Forward == dir);

            var maxThrust = thrusters.Sum(t => t.MaxEffectiveThrust);
            var acceleration = maxThrust / mass;

            var gravityComponent = Vector3D.ProjectOnVector(ref gravity, ref direction);
            var gravityAcceleration = (float)gravityComponent.Length();
            var gravityAngle = Vector3D.Angle(directionComponent, gravity);
            bool isGravityHelping = gravityAngle >= Math.PI / 2;

            var maxAcceleration = acceleration + (isGravityHelping ? gravityAcceleration : -gravityAcceleration);

            var timeToStop = velocity / maxAcceleration;
            var DistanceToStop = velocity * timeToStop / 2;

            if (velocityAngle > Math.PI / 2) velocity *= -1;
            return new VelocityStats
            {
                Velocity = velocity,
                TimeToStop = timeToStop,
                DistanceToStop = DistanceToStop
            };
        }

        private struct VelocityStats
        {
            public float Velocity;
            public float TimeToStop;
            public float DistanceToStop;
        }

        private string formatRow(VelocityStats stats, string direction)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(direction);
            sb.Append("  ");
            if (stats.Velocity >= -0.9f)
            {
                sb.Append(' ');
            }             
            sb.Append((int)stats.Velocity);
            sb.Append(new string(' ', 10 - sb.Length));
            sb.Append(stats.TimeToStop.ToString("0.#"));
            sb.Append('s');
            sb.Append(new string(' ', 16 - sb.Length));
            sb.Append((int)stats.DistanceToStop);
            sb.Append('m');
            sb.Append('\n');
            return sb.ToString();
        }

        public void Main(string arg)
        {
            List<IMyCockpit> cockpits = new List<IMyCockpit>();
            GridTerminalSystem.GetBlocksOfType(cockpits, c => c.IsSameConstructAs(Me));

            if (cockpits.Count == 0)
            {
                Status(false, "No cockpit found");
                return;
            }

            var cockpit = cockpits[0];
            var velocities = cockpit.GetShipVelocities().LinearVelocity;
            var mass = cockpit.CalculateShipMass().TotalMass;
            var gravity = cockpit.GetNaturalGravity();

            var forward = GetVelocityStats(velocities, cockpit.WorldMatrix.Forward, mass, gravity, Base6Directions.Direction.Forward, Base6Directions.Direction.Backward);
            var right = GetVelocityStats(velocities, cockpit.WorldMatrix.Right, mass, gravity, Base6Directions.Direction.Right, Base6Directions.Direction.Left);
            var up = GetVelocityStats(velocities, cockpit.WorldMatrix.Up, mass, gravity, Base6Directions.Direction.Up, Base6Directions.Direction.Down);

            var monitorBlock = GridTerminalSystem.GetBlockWithName(targetBlock) as IMyTextSurfaceProvider;
            if (monitorBlock == null)
            {
                Status(false, "No monitor found");
                return;
            }

            var screen = monitorBlock.GetSurface(screenIndex);
            screen.WriteText( "DIR  VEL  TTS   DTS\n", false);
            screen.WriteText(formatRow(forward, "FW"), true);
            screen.WriteText(formatRow(right, "RT"), true);
            screen.WriteText(formatRow(up, "UP"), true);

            Status(true, "OK");
        }
    }
}
