using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scada
{
        public class PID
        {
            #region Fields

            public double pv;
            public double sp;

            //Gains
            private double kp;
            private double ki;
            private double kd;

            //Running Values
            private DateTime lastUpdate;
            private double lastPV;
            private double errSum;

            //Reading/Writing Values
            //private GetDouble readPV;
            //private GetDouble readSP;
            //private SetDouble writeOV;

            //Max/Min Calculation
            private double pvMax;
            private double pvMin;
            private double outMax;
            private double outMin;

            //Threading and Timing
           // private double computeHz = 1.0f;
            private Thread runThread;

            #endregion

            #region Properties

            public double PGain
            {
                get { return kp; }
                set { kp = value; }
            }

            public double IGain
            {
                get { return ki; }
                set { ki = value; }
            }

            public double DGain
            {
                get { return kd; }
                set { kd = value; }
            }

            public double PVMin
            {
                get { return pvMin; }
                set { pvMin = value; }
            }

            public double PVMax
            {
                get { return pvMax; }
                set { pvMax = value; }
            }

            public double OutMin
            {
                get { return outMin; }
                set { outMin = value; }
            }

            public double OutMax
            {
                get { return outMax; }
                set { outMax = value; }
            }

            public bool PIDOK
            {
                get { return runThread != null; }
            }

            #endregion

            #region Construction / Deconstruction

            public void initPID(double pG, double iG, double dG,
                double pMax, double pMin, double oMax, double oMin
                /*GetDouble pvFunc, GetDouble spFunc, SetDouble outFunc */)
            {
                kp = pG;
                ki = iG;
                kd = dG;
                pvMax = pMax;
                pvMin = pMin;
                outMax = oMax;
                outMin = oMin;
                //readPV = pvFunc;
                //readSP = spFunc;
                //writeOV = outFunc;
            }

            /*
            ~PID()
            {
                Disable();
                readPV = null;
                readSP = null;
                writeOV = null;
            }
            */
            #endregion

            #region Public Methods
            /*
            public void Enable()
            {
                if (runThread != null)
                    return;

                Reset();

                runThread = new Thread(new ThreadStart(Run));
                runThread.IsBackground = true;
                runThread.Name = "PID Control";
                runThread.Start();
            }

            public void Disable()
            {
                if (runThread == null)
                    return;

                runThread.Abort();
                runThread = null;
            }
            */
            public void Reset()
            {
                errSum = 0.0f;
                lastUpdate = DateTime.Now;
            }

            #endregion

            #region Private Methods

            private double ScaleValue(double value, double valuemin,
                    double valuemax, double scalemin, double scalemax)
            {
                double vPerc = (value - valuemin) / (valuemax - valuemin);
                double bigSpan = vPerc * (scalemax - scalemin);

                double retVal = scalemin + bigSpan;

                return retVal;
            }

            private double Clamp(double value, double min, double max)
            {
                if (value > max)
                    return max;
                if (value < min)
                    return min;
                return value;
            }

            public double ComputePID()
            {
                /*
                if (readPV == null || readSP == null || writeOV == null)
                    return(0);

                double pv = readPV();
                double sp = readSP();
                */
                //We need to scale the pv to +/- 100%, but first clamp it
                pv = Clamp(pv, pvMin, pvMax);
                pv = ScaleValue(pv, pvMin, pvMax, -1.0f, 1.0f);

                //We also need to scale the setpoint
                sp = Clamp(sp, pvMin, pvMax);
                sp = ScaleValue(sp, pvMin, pvMax, -1.0f, 1.0f);

                //Now the error is in percent...
                double err = sp - pv;

                double pTerm = err * kp;
                double iTerm = 0.0f;
                double dTerm = 0.0f;

                double partialSum = 0.0f;
                DateTime nowTime = DateTime.Now;

              
                    double dT = (nowTime - lastUpdate).TotalSeconds;

                    //Compute the integral if we have to...
                    if (pv >= pvMin && pv <= pvMax)
                    {
                        partialSum = errSum + dT * err;
                        iTerm = ki * partialSum;
                    }

                    if (dT != 0.0f)
                        dTerm = kd * (pv - lastPV) / dT;
                

                lastUpdate = nowTime;
                errSum = partialSum;
                lastPV = pv;

                //Now we have to scale the output value to match the requested scale
                double outReal = pTerm + iTerm + dTerm;

                outReal = Clamp(outReal, -1.0f, 1.0f);
                outReal = ScaleValue(outReal, -1.0f, 1.0f, outMin, outMax);

                //Write it out to the world
                return(outReal);
            }

            #endregion

            #region Threading

            /*
            private void Run()
            {
                while (true)
                {
                    try
                    {
                        int sleepTime = (int)(1000 / computeHz);
                        Thread.Sleep(sleepTime);
                        Compute();
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            */
            #endregion
        }

    public class SignalGenerator
    {
        private SignalType signalType = SignalType.Sine;

        public SignalType SignalType
        {
            get { return signalType; }
            set { signalType = value; }
        }

        private float frequency = 1f;

        public float Frequency
        {
            get { return frequency; }
            set { frequency = value; }
        }

        private float phase = 0f;
 
        public float Phase
        {
            get { return phase; }
            set { phase = value; }
        }

        private float amplitude = 1f;

        public float Amplitude
        {
            get { return amplitude; }
            set { amplitude = value; }
        }

        private float offset = 0f;
     
        public float Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        private float invert = 1; // Yes=-1, No=1
  
        public bool Invert
        {
            get { return invert == -1; }
            set { invert = value ? -1 : 1; }
        }


 
        /// Time the signal generator was started
        private long startTime = Stopwatch.GetTimestamp();


        /// Ticks per second on this CPU
        private long ticksPerSecond = Stopwatch.Frequency;


        public SignalGenerator(SignalType initialSignalType)
        {
            signalType = initialSignalType;
        }

        public SignalGenerator() { }


        public float GetValue(float time)
        {
            float value = 0f;
            float t = frequency * time + phase;
            switch (signalType)
            { 
                case SignalType.Sine: // sin( 2 * pi * t )
                    value = (float)Math.Sin(2f * Math.PI * t);
                    break;
                case SignalType.Square: // sign( sin( 2 * pi * t ) )
                    value = Math.Sign(Math.Sin(2f * Math.PI * t));
                    break;
                case SignalType.Triangle:
                    // 2 * abs( t - 2 * floor( t / 2 ) - 1 ) - 1
                    value = 1f - 4f * (float)Math.Abs
                        (Math.Round(t - 0.25f) - (t - 0.25f));
                    break;
                case SignalType.Sawtooth:
                    // 2 * ( t/a - floor( t/a + 1/2 ) )
                    value = 2f * (t - (float)Math.Floor(t + 0.5f));
                    break;
            }

            return (invert * amplitude * value + offset);
        }

        public float GetValue()
        {
            float time = (float)(Stopwatch.GetTimestamp() - startTime)
                            / ticksPerSecond;
            return GetValue(time);
        }

        public void Reset()
        {
            startTime = Stopwatch.GetTimestamp();
        }

    }


    public enum SignalType
    {
        Sine,
        Square,
        Triangle,
        Sawtooth
    }
    public enum ControllerType
    {
        OnOff,
        Pid,       
    }


    public class Heater
    {
        private ControllerType controllerType = ControllerType.OnOff;

        public ControllerType ControllerType
        {
            get { return controllerType; }
            set { controllerType = value; }
        }

        private float frequency = 1f;

        public float Frequency
        {
            get { return frequency; }
            set { frequency = value; }
        }

        private float phase = 0f;

        public float Phase
        {
            get { return phase; }
            set { phase = value; }
        }

        private float amplitude = 1f;

        public float Amplitude
        {
            get { return amplitude; }
            set { amplitude = value; }
        }

        private float offset = 0f;

        public float Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        private float invert = 1; // Yes=-1, No=1

        public bool Invert
        {
            get { return invert == -1; }
            set { invert = value ? -1 : 1; }
        }



        /// Time the signal generator was started
        private long startTime = Stopwatch.GetTimestamp();


        /// Ticks per second on this CPU
        private long ticksPerSecond = Stopwatch.Frequency;


        public Heater() { }


        public float GetValue(float time)
        {
            float value = 0f;
            float t = frequency * time + phase;
            switch (controllerType)
            {
                case ControllerType.OnOff:
                    value = 50;
                    break;
                case ControllerType.Pid:
                    value = 51;
                    break;
      
            }

            return ( value );
        }

        public float GetValue()
        {
            float time = (float)(Stopwatch.GetTimestamp() - startTime)
                            / ticksPerSecond;
            return GetValue(time);
        }

        public void Reset()
        {
            startTime = Stopwatch.GetTimestamp();
        }

    }



}
