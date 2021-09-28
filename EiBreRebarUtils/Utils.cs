using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;


namespace NO.RebarUtils
{

    public class Utils
    {
        /// <summary>
        /// Get Transformed Centerline curves at position.
        /// This method extracts the centerlinecurves for a rebar set at a given index and transforms them to the correct position.
        /// </summary>
        /// <param name="rebar"></param>
        /// <param name="barPosIndex"></param>
        /// <returns></returns>
        public static IList<Curve> GetTransformedCenterLineCurvesAtPostition(Rebar rebar, int barPosIndex)
        {
            RebarShapeDrivenAccessor sda = rebar.GetShapeDrivenAccessor();
            Transform transform = sda.GetBarPositionTransform(barPosIndex);
            IList<Curve> curves = rebar.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, barPosIndex);
            IList<Curve> transformedCurves = new List<Curve>();
            foreach (Curve curve in curves)
            {
                transformedCurves.Add(curve.CreateTransformed(transform));
            }
            return transformedCurves;
        }

        /// <summary>
        /// Project Point To Plane. Projects a XYZ to a plane. 
        /// https://thebuildingcoder.typepad.com/blog/2014/09/planes-projections-and-picking-points.html#12
        /// </summary>
        /// <param name="pickedPoint">A point to project</param>
        /// <param name="viewPlane">A plane to project the point on to</param>
        /// <returns>projected point</returns>
        internal static XYZ ProjectPointToPlane(XYZ pickedPoint, Plane viewPlane)
        {
            XYZ v = pickedPoint - viewPlane.Origin;
            double signedDistance = viewPlane.Normal.DotProduct(v);
            return pickedPoint - signedDistance * viewPlane.Normal;
        }

        /// <summary>
        /// Check if two vectors are parallell
        /// </summary>
        /// <param name="p">first vector</param>
        /// <param name="q">second vector</param>
        /// <returns>true if the vectors are parallell, false otherwise</returns>
        internal static bool IsParallel(XYZ p, XYZ q)
        {
            return p.CrossProduct(q).IsZeroLength();
        }
    } //class
} //namespace