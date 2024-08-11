using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace SpaceEngineersScripts.Autopilots.FlightAssistLite
{
    public class Program : MyGridProgram
    {
        // Configuration
        string rotorGroupName = "Thruster rotors";



        // Script start
        IMyCockpit cockpit;
        List<IMyThrust> staticDownThrusters = new List<IMyThrust>();
        List<IMyMotorStator> rotors = new List<IMyMotorStator>();
        List<IMyThrust> rotorThrusters = new List<IMyThrust>();
        List<IMyGyro> gyros = new List<IMyGyro>();
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

            GridTerminalSystem.GetBlocksOfType(staticDownThrusters, thruster => thruster.CubeGrid == cockpit.CubeGrid
                && thruster.Orientation.Forward == Base6Directions.Direction.Down
                );
            GridTerminalSystem.GetBlockGroupWithName(rotorGroupName)?.GetBlocksOfType(rotors);
            GridTerminalSystem.GetBlocksOfType(rotorThrusters, thruster => thruster.CubeGrid != cockpit.CubeGrid && thruster.IsSameConstructAs(cockpit));
            GridTerminalSystem.GetBlocksOfType(gyros, g => g.IsSameConstructAs(cockpit));
            foreach(var gyro in gyros)
            {
                gyro.GyroOverride = true;
            }
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

            Echo("foo");

            // Get gravity
            var gravity = cockpit.GetTotalGravity();

            Foo(velocityInput);
            //SetRotors(velocityInput);
            //SetRotorThrusters();
        }

        private float ComputeSpareThrusterCapacity()
        {
            var currentThrust = 0f;
            var maxThrust = 0f;


            foreach(var thruster in staticDownThrusters)
            {
                currentThrust += thruster.CurrentThrust;
                maxThrust += thruster.MaxEffectiveThrust;                
            }

            foreach(var thruster in rotorThrusters)
            {
                currentThrust += thruster.CurrentThrust;
                maxThrust += thruster.MaxEffectiveThrust;
            }

            return 1 - currentThrust / maxThrust;
        }

        private Vector3D GetRelativeVelocities()
        {
            var velocities = cockpit.GetShipVelocities();
            var gravity = cockpit.GetNaturalGravity();

            Matrix cockpitMatrix;
            cockpit.Orientation.GetMatrix(out cockpitMatrix);
                        
            var absoluteForward = Vector3D.TransformNormal(cockpitMatrix.Forward, cockpit.WorldMatrix);

            var relativeRight = gravity.Cross(absoluteForward);
            var relativeForward = relativeRight.Cross(gravity);
            Matrix relativeOrientation = new Matrix();
            relativeOrientation.Forward = relativeForward.Normalized();
            relativeOrientation.Right = relativeRight.Normalized();
            relativeOrientation.Down = gravity.Normalized();
            relativeOrientation.M44 = 1;

            Matrix changeOfBaseMatrix = Matrix.Invert(relativeOrientation);
            var relativeVelocities = Vector3D.Transform(velocities.LinearVelocity, changeOfBaseMatrix);
            return relativeVelocities;
        }

        private void SetGyros(Vector3 input, float spareThrusterCapacity, float verticalSpeed)
        {
            var rollInput = cockpit.RollIndicator;
            var orientationInput = cockpit.RotationIndicator;
            var velocities = GetRelativeVelocities();
            velocities.Z = 0;
            var sideSlip = (float)cockpit.GetShipVelocities().LinearVelocity.X;
            var orientation = GetCurrentOrientation();

            float outputRoll = 0, outputYaw, outputPitch;
            

            if (rollInput != 0)
            {
                outputRoll = 5 * rollInput;
            } 
            else if (input.X != 0)
            {
            }
            else if (verticalSpeed < -0.15f || Math.Abs(sideSlip) < 0.1f)
            {
            }
            else
            {
            }

            Status(true, $"thruster: {spareThrusterCapacity}\nvspeed: {verticalSpeed}\ninput.X: {input.X}\nsideSlip: {sideSlip}\noutputRoll: \n");

            outputPitch = -orientationInput.X;
            outputYaw = orientationInput.Y;

            Vector3 target = new Vector3(outputPitch, outputYaw, outputRoll);
            Matrix cockpitMat;
            cockpit.Orientation.GetMatrix(out cockpitMat);
            Vector3 locGyroSet = Vector3.Transform(target, Matrix.Transpose(cockpitMat));
            foreach (var gyro in gyros)
            {
                Matrix currentGyroMat;
                gyro.Orientation.GetMatrix(out currentGyroMat);
                var ordersForCurrentGyro = Vector3.Transform(locGyroSet, currentGyroMat);

                gyro.Pitch = ordersForCurrentGyro.X;
                gyro.Yaw = ordersForCurrentGyro.Y;
                gyro.Roll = ordersForCurrentGyro.Z;
            }
        }

        private void SetRotorAngle(float targetAngle)
        {
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
            var minRequiredThrustRatio = currentForces.Length() / maxThrustAgainstGravity.Length();

            Echo($"Min required thrust percentage: {minRequiredThrustRatio * 100}%");
            if (minRequiredThrustRatio >= 1)
            {
                //TODO: just go to zero
            }

            var maxAvailableAngleOffset = (float)Math.Asin(minRequiredThrustRatio);
            var maxFwThrust = (float)Math.Sqrt((minRequiredThrustRatio - 1) * (minRequiredThrustRatio - 1) * totalAvailableThrust * totalAvailableThrust);
            var maxAcceleration = maxFwThrust / cockpit.CalculateShipMass().TotalMass;

            var maxForwardAngle = counterGravityAngle - maxAvailableAngleOffset;
            var maxBackwardAngle = counterGravityAngle + maxAvailableAngleOffset;

            Echo($"Zero angle: {counterGravityAngle}");
            Echo($"maxForwardAngle: {maxForwardAngle}");
            Echo($"maxBackwardAngle: {maxBackwardAngle}");
            Echo($"maxFwThrust: {maxFwThrust}");
            Echo($"maxAcceleration: {maxAcceleration}");

            var forwardCommand = GetForwardIntent(input, maxAcceleration);
            var upCommand = GetUpIntent(input, maxAcceleration);

            Vector2 command = new Vector2(forwardCommand, upCommand);
            if (command == Vector2.Zero)
            {
                SetRotorAngle(0);
                return;
            };

            command.Normalize();
            var angle = (float)Math.Asin(command.Y);
            if (command.X < 0) angle = (float)Math.PI - angle;

            Echo($"command: {command}");
            Echo($"angle: {angle}");

            var lerpRatio = Math.Abs(angle / (float)Math.PI);

            var targetAngle = maxForwardAngle - (maxForwardAngle - maxBackwardAngle) * lerpRatio;
            SetRotorAngle(targetAngle);

            // maxThrustVector
            var maxThrustAtCurrentAngle = rotorThrusters[0].WorldMatrix.Forward.Normalized() * totalAvailableThrust;

            /*
             * Thruster output: 
             * if no up/down command: exactly match so that 0 up velocity is created
             * if up command: full thrust
             * if down command: 
             */
        }

        private void SetRotorThrusterOutput(float ratio)
        {
            foreach(var thruster in rotorThrusters)
            {
                thruster.ThrustOverridePercentage = ratio * 100;
            }
        }

        private float GetForwardIntent(Vector3 input, float maxAcceleration)
        {
            if (input.Z != 0) return -input.Z;

            if (currentMode == Mode.Coast) return 0;

            // dampeners
            var velocities = cockpit.GetShipVelocities().LinearVelocity;
            var forward = cockpit.WorldMatrix.Forward;
            var forwardVelocity = Vector3D.ProjectOnVector(ref velocities, ref forward);

            float minTimeToStop = (float)forwardVelocity.Length() / maxAcceleration;
            return MyMath.Clamp(minTimeToStop, -1, 1);
        }

        private float GetUpIntent(Vector3 input, float maxAcceleration)
        {
            if (input.Y != 0) return input.Y;

            if (currentMode == Mode.Coast) return 0;

            // dampeners
            var velocities = cockpit.GetShipVelocities().LinearVelocity;
            var up = cockpit.WorldMatrix.Up;
            var upVelocity = Vector3D.ProjectOnVector(ref velocities, ref up);

            float minTimeToStop = (float)upVelocity.Length() / maxAcceleration;
            return MyMath.Clamp(minTimeToStop, -1, 1);
        }

        private void SetRotors(Vector3 input)
        {
            var currentOrientation = GetCurrentOrientation();
            var defaultAngle = -currentOrientation.Pitch;

            var totalAvailableThrust = 0f;
            foreach(var thruster in rotorThrusters)
            {
                totalAvailableThrust += thruster.MaxEffectiveThrust;
            }
            Echo($"Max thrust: {totalAvailableThrust}");

            if (input.Z > 0)
            {

            } 
            else if (input.Z < 0)
            {

            }
            else if (currentMode == Mode.Dampeners)
            {

            } 
            else
            {

            }
            SetRotorAngle(0);
        }

        private void SetRotorThrusters()
        {

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
