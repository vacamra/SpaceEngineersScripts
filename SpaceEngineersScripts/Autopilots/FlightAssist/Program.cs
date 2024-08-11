using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace SpaceEngineersScripts.Autopilots.FlightAssist
{
    public class Program : MyGridProgram
    {
        // Configuration
        string rotorGroupName = "Thruster rotors";



        // Script start
        IMyCockpit cockpit;
        List<IMyThrust> staticDownThrusters = new List<IMyThrust>();
        List<IMyMotorRotor> rotors = new List<IMyMotorRotor>();
        List<IMyThrust> rotorThrusters = new List<IMyThrust>();
        List<IMyGyro> gyros = new List<IMyGyro>();
        double lastElevation;


        DecayingIntegralPID rollPid = new DecayingIntegralPID(0.3f, 0.1f, 1f, 1, 0.8f);

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

            cockpit.TryGetPlanetElevation(MyPlanetElevation.Surface, out lastElevation);
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
            
            

            // Get gravity
            var gravity = cockpit.GetTotalGravity();

            var spareThrusterCapacity = ComputeSpareThrusterCapacity();

            SetGyros(velocityInput, spareThrusterCapacity, (float)(elevation - lastElevation));
            SetRotors();
            SetRotorThrusters();

            lastElevation = elevation;
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

            float outputRoll, outputYaw, outputPitch;
            

            if (rollInput != 0)
            {
                outputRoll = 5 * rollInput;
                rollPid.Reset();
            } 
            else if (input.X != 0)
            {
                outputRoll = rollPid.Control(input.X * (spareThrusterCapacity + verticalSpeed));
            }
            else if (verticalSpeed < -0.15f || Math.Abs(sideSlip) < 0.1f)
            {
                outputRoll = 20 * rollPid.Control(orientation.Roll);
            }
            else
            {
                outputRoll = rollPid.Control(-sideSlip);
            }

            Status(true, $"thruster: {spareThrusterCapacity}\nvspeed: {verticalSpeed}\ninput.X: {input.X}\nsideSlip: {sideSlip}\noutputRoll: {outputRoll}\n");

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

        private void SetRotors()
        {

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







        public class PID
        {
            public float Kp { get; set; } = 0;
            public float Ki { get; set; } = 0;
            public float Kd { get; set; } = 0;
            public float Value { get; private set; }

            float _timeStep = 0;
            float _inverseTimeStep = 0;
            float _errorSum = 0;
            float _lastError = 0;
            bool _firstRun = true;

            public PID(float kp, float ki, float kd, float timeStep)
            {
                Kp = kp;
                Ki = ki;
                Kd = kd;
                _timeStep = timeStep;
                _inverseTimeStep = 1 / _timeStep;
            }

            protected virtual float GetIntegral(float currentError, float errorSum, float timeStep)
            {
                return errorSum + currentError * timeStep;
            }

            public float Control(float error)
            {
                //Compute derivative term
                float errorDerivative = (error - _lastError) * _inverseTimeStep;

                if (_firstRun)
                {
                    errorDerivative = 0;
                    _firstRun = false;
                }

                //Get error sum
                _errorSum = GetIntegral(error, _errorSum, _timeStep);

                //Store this error as last error
                _lastError = error;

                //Construct output
                Value = Kp * error + Ki * _errorSum + Kd * errorDerivative;
                return Value;
            }

            public float Control(float error, float timeStep)
            {
                if (timeStep != _timeStep)
                {
                    _timeStep = timeStep;
                    _inverseTimeStep = 1 / _timeStep;
                }
                return Control(error);
            }

            public virtual void Reset()
            {
                _errorSum = 0;
                _lastError = 0;
                _firstRun = true;
            }
        }

        public class DecayingIntegralPID : PID
        {
            public float IntegralDecayRatio { get; set; }

            public DecayingIntegralPID(float kp, float ki, float kd, float timeStep, float decayRatio) : base(kp, ki, kd, timeStep)
            {
                IntegralDecayRatio = decayRatio;
            }

            protected override float GetIntegral(float currentError, float errorSum, float timeStep)
            {
                return errorSum * (1.0f - IntegralDecayRatio) + currentError * timeStep;
            }
        }
    }
}
