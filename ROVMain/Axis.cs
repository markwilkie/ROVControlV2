using System;
using System.Collections.Generic;
using System.Text;

namespace ROVControl
{
    class Axis
    {
        //private properties
        const int POINTS_ON_CURVE = 2;
        BezierCalculator bc = new BezierCalculator();
        List<double> axisPosList = new List<double>();
        private int currentPosition;
        private int trimOffset;
        private bool invertFlag = false;
        
        //public properties
        public int maxPosition;
        public int centerPosition;
        
        //last n points for x/y from joystick, and z list from throttle
        private int pointsToUse = Properties.Settings.Default.SMOOTHING_FACTOR;

        //Joystick sensivity curve on/off
        private bool joystickSensitivityCurve = Properties.Settings.Default.JOYSTICK_SENSITIVITY_CURVE;

        //Constructor
        public Axis(int max)
        {
            maxPosition = max;
            centerPosition = maxPosition / 2;
            currentPosition = centerPosition;
            trimOffset = 0;
        }

        //Constructor
        public Axis(int max,bool invert)
        {
            maxPosition = max;
            centerPosition = maxPosition / 2;
            currentPosition = centerPosition;
            trimOffset = 0;
            invertFlag = invert;
        }

        //Set current position
        public void SetCurrentPosition(int pos)
        {
            //Make sure we're still in bounds
            if (pos > maxPosition) pos = maxPosition;
            if (pos < 0) pos = 0;

            currentPosition = pos;
        }

        //Get current position, factoring in spline and trim
        public int GetLatestPosition()
        {
            //Start with current position
            int latestPosition = currentPosition;

            //Apply mapping if on
            if(joystickSensitivityCurve)
                latestPosition = SplineEvaluation(currentPosition);
            
            //Factor trim in
            latestPosition = latestPosition + (trimOffset * 10);

            //Make sure we're still in bounds
            if (latestPosition > maxPosition) latestPosition = maxPosition;
            if (latestPosition < 0) latestPosition = 0;

            return latestPosition;
        }

        //Get current position as a percentage.  Will return a positive number both sides of zero or center.
        //
        //This is only useful when you also know direction
        //
        public int GetLatestPositionAbsPercentage()
        {
            //Get latest position (according to curve and trim)
            int lastestPos = GetLatestPosition();

            //Get absolute position
            int absPos = lastestPos < centerPosition ? centerPosition - lastestPos : lastestPos - centerPosition;

            //Figure percentage and return
            double perc = ((double)absPos / (double)centerPosition);
            perc = perc * 100;

            return (int)perc;
        }

        //Returns a positive or negative percentage
        public int GetLatestPositionPercentage()
        {
            //Get percentage (absolute)
            int perc = GetLatestPositionAbsPercentage();

            //if less than center, make below zero
            perc = PositionLessThanCenter() ? perc = perc * -1 : perc;

            //invert if necessary
            perc = invertFlag ? perc = perc * -1 : perc;

            return perc;
        }

        //Get direction
        public bool PositionLessThanCenter()
        {
            return currentPosition < centerPosition ? true : false;
        }

        //Smooth out "jerky" joystick
        public void SmoothInput()
        {
            //If POINTS_TO_USE is zero, simply return
            if (pointsToUse <= 0)
                return;
            //
            //First, smooth the curve out using Bezier
            if (axisPosList.Count > pointsToUse)
                axisPosList.RemoveAt(0);
            axisPosList.Add(currentPosition);

            double[] ptind = new double[axisPosList.Count];
            double[] curvePointResults = new double[POINTS_ON_CURVE];
            axisPosList.CopyTo(ptind, 0);
            bc.Bezier2D(ptind, (POINTS_ON_CURVE) / 2, curvePointResults);

            //reset own position to be more smooth
            currentPosition = (int)curvePointResults[1];
        }

        //Use spline curve to determine input mapping to reduce sensitivity
        private int SplineEvaluation(int position)
        {
            //Setup x andy
            double[] x = { 0, 50, 100, 150, 200, 250, 300, 350, 400, 450, 500, 550, 600, 650, 700, 750, 800, 850, 900, 950, 999 };
            double[] y = { 0, 100, 210, 280, 350, 400, 440, 470, 490, 500, 500, 500, 510, 530, 560, 600, 650, 720, 790, 900, 999 };

            //Convert joystick input to the percentage this algorythmn needs
            double joystickPerc = ((double)position / maxPosition) * 1000;

            //use AlgLib to setup curve - then, find interval
            alglib.spline1dinterpolant s;
            alglib.spline1dbuildlinear(x, y, out s);
            double newJoyPerc = alglib.spline1dcalc(s, joystickPerc);

            //Recal new joystick position now
            return ((int)(maxPosition * (newJoyPerc / 1000)));
        }

        public void SetTrimOffset(int offset)
        {
            trimOffset = offset;
        }

        //Set trim offset
        public void SetTrimOffset(int trimAmt, int maxTrim)
        {
            trimOffset = trimAmt - (maxTrim / 2);
        }

        //Get trim offset
        public int GetTrimOffset()
        {
            return trimOffset;
        }
    }
}
