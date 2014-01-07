using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace GAW
{
    partial class CircleControl
    {
        /// <summary>
        /// 	Represents a set of <c>Marker</c> objects which move as a group when any member of the set is dragged.
        /// </summary>
		public class MarkerSet : CollectionItem
		{
		    /// <summary>
		    /// 	Initializes a new instance of the <c>MarkerSet</c> class with an initial angle of 0.0,
			///     and includeInFixedBackground set to false.
		    /// </summary>
			public MarkerSet() : this(0.0f)
			{ }

			/// <summary>
			/// 	Initializes a new instance of the <c>MarkerSet</c> class with includeInFixedBackground set to false.
			/// </summary>
			/// <param name="angle">Initial angle.</param>
			public MarkerSet(float angle) : this(angle, false)
			{ }

			/// <summary>
			/// 	Initializes a new instance of the <c>MarkerSet</c> class.
			/// </summary>
			/// <param name="angle">Initial angle.</param>
			/// <param name="includeInFixedBackground"><c>True</c> if this marker set is part of the fixed background, <c>false</c> otherwise.</param>
			public MarkerSet(float angle, bool includeInFixedBackground) : base()
			{
				this.angle = angle;
				this.includeInFixedBackground = includeInFixedBackground;
				this.markers = new List<Marker>();
			}

			/// <summary>
			/// 	Produces a copy of this <c>MarkerSet</c>.
			/// 	The new instance does not belong to any <c>CircleControl</c>.
			/// </summary>
			/// <returns>A new <c>MarkerSet</c>.</returns>
			public MarkerSet Clone()
			{
				MarkerSet ms = new MarkerSet(angle);
				for (int i = 0; i < markers.Count; i++)
				{
					Marker m = markers[i].Clone();
					ms.Add(m);
				}
				return ms;
			}

			private float angle;
			/// <summary>
			/// 	Gets or sets the angle.
			/// </summary>
			/// <value>
			/// 	The angular location of the marker set.
			/// </value>
			public float Angle
			{
				get { return angle; }
				set
				{
					if (Cc == null)
					{
						angle = value;
					}
					else
					{
						float a = Cc.AdjustAngle(value);
						if (a != angle)
						{
							angle = a;
							Redraw(includeInFixedBackground);
						}
					}
				}
			}

			private bool includeInFixedBackground;
			/// <summary>
			/// 	Gets or sets the includeInFixedBackground flag.
			/// </summary>
			/// <value>
			/// 	<c>True</c> if this marker set is part of the fixed background, <c>false</c> otherwise.
			/// </value>
			/// <seealso cref="CircleControl.FixedBackground" />
			public bool IncludeInFixedBackground
			{
				get { return includeInFixedBackground; }
				set
				{
					if (Cc == null)
					{
						includeInFixedBackground = value;
					}
					else if (includeInFixedBackground != value)
					{
						includeInFixedBackground = value;
						Redraw(true);
					}
				}
			}

			private List<Marker> markers;

			/// <summary>
			/// 	Gets the <c>Marker</c> at the specified index.
			/// </summary>
			/// <value>
			/// 	<c>Marker</c> at the specified index.
			/// </value>
			/// <param name="index">The zero-based index of the <c>Marker</c> to get.</param>
			/// <returns>
			///		The <c>Marker</c> at the specified index.
			/// </returns>
			/// <exception cref="System.ArgumentOutOfRangeException">
			///		Thrown if <paramref name="index" /> is less than zero or greater than <c>Count</c>.
			///	</exception>
			public Marker this[int index]
			{
				get { return markers[index]; }
			}

			/// <summary>
			/// 	Gets the number of <c>Marker</c> objects contained in the <c>MarkerSet</c>.
			/// </summary>
			/// <value>
			/// 	The number of <c>Marker</c> objects contained in the <c>MarkerSet</c>.
			/// </value>
			public int Count 
			{
				get { return markers.Count; }
			}

			internal void CheckIfNull(Marker marker)
			{
				if (marker == null)
				{
					throw new ArgumentNullException("marker");
				}
			}

			internal void Check(Marker marker)
			{
				CheckIfNull(marker);
				if (marker.Ms != null)
				{
					throw new ArgumentException(string.Format("Cannot add or insert the item '{0}' in more than one place.  You must first remove it from its current location or clone it.", marker),
												"marker");
				}
			}

			/// <summary>
			/// 	Returns the zero-based index of the occurence of a <c>Marker</c> in the <c>MarkerSet</c>.
			/// </summary>
			/// <param name="marker">The <c>Marker</c> to locate in the <c>MarkerSet</c>.</param>
			/// <returns>
			///		The zero-based index of <paramref name="marker" />, if found; otherwise -1.
			/// </returns>
			/// <exception cref="System.ArgumentNullException">
			/// 	Thrown if <paramref name="marker" /> is null.
			/// </exception>
			public int IndexOf(Marker marker)
			{
				CheckIfNull(marker);
				return markers.IndexOf(marker);
			}

			/// <summary>
			/// 	Adds a <c>Marker</c> to the end of the <c>MarkerSet</c>.
			/// </summary>
			/// <param name="marker">The <c>Marker</c> to be added to the end of the <c>MarkerSet</c>.</param>
			/// <exception cref="System.ArgumentNullException">
			/// 	Thrown if <paramref name="marker" /> is null.
			/// </exception>
			/// <exception cref="System.ArgumentException">
			/// 	Thrown if <paramref name="marker" /> already belongs to a <c>MarkerSet</c>.
			/// </exception>
			public void Add(Marker marker)
			{
				Check(marker);
				marker.Ms = this;
				markers.Add(marker);
				Redraw(includeInFixedBackground);
			}

			/// <summary>
			/// 	Initializes a new instance of the <c>Marker</c> class with a solid color at offset angle 0.0 and adds it to the end of the <c>MarkerSet</c>.
			/// 	The marker can be dragged with the left mouse button and is visible.
			/// </summary>
			/// <param name="solidColor">Internal color.</param>
			/// <param name="borderColor">Border color.</param>
			/// <param name="borderSize">Border thickness (in pixels).</param>
			/// <param name="points">Array of <c>System.Drawing.PointF</c> structures that represent the vertices of the marker when the marker is at zero degrees.</param>
			/// <remarks>
			///		Refer to the Overview for a discussion of the coordinate system used by <c>points</c>.
			/// </remarks>
			public void Add(Color solidColor, Color borderColor, float borderSize, PointF[] points)
			{
				Add(new Marker(solidColor, borderColor, borderSize, points));
			}

			/// <summary>
			/// 	Initializes a new instance of the <c>Marker</c> class with a solid color and adds it to the end of the <c>MarkerSet</c>.
			/// 	The marker is visible.
			/// </summary>
			/// <param name="solidColor">Internal color.</param>
			/// <param name="borderColor">Border color.</param>
			/// <param name="borderSize">Border thickness (in pixels).</param>
			/// <param name="points">Array of <c>System.Drawing.PointF</c> structures that represent the vertices of the marker when the marker is at zero degrees.</param>
			/// <param name="offsetAngle">Offset angle of marker within <c>MarkerSet</c>.</param>
			/// <param name="dragButtons"><c>System.Windows.Forms.MouseButtons</c> value indicating which mouse button(s) drag this marker.  If the value is <c>MouseButtons.None</c>, then this marker cannot be dragged.</param>
			/// <remarks>
			///		Refer to the Overview for a discussion of the coordinate system used by <c>points</c>.
			/// </remarks>
			public void Add(Color solidColor, Color borderColor, float borderSize, PointF[] points, float offsetAngle, MouseButtons dragButtons)
			{
				Add(new Marker(solidColor, borderColor, borderSize, points, offsetAngle, dragButtons));
			}

			/// <summary>
			/// 	Initializes a new instance of the <c>Marker</c> class with a solid color and adds it to the end of the <c>MarkerSet</c>.
			/// </summary>
			/// <param name="solidColor">Internal color.</param>
			/// <param name="borderColor">Border color.</param>
			/// <param name="borderSize">Border thickness (in pixels).</param>
			/// <param name="points">Array of <c>System.Drawing.PointF</c> structures that represent the vertices of the marker when the marker is at zero degrees.</param>
			/// <param name="offsetAngle">Offset angle of marker within <c>MarkerSet</c>.</param>
			/// <param name="dragButtons"><c>System.Windows.Forms.MouseButtons</c> value indicating which mouse button(s) drag this marker.  If the value is <c>MouseButtons.None</c>, then this marker cannot be dragged.</param>
			/// <param name="visible"><c>True</c> if the marker is visible, <c>false</c> otherwise.</param>
			/// <remarks>
			///		Refer to the Overview for a discussion of the coordinate system used by <c>points</c>.
			/// </remarks>
			public void Add(Color solidColor, Color borderColor, float borderSize, PointF[] points, float offsetAngle, MouseButtons dragButtons, bool visible)
			{
				Add(new Marker(solidColor, borderColor, borderSize, points, offsetAngle, dragButtons, visible));
			}

            /// <summary>
			/// 	Initializes a new instance of the <c>Marker</c> class with radial gradient coloring and adds it to the end of the <c>MarkerSet</c>.
            /// </summary>
			/// <param name="point1">Starting point of gradient.</param>
			/// <param name="point2">Ending point of gradient.</param>
			/// <param name="color1">Color of marker at starting point of gradient.</param>
			/// <param name="color2">Color of marker at ending point of gradient.</param>
			/// <param name="borderColor">Border color.</param>
			/// <param name="borderSize">Border thickness (in pixels).</param>
			/// <param name="points">Array of <c>System.Drawing.PointF</c> structures that represent the vertices of the marker when the marker is at zero degrees.</param>
			/// <param name="offsetAngle">Offset angle of marker within <c>MarkerSet</c>.</param>
			/// <param name="dragButtons"><c>System.Windows.Forms.MouseButtons</c> value indicating which mouse button(s) drag this marker.  If the value is <c>MouseButtons.None</c>, then this marker cannot be dragged.</param>
			/// <param name="visible"><c>True</c> if the marker is visible, <c>false</c> otherwise.</param>
			/// <remarks>
			///		Refer to the Overview for a discussion of the coordinate system used by <c>points</c>.
			/// </remarks>
            public void Add(PointF point1, PointF point2, Color color1, Color color2, Color borderColor, float borderSize, PointF[] points, float offsetAngle, MouseButtons dragButtons, bool visible)
            {
                Add(new Marker(point1, point2, color1, color2, borderColor, borderSize, points, offsetAngle, dragButtons, visible));
            }

			/// <summary>
			/// 	Initializes a new instance of the <c>Marker</c> class with linear gradient coloring and adds it to the end of the <c>MarkerSet</c>.
			/// </summary>
			/// <param name="gradientMode">A <c>System.Drawing.Drawing2D.LinearGradientMode</c> enumeration value that specifies the orientation of the gradient.</param>
			/// <param name="startColor">Starting color.</param>
			/// <param name="endColor">Ending color.</param>
			/// <param name="borderColor">Border color.</param>
			/// <param name="borderSize">Border thickness (in pixels).</param>
			/// <param name="points">Array of <c>System.Drawing.PointF</c> structures that represent the vertices of the marker when the marker is at zero degrees.</param>
			/// <param name="offsetAngle">Offset angle of marker within <c>MarkerSet</c>.</param>
			/// <param name="dragButtons"><c>System.Windows.Forms.MouseButtons</c> value indicating which mouse button(s) drag this marker.  If the value is <c>MouseButtons.None</c>, then this marker cannot be dragged.</param>
			/// <param name="visible"><c>True</c> if the marker is visible, <c>false</c> otherwise.</param>
			/// <remarks>
			///		Refer to the Overview for a discussion of the coordinate system used by <c>points</c>.
			/// </remarks>
			public void Add(LinearGradientMode gradientMode, Color startColor, Color endColor, Color borderColor, float borderSize, PointF[] points, float offsetAngle, MouseButtons dragButtons, bool visible)
			{
                Add(new Marker(gradientMode, startColor, endColor, borderColor, borderSize, points, offsetAngle, dragButtons, visible));
			}

            /// <summary>
			/// 	Initializes a new instance of the <c>Marker</c> class with a hatch pattern and adds it to the end of the <c>MarkerSet</c>.
            /// </summary>
			/// <param name="hatchStyle">A <c>System.Drawing.Drawing2D.HatchStyle</c> enumeration value that specifies the style of hatching.</param>
			/// <param name="foreColor">Hatch lines color.</param>
			/// <param name="backColor">Background color.</param>
			/// <param name="borderColor">Border color.</param>
			/// <param name="borderSize">Border thickness (in pixels).</param>
			/// <param name="points">Array of <c>System.Drawing.PointF</c> structures that represent the vertices of the marker when the marker is at zero degrees.</param>
			/// <param name="offsetAngle">Offset angle of marker within <c>MarkerSet</c>.</param>
			/// <param name="dragButtons"><c>System.Windows.Forms.MouseButtons</c> value indicating which mouse button(s) drag this marker.  If the value is <c>MouseButtons.None</c>, then this marker cannot be dragged.</param>
			/// <param name="visible"><c>True</c> if the marker is visible, <c>false</c> otherwise.</param>
			/// <remarks>
			///		Refer to the Overview for a discussion of the coordinate system used by <c>points</c>.
			/// </remarks>
            public void Add(HatchStyle hatchStyle, Color foreColor, Color backColor, Color borderColor, float borderSize, PointF[] points, float offsetAngle, MouseButtons dragButtons, bool visible)
            {
                Add(new Marker(hatchStyle, foreColor, backColor, borderColor, borderSize, points, offsetAngle, dragButtons, visible));
            }

			/// <summary>
			/// 	Inserts a <c>Marker</c> into the <c>MarkerSet</c> at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index at which <paramref name="marker" />should be inserted.</param>
			/// <param name="marker">The <c>Marker</c> to insert.</param>
			/// <exception cref="System.ArgumentNullException">
			/// 	Thrown if <paramref name="marker" /> is null.
			/// </exception>
			/// <exception cref="System.ArgumentException">
			/// 	Thrown if <paramref name="marker" /> already belongs to a <c>MarkerSet</c>.
			/// </exception>
			/// <exception cref="System.ArgumentOutOfRangeException">
			/// 	Thrown if <paramref name="index" /> is less than zero or greater than <c>Count</c>.
			/// </exception>
			public void Insert(int index, Marker marker)
			{
				Check(marker);
				if (index < 0 || index > Count)
				{
					throw new ArgumentOutOfRangeException();
				}
                marker.Ms = this;
				markers.Insert(index, marker);
				Redraw(includeInFixedBackground);
			}

			private void RemoveAt2(int index)
			{
				Marker marker = markers[index];
				marker.Ms = null;
				markers.RemoveAt(index);
			}

			/// <summary>
			/// 	Removes the <c>Marker</c> at the specified index of the <c>MarkerSet</c>
			/// </summary>
			/// <param name="index">The zero-based index of the <c>Marker</c> to remove.</param>
			/// <exception cref="System.ArgumentOutOfRangeException">
			/// 	Thrown if <paramref name="index" /> is less than zero or greater than <c>Count</c>.
			/// </exception>
			public void RemoveAt(int index)
			{
				if (index <= 0 || index >= Count)
				{
					throw new ArgumentOutOfRangeException();
				}
				RemoveAt2(index);
				Redraw(includeInFixedBackground);
			}

			/// <summary>
			/// 	Removes the occurence of the <c>Marker</c> from the <c>MarkerSet</c>.
			/// </summary>
			/// <param name="marker">The <c>Marker</c> to remove from <c>MarkerSet</c>.</param>
			/// <exception cref="System.ArgumentNullException">
			///  	Thrown if <paramref name="marker" /> is null.
			/// </exception>
			public void Remove(Marker marker)
			{
				int index = IndexOf(marker);
				if (index >= 0)
				{
					RemoveAt(index);
				}
			}

			/// <summary>
			/// 	Removes all the <c>Marker</c> objects from the <c>MarkerSet</c>.
			/// </summary>
			public void Clear()
			{
				while (Count > 0)
				{
					RemoveAt2(0);
				}
				Redraw(includeInFixedBackground);
			}
		}
	}
}
