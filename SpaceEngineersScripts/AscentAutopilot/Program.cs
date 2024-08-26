using Sandbox.ModAPI.Ingame;
using VRage;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRageMath;

namespace SpaceEngineersScripts.AscentAutopilot
{
    public class Program: MyGridProgram
    {
        /*
            This script supports automatic ascent & descent. For ascent it will try to maintain required speed using the up-facing thrusters. 
            For descent it will calculate when it should enable dampeners and it will enable it at that point.
        */
        // Configuration
        private float targetVelocity = 95;
        private float targetGravity = 0.05f;
        private float breakElevation = 100f;
        private string ReferenceCockpitName = "Cockpit";
        
        
        // scripts
        private Mode currentMode = Mode.Off;
        private enum Mode { Off, Ascent, Landing }

        private void Status(bool success, string status)
        {
            var surface = Me.GetSurface(0);
            surface.ContentType = ContentType.TEXT_AND_IMAGE;
            surface.BackgroundColor = success ? Color.Blue : Color.Red;
            surface.FontColor = Color.White;
            surface.FontSize = 2;
            surface.WriteText($"Ascent autopilot\n" +
                $"Status: \n" +
                $"{status}", false);
        }

        public void Main(string argument)
        {
            Status(true, "OK");

            var cockpit = GridTerminalSystem.GetBlockWithName(ReferenceCockpitName) as IMyCockpit;
            if (cockpit == null)
            {
                Status(false, "Cockpit not found");
                return;
            }
            
            switch (argument)
            {
                case "ascent":
                    PrepareAscent();
                    break;
                case "landing":
                    PrepareLanding();
                    break;
                case "off":
                    TurnOff();
                    break;
                case "":
                    Run();
                    break;
                default:
                    Status(false, $"Unknown arg\n{argument}");
                    TurnOff();
                    break;
            }            
        }

        private List<IMyThrust> GetThrusters()
        {
            List<IMyThrust> thrusters = new List<IMyThrust>();
            var cockpit = GridTerminalSystem.GetBlockWithName(ReferenceCockpitName) as IMyCockpit;
            GridTerminalSystem.GetBlocksOfType(thrusters, t => t.CubeGrid == Me.CubeGrid && Base6Directions.GetFlippedDirection(t.Orientation.Forward) == cockpit.Orientation.Up);

            return thrusters;
        }

        private void PrepareAscent()
        {
            currentMode = Mode.Ascent;
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            var thrusters = GetThrusters();
            foreach (var thruster in thrusters)
            {
                thruster.ThrustOverride = 0;
            }
            var cockpit = GridTerminalSystem.GetBlockWithName(ReferenceCockpitName) as IMyCockpit;
            cockpit.DampenersOverride = false;
        }

        private void PrepareLanding()
        {
            currentMode = Mode.Landing;
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            var thrusters = GetThrusters();
            foreach (var thruster in thrusters)
            {
                thruster.ThrustOverride = 0;
            }
            var cockpit = GridTerminalSystem.GetBlockWithName(ReferenceCockpitName) as IMyCockpit;
            cockpit.DampenersOverride = false;
        }

        private void TurnOff()
        {
            currentMode = Mode.Off;
            Runtime.UpdateFrequency = UpdateFrequency.None;
            var thrusters = GetThrusters();
            foreach (var thruster in thrusters)
            {
                thruster.ThrustOverride = 0;
            }
        }

        private void Run()
        {
            var thrusters = GetThrusters();
            var cockpit = GridTerminalSystem.GetBlockWithName(ReferenceCockpitName) as IMyCockpit;
            var gravity = cockpit.GetNaturalGravity().Length();
            var velocity = cockpit.GetShipVelocities().LinearVelocity.Length();
            
            if (currentMode == Mode.Ascent) RunAscent(thrusters, cockpit, gravity, velocity);
            else if (currentMode == Mode.Landing) RunLanding(thrusters, cockpit, gravity, velocity);
        }

        private void RunAscent(List<IMyThrust> thrusters, IMyCockpit cockpit, double gravity, double velocity)
        {
            if (gravity <= targetGravity)
            {
                TurnOff();
                return;
            }
            
            if (velocity > targetVelocity)
            {
                foreach (var thruster in thrusters)
                {
                    thruster.ThrustOverride = 0;
                }
            }
            else
            {
                foreach (var thruster in thrusters)
                {
                    thruster.ThrustOverridePercentage = 100;
                }
            }
        }

        private void RunLanding(List<IMyThrust> thrusters, IMyCockpit cockpit, double gravity, double velocity)
        {
            if (gravity == 0)
            {
                foreach (var thruster in thrusters)
                {
                    thruster.ThrustOverride = 0;
                }
                return;
            }
            var mass = cockpit.CalculateShipMass().TotalMass;
            var maxThrust = thrusters.Sum(t => t.MaxEffectiveThrust);
            var maxAcceleration = maxThrust / mass - gravity;
            var timeToStop = velocity / maxAcceleration;

            var distanceToStop = velocity * timeToStop / 2;
            var startBreakingPoint = distanceToStop + breakElevation;
            double currentElevation;
            cockpit.TryGetPlanetElevation(MyPlanetElevation.Surface, out currentElevation);
            if (currentElevation <= startBreakingPoint)
            {
                TurnOff();
                cockpit.DampenersOverride = true;
            } 
        }
    }
}
