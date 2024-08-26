using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace SpaceEngineersScripts.Autopilots.FlightAssistLite
{
    public class Program : MyGridProgram
    {
        // Configuration

        // Script start
        IMyCockpit cockpit;
        List<IMyMotorStator> rotors = new List<IMyMotorStator>();
        List<IMyThrust> rotorThrusters = new List<IMyThrust>();
        Mode currentMode = Mode.Dampeners;

        private enum Mode { Dampeners, Coast }

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            List<IMyCockpit> cockpits = new List<IMyCockpit>();
            GridTerminalSystem.GetBlocksOfType(cockpits, cockpit => cockpit.IsMainCockpit);
            if (cockpits.Count == 0)
            {
                return;
            }
            cockpit = cockpits[0];

            GridTerminalSystem.GetBlocksOfType(rotors);
            GridTerminalSystem.GetBlocksOfType(rotorThrusters, thruster => thruster.CubeGrid != cockpit.CubeGrid && thruster.IsSameConstructAs(cockpit));
        }

        private struct AtmosphericOrientation
        {
            public float Pitch;
            public float Roll;
        }

        private AtmosphericOrientation GetCurrentOrientation()
        {            
            var gravity = cockpit.GetNaturalGravity();
            return new AtmosphericOrientation
            {
                Roll = MyMath.AngleBetween(cockpit.WorldMatrix.Right, gravity) - (float)Math.PI / 2f,
                Pitch = MyMath.AngleBetween(cockpit.WorldMatrix.Forward, gravity) - (float)Math.PI / 2f
            };
        }

        public void Main(string argument)
        {
            if (cockpit == null)
            {
                Echo("No main cockpit found.");
                return;
            }

            double elevation;
            if (!cockpit.TryGetPlanetElevation(MyPlanetElevation.Surface, out elevation))
            {
                Echo("Not in gravity");
                return;
            }

            // Get player input (both velocity + orientation)
            var velocityInput = cockpit.MoveIndicator;
            // X = Left/Right (Right = 1, Left = -1)
            // Y = Up/Down (Up = 1, Down = -1)
            // Z = Forward/Backward (Forward = -1, Backward = 1)

            Foo(velocityInput);
        }


        private float SetRotorAngle(float targetAngle)
        {
            float lastCurrentAngle = targetAngle;
            foreach (var rotor in rotors)
            {
                var topGrid = rotor.TopGrid;
                List<IMyThrust> localThrusters = new List<IMyThrust>();
                GridTerminalSystem.GetBlocksOfType(localThrusters, thrust => thrust.CubeGrid == topGrid);

                var downwardAngle = (float)Vector3.Angle(cockpit.WorldMatrix.Down, localThrusters[0].WorldMatrix.Forward);
                var forwardAngle = (float)Vector3.Angle(cockpit.WorldMatrix.Forward, localThrusters[0].WorldMatrix.Forward);                

                float currentAngle = forwardAngle > Math.PI / 2 ? -downwardAngle : downwardAngle;               
                int direction = -1;                

                if (rotor.Orientation.Up == Base6Directions.Direction.Left)
                {
                    direction *= -1;
                }
                float angleDifference = targetAngle - currentAngle;
                rotor.TargetVelocityRad = 3 * angleDifference * direction;
            }
            return lastCurrentAngle;
        }

        private Vector3D GetCurrentStaticThrust()
        {
            List<IMyThrust> thrusters = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType(thrusters, t => t.CubeGrid == cockpit.CubeGrid);

            Vector3D sumThrust = Vector3D.Zero;            
            foreach(var thruster in thrusters)
            {
                sumThrust += thruster.CurrentThrust * thruster.WorldMatrix.Forward;
            }
            return sumThrust;
        }

        private void Foo(Vector3 input)
        {
            var currentThrust = GetCurrentStaticThrust();
            var gravity = cockpit.GetNaturalGravity() * cockpit.CalculateShipMass().TotalMass;
            var counterGravity = -gravity;

            var forceAgainstGravity = Vector3D.ProjectOnVector(ref currentThrust, ref counterGravity);
            var currentForces = forceAgainstGravity + gravity;

            var totalAvailableThrust = 0f;
            foreach (var thruster in rotorThrusters)
            {
                totalAvailableThrust += thruster.MaxEffectiveThrust;
            }

            var currentOrientation = GetCurrentOrientation();
            var counterGravityAngle = -currentOrientation.Pitch;

            var maxThrustVector = (cockpit.WorldMatrix.Up.Normalized() * totalAvailableThrust).Rotate(cockpit.WorldMatrix.Right, -counterGravityAngle);
            var maxThrustAgainstGravity = Vector3D.ProjectOnVector(ref maxThrustVector, ref currentForces);
            var minRequiredThrustRatio = (float)(currentForces.Length() / maxThrustAgainstGravity.Length());

            Echo($"Min required thrust percentage: {minRequiredThrustRatio * 100}%");
            if (minRequiredThrustRatio >= 1)
            {
                //TODO: just go to zero
            }

            var maxAvailableAngleOffset = (float)Math.Asin(minRequiredThrustRatio);
            var maxFwThrust = (float)Math.Sqrt((minRequiredThrustRatio - 1) * (minRequiredThrustRatio - 1) * totalAvailableThrust * totalAvailableThrust);
            var maxAcceleration = maxFwThrust / cockpit.CalculateShipMass().TotalMass;

            Echo($"Zero angle: {counterGravityAngle}");
            Echo($"maxFwThrust: {maxFwThrust}");
            Echo($"maxAcceleration: {maxAcceleration}");

            var upCommand = GetUpIntent(input, minRequiredThrustRatio, maxAcceleration);            
            var forwardCommand = GetForwardIntent(input, minRequiredThrustRatio, maxAcceleration);
            Echo($"upCommand: {upCommand}");
            Echo($"forwardCommand: {forwardCommand}");


            Vector2 command = new Vector2(forwardCommand, upCommand);
            if (command == Vector2.Zero)
            {
                SetRotorAngle(0);
                SetRotorThrusterOutput(0);
                return;
            };

            var commandDirection = command;
            commandDirection.Normalize();
            var angle = (float)((commandDirection.X < 0)
                ? (Math.PI - Math.Asin(commandDirection.Y))
                : (Math.Asin(commandDirection.Y)));

            Echo($"command: {command}");
            Echo($"angle: {angle}");
            
            var targetAngle = (counterGravityAngle + angle) - (float)Math.PI / 2;
            Echo($"targetAngle: {targetAngle}");
            var currentAngle = SetRotorAngle(targetAngle);

            // maxThrustVector
            var desiredThrust = command * totalAvailableThrust;

            var maxCurrentThrust = commandDirection;
            maxCurrentThrust.Rotate(targetAngle - currentAngle);
            maxCurrentThrust *= totalAvailableThrust;

            var projectedMaxCurrent = maxCurrentThrust.Length() * Math.Cos(targetAngle - currentAngle);

            var proposedRatio = desiredThrust.Length() / projectedMaxCurrent;
            Echo($"proposedRatio: {proposedRatio}");

            SetRotorThrusterOutput((float)proposedRatio);
        }

        private void SetRotorThrusterOutput(float ratio)
        {
            foreach(var thruster in rotorThrusters)
            {
                thruster.ThrustOverride = thruster.MaxEffectiveThrust * ratio;
            }
        }

        private float GetForwardIntent(Vector3 input, float minRequiredThrustRatio, float maxAcceleration)
        {
            if (input.Z > 0) return -(1 - minRequiredThrustRatio);
            if (input.Z < 0) return (1 - minRequiredThrustRatio);
            if (input.Z != 0) return -input.Z;

            if (currentMode == Mode.Coast) return 0;
            var maxFw = (float)Math.Sqrt(1 - minRequiredThrustRatio * minRequiredThrustRatio);

            // dampeners
            var velocities = cockpit.GetShipVelocities().LinearVelocity;
            var forward = cockpit.WorldMatrix.Forward;
            var forwardVelocity = Vector3D.ProjectOnVector(ref velocities, ref forward);
            bool isForward = Vector3D.Angle(forward, forwardVelocity) < Math.PI / 2;
            
            float minTimeToStop = (float)forwardVelocity.Length() / maxAcceleration;
            if (!isForward) minTimeToStop *= -1;
            Echo($"fw: {forwardVelocity}, {forwardVelocity.Length()}, {maxAcceleration}");
            return MyMath.Clamp(minTimeToStop, -maxFw, maxFw);
        }

        private float GetUpIntent(Vector3 input, float minRequiredThrustRatio, float maxAcceleration)
        {
            if (input.Y > 0) return input.Y;
            if (input.Y < 0) return 0;

            if (currentMode == Mode.Coast) return minRequiredThrustRatio;

            // dampeners
            var velocities = cockpit.GetShipVelocities().LinearVelocity;
            var up = -cockpit.GetNaturalGravity();
            var upVelocityVec = Vector3D.ProjectOnVector(ref velocities, ref up);
            var angle = Vector3D.Angle(upVelocityVec, up);
            bool goingUp = angle < Math.PI / 2;
            var velocity = (float)upVelocityVec.Length();

            if (goingUp)
            {
                float minTimeToStop = velocity / (float)up.Length();
                if (minTimeToStop > 1)
                {
                    return 0;
                }
                else
                {
                    return minRequiredThrustRatio * minTimeToStop;
                }
            }
            else
            {
                float minTimeToStop = velocity / maxAcceleration;
                return minRequiredThrustRatio + MyMath.Clamp(minTimeToStop, 0, 1 - minRequiredThrustRatio);
            }
        }
      

        private void Status(bool success, string status)
        {
            var surface = Me.GetSurface(0);
            surface.ContentType = ContentType.TEXT_AND_IMAGE;
            surface.BackgroundColor = success ? Color.Blue : Color.Red;
            surface.FontColor = Color.White;
            surface.FontSize = 1.5f;
            surface.WriteText($"Flight Assist\n" +
                $"Status: \n" +
                $"{status}", false);
        }
    }
}
