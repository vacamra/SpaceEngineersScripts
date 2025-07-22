using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceEngineersScripts.Utilities
{
    public class PID
    {
        public float P { get; set; } = 0;
        public float I { get; set; } = 0;
        public float D { get; set; } = 0;

        float timeStep = 0;
        float errorSum = 0;
        float lastError = 0;
        bool firstRun = true;

        public PID(float p, float i, float d, float timeStep)
        {
            P = p;
            I = i;
            D = d;
            this.timeStep = timeStep;
        }

        protected virtual float GetIntegral(float currentError, float errorSum, float timeStep)
        {
            return errorSum + currentError * timeStep;
        }

        public float Control(float error)
        {
            float errorDerivative = (error - lastError) / timeStep;

            if (firstRun)
            {
                errorDerivative = 0;
                firstRun = false;
            }

            errorSum = GetIntegral(error, errorSum, timeStep);
            lastError = error;

            return P * error + I * errorSum + D * errorDerivative;
        }

        public float Control(float error, float timeStep)
        {
            if (timeStep != this.timeStep)
            {
                this.timeStep = timeStep;
            }
            return Control(error);
        }

        public virtual void Reset()
        {
            errorSum = 0;
            lastError = 0;
            firstRun = true;
        }
    }

    public class DecayingIntegralPID : PID
    {
        public float IntegralDecayRatio { get; set; }

        public DecayingIntegralPID(float p, float i, float d, float timeStep, float decayRatio) : base(p, i, d, timeStep)
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
