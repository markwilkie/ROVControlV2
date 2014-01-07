using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GAW;
using System.Drawing;
using System.Collections;

namespace ROVControl
{
    class CompassControl
    {
        //Control handle
        CircleControl compass;

        //Compass
        private CircleControl.MarkerSet arrow;
        private CircleControl.MarkerSet person;

        public CompassControl(CircleControl circleControl)
        {
            compass = circleControl;

            InitCompass();
        }

        //Compass control init
        private void InitCompass()
        {
            compass.MarkerSets.Clear();

            // Hour ticks
            compass.MajorTicks = 8;
            compass.MajorTickStart = 0.3f;
            compass.MajorTickSize = 0.60f;
            compass.MajorTickThickness = 1.0f;

            //Add ring
            //compass.Rings.Clear();
            compass.Rings.Add(.9f, Color.Transparent, Color.White, 1.0f);
            compass.Rings.Add(1f, Color.Transparent, Color.White, 1.0f);

            // Arrow hand
            float offset = .45f;
            ArrayList arrowPoly = new ArrayList();
            arrowPoly.Add(new PointF(0.225F + offset, 0.00F));
            arrowPoly.Add(new PointF(0.175F + offset, 0.10F));
            arrowPoly.Add(new PointF(0.40F + offset, 0.00f));
            arrowPoly.Add(new PointF(0.175F + offset, -0.10F));
            arrow = new CircleControl.MarkerSet();
            arrow.Add(CircleControl.MakeArgb(0.8f, Color.White), Color.Black, 1.0f, (PointF[])arrowPoly.ToArray(typeof(PointF)));
            compass.MarkerSets.Add(arrow);

            // Person marker
            offset = .5f;
            ArrayList personPoly = new ArrayList();
            personPoly.Add(new PointF(-.125f, 0f + offset));
            personPoly.Add(new PointF(-.025f, .15f + offset));
            personPoly.Add(new PointF(-.025f, .2f + offset));
            personPoly.Add(new PointF(-.1f, .15f + offset));
            personPoly.Add(new PointF(-.1f, .2f + offset));
            personPoly.Add(new PointF(-.025f, .25f + offset));
            personPoly.Add(new PointF(-.025f, .275f + offset));
            personPoly.Add(new PointF(-.05f, .275f + offset));
            personPoly.Add(new PointF(-.05f, .35f + offset));
            personPoly.Add(new PointF(.05f, .35f + offset));
            personPoly.Add(new PointF(.05f, .275f + offset));
            personPoly.Add(new PointF(.025f, .275f + offset));
            personPoly.Add(new PointF(.025f, .25f + offset));
            personPoly.Add(new PointF(.1f, .2f + offset));
            personPoly.Add(new PointF(.1f, .15f + offset));
            personPoly.Add(new PointF(.025f, .2f + offset));
            personPoly.Add(new PointF(.025f, .15f + offset));
            personPoly.Add(new PointF(.125f, 0f + offset));
            personPoly.Add(new PointF(.075f, 0f + offset));
            personPoly.Add(new PointF(0f, .1f + offset));
            personPoly.Add(new PointF(-.075f, 0f + offset));
            person = new CircleControl.MarkerSet();
            person.Add(CircleControl.MakeArgb(0.8f, Color.Red), Color.Black, 1.0f, (PointF[])personPoly.ToArray(typeof(PointF)));
            compass.MarkerSets.Add(person);
        }
    }
}
