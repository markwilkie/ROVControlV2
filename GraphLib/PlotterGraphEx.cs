using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel.Design;
using System.Drawing.Drawing2D;

 
/* Copyright (c) 2008,2009 DI Zimmermann Stephan (stefan.zimmermann@tele2.at)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to 
 * deal in the Software without restriction, including without limitation the 
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or 
 * sell copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software. 
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
 * THE SOFTWARE.
 */

 
namespace GraphLib
{        
    public partial class PlotterDisplayEx : UserControl
    {

        #region MEMBERS

        delegate void InvokeVoidFuncDelegate();

        #endregion

        #region CONSTRUCTOR

        public PlotterDisplayEx()
        {
            InitializeComponent();

            //Get rid of the dang tool bar
            splitContainer1.Panel1Collapsed = true;
            splitContainer1.Panel1.Hide();
        }
         
        #endregion

        #region PROPERTIES

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(System.ComponentModel.Design.CollectionEditor),
               typeof(System.Drawing.Design.UITypeEditor))]
        public List<DataSource> DataSources
        {
            get { return gPane.Sources; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(System.ComponentModel.Design.CollectionEditor),
               typeof(System.Drawing.Design.UITypeEditor))]
        public PlotterGraphPaneEx.LayoutMode PanelLayout
        {
            get { return gPane.layout; }
            set { gPane.layout = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(System.ComponentModel.Design.CollectionEditor),
               typeof(System.Drawing.Design.UITypeEditor))]
        public SmoothingMode Smoothing
        {
            get { return gPane.smoothing; }
            set { gPane.smoothing = value; }
        }

        [Category("Playback")]
        [DefaultValue(typeof(bool), "true")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool ShowMovingGrid
        {
            get { return gPane.hasMovingGrid; }
            set { gPane.hasMovingGrid = value; }
        }


        [Category("Properties")]   
        [DefaultValue(typeof(Color), "")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color BackgroundColorTop
        {
            get { return gPane.BgndColorTop; }
            set { gPane.BgndColorTop = value; }
        }

        [Category("Properties")]  
        [DefaultValue(typeof(Color), "")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color BackgroundColorBot
        {
            get { return gPane.BgndColorBot; }
            set { gPane.BgndColorBot = value; }
        }

        [Category("Properties")]  
        [DefaultValue(typeof(Color), "")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color DashedGridColor
        {
            get { return gPane.MinorGridColor; }
            set { gPane.MinorGridColor = value; }
        }

        [Category("Properties")]
        [DefaultValue(typeof(Color), "")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Color SolidGridColor
        {
            get { return gPane.MajorGridColor; }
            set { gPane.MajorGridColor = value; }
        }
                
      
        public bool DoubleBuffering
        {
            get { return gPane.useDoubleBuffer; }
            set { gPane.useDoubleBuffer = value; }
        }

        #endregion

        #region PUBLIC METHODS

        public void SetDisplayRangeX(float x_start, float x_end )
        {
            gPane.XD0 = x_start;
            gPane.XD1 = x_end;
            gPane.CurXD0 = gPane.XD0;
            gPane.CurXD1 = gPane.XD1;
        }
       
        public void SetGridDistanceX(float grid_dist_x_samples)
        {
            gPane.grid_distance_x = grid_dist_x_samples;           
        }
      
        public void SetGridOriginX(float off_x)
        {
            gPane.grid_off_x = off_x;          
        }

        
        #endregion

        #region PRIVATE METHODS

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        private void UpdateControl()
        {
            try
            {
                bool AllAutoscaled = true;

                foreach (DataSource s in gPane.Sources)
                {
                    AllAutoscaled &= s.AutoScaleX;
                }
            }
            catch
            {
            }
        }

        #endregion

    }
}
