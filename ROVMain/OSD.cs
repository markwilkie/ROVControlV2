using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Video
{
    class OSD
    {
        Brush brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(255, Color.Red));
        Font font = new System.Drawing.Font("Tahoma", 12);

        //Constants for video window resolution.  Used for placement of on screen video
        private const int XRES = 640;
        private const int YRES = 480;
        private const int TOPROW = 0;

        //vars
        private int xCenter = XRES / 2;
        private int yCenter = YRES / 2;

        private String headingStr;
        private int headingX;


        public void videoBoxControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            e.Graphics.DrawString(headingStr, font, brush, headingX, TOPROW);  //draw heading

            //e.Graphics.DrawString("Hello World", f, b, 0, 0);
            //e.Graphics.DrawString("Centered on Screen", f, b, 312, 10);
            //e.Graphics.DrawString("Hello World222", f, b, 50, 50);
        }

        //Called to update value - this is NOT painted until the form calls the _Paint member above
        public void setHeading(int heading)
        {
            headingStr = "H: " + heading.ToString();
            headingX = xCenter - (headingStr.Length / 2);
        }
 
    }
}
