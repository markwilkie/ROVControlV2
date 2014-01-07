using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace JoystickSample
{
    public partial class POV : UserControl
    {
        public POV()
        {
            InitializeComponent();
        }

        private int tiltPos = 32767/2;
        private int panPos = 32767/2;

        public int TiltPos
        {
            set 
            {
                //lblPOV.Text = "Axis: " + axisId + "  Value: " + value;
                tbTiltPos.Value = value;
                tiltPos = value; 
            }
        }

        public int PanPos
        {
            set
            {
                //lblPOV.Text = "Axis: " + axisId + "  Value: " + value;
                tbPanPos.Value = value;
                panPos = value;
            }
        }

        private void POV_Load(object sender, EventArgs e)
        {

        }


    }
}
