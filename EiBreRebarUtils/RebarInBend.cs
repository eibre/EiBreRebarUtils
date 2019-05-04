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

            Reference pickedReference = null;
            try
            {
                pickedReference = uidoc.Selection.PickObject(ObjectType.Element, new RebarSelectFilter() , "Pick a Rebar");
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }

            Rebar rebar = doc.GetElement(pickedReference) as Rebar;
            if (!rebar.IsRebarShapeDriven() || rebar.LayoutRule == RebarLayoutRule.Single)
            {
                message = "Singe rebar and non-shape driven rebars are not supported.";
                return Result.Failed;
            }
            double rebarDiameter = rebar.GetBendData().BarDiameter;
            RebarBarType barType = doc.GetElement(rebar.GetTypeId()) as RebarBarType;

            IList<Curve> transformedCurvesFirst = GetTransformedCenterLineCurvesAtPostition(rebar, 0);
            IList<Curve> transformedCurvesLast = GetTransformedCenterLineCurvesAtPostition(rebar, rebar.NumberOfBarPositions-1);

            XYZ direction = transformedCurvesFirst.OfType<Line>().First().Direction;

            List<XYZ> rebarInBendFirstPoints = GetPointInArc(transformedCurvesFirst, 0);
            List<XYZ> rebarInBendLastPoints = GetPointInArc(transformedCurvesLast, rebar.NumberOfBarPositions-1);

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

        private List<XYZ> GetPointInArc(IList<Curve> transformedCurves, double diameter)
        {
            List<XYZ> rebarInBendPoints = new List<XYZ>();

            foreach (Arc arc in transformedCurves.OfType<Arc>())
            {
                XYZ center = arc.Center;
                XYZ arcMidpoint = arc.Evaluate(0.5, true);
                Line line = Line.CreateBound(center, arcMidpoint);
                double offset = line.Length - diameter;
                rebarInBendPoints.Add(line.Evaluate(offset, false));
            }

            return rebarInBendPoints;
        }

        private IList<Curve> GetTransformedCenterLineCurvesAtPostition(Rebar rebar, int barPosIndex)
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
    } //class
} //namespace