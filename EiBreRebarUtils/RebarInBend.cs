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

    //Set the attributes
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]

    public class RebarInBend : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Reference pickedReference = uidoc.Selection.PickObject(ObjectType.Element, "Pick a Rebar");
            if (pickedReference == null)
            {
                message = "Nothing is selected";
                return Result.Failed;
            }

            Rebar rebar = doc.GetElement(pickedReference) as Rebar;
            if (!rebar.IsRebarShapeDriven() && rebar.LayoutRule == RebarLayoutRule.Single)
            {
                message = "Singe rebar and non-shape driven rebars are not supported.";
                return Result.Failed;
            }
            double rebarDiameter = rebar.GetBendData().BarDiameter;
            RebarBarType barType = doc.GetElement(rebar.GetTypeId()) as RebarBarType;

            RebarShapeDrivenAccessor sda = rebar.GetShapeDrivenAccessor();
            Transform firstTransform = sda.GetBarPositionTransform(0);
            Transform lastTransform = sda.GetBarPositionTransform(rebar.NumberOfBarPositions - 1);
            IList<Curve> firstCurves = rebar.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, 0);
            IList<Curve> lastCurves = rebar.GetCenterlineCurves(false, false, false, MultiplanarOption.IncludeOnlyPlanarCurves, rebar.NumberOfBarPositions - 1);
            IList<Curve> firstTransformedCurves = new List<Curve>();
            IList<Curve> lastTransformedCurves = new List<Curve>();
            foreach (Curve curve in firstCurves)
            {
                firstTransformedCurves.Add(curve.CreateTransformed(firstTransform));
            }
            foreach (Curve curve in lastCurves)
            {
                lastTransformedCurves.Add(curve.CreateTransformed(lastTransform));
            }
            XYZ direction = firstTransformedCurves.OfType<Line>().First().Direction;

            List<XYZ> rebarInBendFirstPoints = new List<XYZ>();
            foreach (Arc arcFirst in firstTransformedCurves.OfType<Arc>())
            {
                XYZ center = arcFirst.Center;
                XYZ arcMidpoint = arcFirst.Evaluate(0.5, true);
                Line line = Line.CreateBound(center, arcMidpoint);
                double offset = line.Length - rebarDiameter;
                rebarInBendFirstPoints.Add(line.Evaluate(offset, false));
            }
            List<XYZ> rebarInBendLastPoints = new List<XYZ>();
            foreach (Arc arcLast in lastTransformedCurves.OfType<Arc>())
            {
                XYZ center = arcLast.Center;
                XYZ arcMidpoint = arcLast.Evaluate(0.5, true);
                Line line = Line.CreateBound(center, arcMidpoint);
                double offset = line.Length - rebarDiameter;
                rebarInBendLastPoints.Add(line.Evaluate(offset, false));
            }

            using (Transaction t1 = new Transaction(doc, "Add rebar in bend"))
            {
                t1.Start();
                for (int i = 0; i < rebarInBendFirstPoints.Count; i++)
                {
                    Line newRebarCenterline = Line.CreateBound(rebarInBendFirstPoints[i], rebarInBendLastPoints[i]);
                    IList<Curve> rebarCurve = new List<Curve>();
                    rebarCurve.Add(newRebarCenterline);
                    Rebar.CreateFromCurves(doc, RebarStyle.Standard, barType, null, null, doc.GetElement(rebar.GetHostId()), direction, rebarCurve, RebarHookOrientation.Left, RebarHookOrientation.Left, true, false);
                }
                doc.Regenerate();
                t1.Commit();
            }

            return Result.Succeeded;
        }
    } //class
} //namespace