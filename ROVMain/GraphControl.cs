using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GraphLib;
using System.Drawing;

namespace ROVControl
{
    class GraphControl
    {
        frmMain frmMain;
        PlotterDisplayEx display;

        int x = 0;
        int y = 30;

        public GraphControl(frmMain fm, PlotterDisplayEx pd)
        {
            frmMain = fm;
            display = pd;

            InitGraph();
        }

        public void InitGraph()
        {
            frmMain.SuspendLayout();
            display.Smoothing = System.Drawing.Drawing2D.SmoothingMode.None;

            display.DataSources.Clear();
            display.SetDisplayRangeX(0, 160);

            int NumGraphs = 2;
            for (int j = 0; j < NumGraphs; j++)
            {
                display.DataSources.Add(new DataSource());
                display.DataSources[j].Name = "Graph " + (j + 1);
                //display.DataSources[j].OnRenderXAxisLabel += RenderXLabel;

                //this.Text = "TEXT";
                display.DataSources[j].Length = 162;
                display.PanelLayout = PlotterGraphPaneEx.LayoutMode.NORMAL;
                display.DataSources[j].AutoScaleY = false;
                display.DataSources[j].AutoScaleX = false;
                display.DataSources[j].SetDisplayRangeY(0, 100);
                display.DataSources[j].SetGridDistanceY(25);
                //display.DataSources[j].OnRenderYAxisLabel = RenderYLabel;
                //display.DataSources[j].XAutoScaleOffset=0;
            }

            //graph tital and line color
            // display.DataSources[0].Name = "Graph";
            display.DataSources[0].GraphColor = Color.Blue;
            display.DataSources[1].GraphColor = Color.Red;

            frmMain.ResumeLayout();
            display.Refresh();
        }

        public void GraphTimer_Tick()
        {
            if (display.DataSources.Count == 0)
                return;

            UpdateGraph(x++, y);
            if (x > 160)
            {
                x = 0;
                y = y + 3;
            }
        }

        public void UpdateGraph(int x, int y)
        {
            display.DataSources[0].Samples[x].x = x;
            display.DataSources[0].Samples[x].y = y;

            display.DataSources[1].Samples[x].x = x;
            display.DataSources[1].Samples[x].y = y + 8;

            display.Refresh();
        }
    }
}
