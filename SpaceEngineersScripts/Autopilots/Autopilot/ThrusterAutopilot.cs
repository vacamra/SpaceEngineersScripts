using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace SpaceEngineersScripts.Autopilots.Autopilot
{
    class ThrusterAutopilot
    {
        public Vector3 Velocity { get; set; } = Vector3.Zero;
        public Vector3? Direction { get; set; } = null;

        public List<ThrusterOutput> LastThrusterOutputs { get; } = new List<ThrusterOutput>();
        public Vector3 LastSpeedDifference { get; set; }

        private MyGridProgram Program;
        private IMyGridTerminalSystem GridTerminalSystem => Program.GridTerminalSystem;
        private IMyProgrammableBlock Me => Program.Me;

        public struct ThrusterOutput
        {
            public Base6Directions.Direction Direction { get; set; }
            public float ThrusterOutputRatio { get; set; }
        }

        public ThrusterAutopilot(MyGridProgram program)
        {
            Program = program;
        }

        public void Actuate()
        {
            ActuateThrusters();
        }

        public void DisableAllOverrides()
        {
            List<IMyThrust> thrusters = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType(thrusters, t => t.IsSameConstructAs(Me) && t.IsFunctional && t.Enabled);
            foreach (var thruster in thrusters)
            {
                thruster.ThrustOverridePercentage = 0;
            }
        }

        private void ActuateThrusters()
        {
            List<IMyShipController> controllers = new List<IMyShipController>();
            GridTerminalSystem.GetBlocksOfType(controllers, c => c.IsSameConstructAs(Me));
            var currentGravity = controllers[0].GetNaturalGravity();
            // act as though the gravity has already applied, this velocity is relative to the world matrix                              
            var currentWorldVelocity = (Vector3)(controllers[0].GetShipVelocities().LinearVelocity + currentGravity * Program.Runtime.TimeSinceLastRun.TotalSeconds);
            var relativeVelocity = Vector3.TransformNormal(currentWorldVelocity, MatrixD.Transpose(controllers[0].WorldMatrix));

            var desiredChange = Vector3.Subtract(Velocity, relativeVelocity);

            var shipMass = controllers[0].CalculateShipMass().TotalMass;

            List<IMyThrust> thrusters = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType(thrusters, t => t.IsSameConstructAs(Me) && t.IsFunctional && t.Enabled);

            LastThrusterOutputs.Clear();
            LastSpeedDifference = desiredChange;
            ActuateThrusters(thrusters, shipMass, Math.Abs(desiredChange.X), desiredChange.X >= 0f ? Base6Directions.Direction.Right : Base6Directions.Direction.Left);
            ActuateThrusters(thrusters, shipMass, Math.Abs(desiredChange.Y), desiredChange.Y >= 0f ? Base6Directions.Direction.Up : Base6Directions.Direction.Down);
            ActuateThrusters(thrusters, shipMass, Math.Abs(desiredChange.Z), desiredChange.Z >= 0f ? Base6Directions.Direction.Backward : Base6Directions.Direction.Forward);
        }

        private void ActuateThrusters(List<IMyThrust> thrusters, float shipMass, float desiredChange, Base6Directions.Direction direction)
        {
            Program.Echo(direction.ToString());
            var relevantThrusters = thrusters.Where(t => Base6Directions.GetDirection(-t.GridThrustDirection) == direction).ToList();
            var maxThrust = relevantThrusters.Sum(t => t.MaxEffectiveThrust);

            var maxAccelerationPerTick = (maxThrust / shipMass) * (float)Program.Runtime.TimeSinceLastRun.TotalSeconds;

            var ratio = desiredChange / maxAccelerationPerTick;

            foreach (var thruster in relevantThrusters)
            {
                thruster.ThrustOverridePercentage = MathHelper.Min(ratio, 1);
                LastThrusterOutputs.Add(new ThrusterOutput
                {
                    Direction = direction,
                    ThrusterOutputRatio = thruster.ThrustOverridePercentage
                });
            }
            foreach (var reverseThruster in thrusters.Where(t => Base6Directions.GetDirection(t.GridThrustDirection) == direction))
            {
                reverseThruster.ThrustOverridePercentage = 0f;
            }
        }
    }
}
