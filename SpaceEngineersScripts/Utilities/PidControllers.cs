using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEngineersScripts.Utilities
{
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

    public class ClampedIntegralPID : PID
    {
        public float IntegralUpperBound { get; set; }
        public float IntegralLowerBound { get; set; }

        public ClampedIntegralPID(float kp, float ki, float kd, float timeStep, float lowerBound, float upperBound) : base(kp, ki, kd, timeStep)
        {
            IntegralUpperBound = upperBound;
            IntegralLowerBound = lowerBound;
        }

        protected override float GetIntegral(float currentError, float errorSum, float timeStep)
        {
            errorSum = errorSum + currentError * timeStep;
            return Math.Min(IntegralUpperBound, Math.Max(errorSum, IntegralLowerBound));
        }
    }

    public class BufferedIntegralPID : PID
    {
        readonly Queue<float> _integralBuffer = new Queue<float>();
        public int IntegralBufferSize { get; set; } = 0;

        public BufferedIntegralPID(float kp, float ki, float kd, float timeStep, int bufferSize) : base(kp, ki, kd, timeStep)
        {
            IntegralBufferSize = bufferSize;
        }

        protected override float GetIntegral(float currentError, float errorSum, float timeStep)
        {
            if (_integralBuffer.Count == IntegralBufferSize)
                _integralBuffer.Dequeue();
            _integralBuffer.Enqueue(currentError * timeStep);
            return _integralBuffer.Sum();
        }

        public override void Reset()
        {
            base.Reset();
            _integralBuffer.Clear();
        }
    }
}
