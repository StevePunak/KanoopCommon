using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KanoopCommon.Geometry
{
    /// <summary>
    /// Simple container class to allow specification of "X" and "Y" offsets; both Point and Size can be used for this purpose
    /// but using this class clearly states that the "X" and "Y" are not point coordinates or width/heights, but offsets from
    /// a particular point
    /// </summary>
    public class Offset
    {
        #region Private fields
        private Double m_DeltaX;
        private Double m_DeltaY;
        #endregion

        #region Public Properties
        /// <summary>
        /// [R] Accessor for DeltaX
        /// </summary>
        public Double DeltaX
        {
            get {
                    return m_DeltaX;
                }
        }

        /// <summary>
        /// [R] Accessor for DeltaY
        /// </summary>
        public Double DeltaY
        {
            get {
                    return m_DeltaY;
                }
        }

        /// <summary>
        /// Magnitude of the offset
        /// </summary>
        public Double Magnitude
        {
            get
            {
                return Math.Sqrt(m_DeltaX * m_DeltaX + m_DeltaY * m_DeltaY);
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Default contructor for this class
        /// </summary>
        public Offset ()
        {
            CoreConstruction (0.0, 0.0);
        }

        /// <summary>
        /// Constructor with the ability to specify both deltax and deltay
        /// </summary>
        /// <param name="deltax">the proposed delta x</param>
        /// <param name="deltay">the proposed delta y</param>
        public Offset (Double deltax, Double deltay)
        {
            CoreConstruction (deltax, deltay);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Core of class contructor
        /// </summary>
        /// <param name="deltax">the proposed delta x</param>
        /// <param name="deltay">the proposed delta y</param>
        private void CoreConstruction (Double deltax, Double deltay)
        {
            m_DeltaX = deltax;
            m_DeltaY = deltay;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Update deltax and deltay
        /// </summary>
        /// <param name="deltax">the proposed delta x</param>
        /// <param name="deltay">the proposed delta y</param>
        public void Update (Double deltax, Double deltay)
        {
            CoreConstruction (deltax, deltay);
        }

        /// <summary>
        /// return a point whose coordinates are offset by the current deltax and deltay
        /// </summary>
        /// <param name="point">the point before an offset is applied</param>
        /// <returns>a PointD whose coordinates are offset from the input point</returns>
        public PointD OffsetPoint (PointD point)
        {
            return new PointD (point.X + m_DeltaX, point.Y + m_DeltaY);
        }

        /// <summary>
        /// return a rectangleD whose vertices are offset by the current deltax and deltay
        /// </summary>
        /// <param name="rectangle">the rectangle before an offset is applied</param>
        /// <returns>a RectangleD whose coordinates are offset from the input point</returns>
        public RectangleD OffsetRectangle (RectangleD rectangle)
        {
            return new RectangleD   (
                                        rectangle.ToRectangle ().Left + m_DeltaX, rectangle.ToRectangle ().Top + m_DeltaY,
                                        rectangle.Width, rectangle.Height
                                    );
        }
        #endregion

        #region Overrides
        /// <summary>
        /// An override to ToString
        /// </summary>
        /// <returns>a string representation of the offset</returns>
        public override String ToString()
        {
            return m_DeltaX.ToString() + "," + m_DeltaY.ToString();
        }
        #endregion
    }
}
