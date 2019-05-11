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


namespace EiBreRebarUtils
{

    public class Utils
    {
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

        internal static XYZ ProjectPointToPlane(XYZ pickedPoint, Plane viewPlane)
        {
            XYZ v = pickedPoint - viewPlane.Origin;
            double signedDistance = viewPlane.Normal.DotProduct(v);
            return pickedPoint - signedDistance * viewPlane.Normal;
        }
    } //class
} //namespace