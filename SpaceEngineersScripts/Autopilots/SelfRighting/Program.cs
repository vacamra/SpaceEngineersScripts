using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace SpaceEngineersScripts.Autopilots.SelfRighting
{
    public class Program : MyGridProgram
    {
        PID pid;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            pid = new PID(2, 0, 5, 10);
        }

        private void Status(bool success, string status)
        {
            var surface = Me.GetSurface(0);
            surface.ContentType = ContentType.TEXT_AND_IMAGE;
            surface.BackgroundColor = success ? Color.Blue : Color.Red;
            surface.FontColor = Color.White;
            surface.FontSize = 2;
            surface.WriteText($"Automated crafting\n" +
                $"Status: \n" +
                $"{status}", false);
        }

        private struct AtmosphericOrientation
        {
            public float Pitch;
            public float Roll;
        }

        private AtmosphericOrientation GetCurrentOrientation()
        {
            var cockpit = GridTerminalSystem.GetBlockWithName("Cockpit") as IMyCockpit;
            var gravity = cockpit.GetNaturalGravity();
            return new AtmosphericOrientation
            {
                Roll = MyMath.AngleBetween(cockpit.WorldMatrix.Right, gravity) - (float)Math.PI / 2f,
                Pitch = MyMath.AngleBetween(cockpit.WorldMatrix.Forward, gravity) - (float)Math.PI / 2f
            };
        }

        public void Main(string argument)
        {
            List<IMyThrust> thrusters = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType(thrusters);

            List<IMyGyro> gyros = new List<IMyGyro>();
            GridTerminalSystem.GetBlocksOfType(gyros, gyro => gyro.GyroOverride);
                        
            var cockpit = GridTerminalSystem.GetBlockWithName("Cockpit") as IMyCockpit;
            var orientation = GetCurrentOrientation();

            float roll = pid.Control(orientation.Roll);
            
            foreach(var gyro in gyros)
            {
                switch (gyro.Orientation.Forward)
                {
                    case Base6Directions.Direction.Forward:
                        gyro.Roll = roll;
                        gyro.Pitch = 0;
                        gyro.Yaw = 0;
                        break;
                    case Base6Directions.Direction.Backward:
                        gyro.Roll = -roll;
                        gyro.Yaw = 0;
                        gyro.Pitch = 0;
                        break;
                    case Base6Directions.Direction.Left:
                        gyro.Roll = 0;
                        gyro.Pitch = roll;
                        gyro.Yaw = 0;
                        break;
                    case Base6Directions.Direction.Right:
                        gyro.Roll = 0;
                        gyro.Pitch = -roll;
                        gyro.Yaw = 0;
                        break;
                    case Base6Directions.Direction.Up:
                        gyro.Roll = 0;
                        gyro.Pitch = 0;
                        gyro.Yaw = roll;
                        break;
                    case Base6Directions.Direction.Down:
                        gyro.Roll = 0;
                        gyro.Pitch = 0;
                        gyro.Yaw = -roll;
                        break;
                }
            }

            cockpit.GetSurface(1).WriteText($"{orientation.Roll}\n{orientation.Pitch}\n{roll}", false);
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
    }
}
