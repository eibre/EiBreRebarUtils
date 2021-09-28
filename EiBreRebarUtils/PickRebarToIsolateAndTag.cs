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

    //Set the attributes
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]

    public class PickRebarToIsolateAndTag : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            while (true) //restart until user press ESC
            {
                Application app = commandData.Application.Application;
                UIDocument uidoc = commandData.Application.ActiveUIDocument;
                Document doc = uidoc.Document;
                double mmToFeet = 1.0 / 304.8;
                Reference pickedRebarRef = null;
                Reference pickedDimensionRef = null;
                try
                {
                    pickedRebarRef = uidoc.Selection.PickObject(ObjectType.Element, new RebarSelectFilter(), "Pick a Rebar, TAB to cycle, ESC to cancel.");
                    pickedDimensionRef = uidoc.Selection.PickObject(ObjectType.Element, new DimensionSelectFilter(), "Pick a Dimension, TAB to cycle, ESC to cancel.");
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Succeeded;
                }

                //Get rebar info
                Rebar rebar = doc.GetElement(pickedRebarRef) as Rebar;
                XYZ pickedPoint = pickedRebarRef.GlobalPoint;
                int barPositionIndex = 0;
                Line rebarSegment = RebarLineNearestPickedPoint(rebar, pickedPoint, out barPositionIndex);

                //Get dimension info
                Dimension dim = doc.GetElement(pickedDimensionRef) as Dimension;

                //Get view info
                View view = doc.ActiveView;
                double viewScale = System.Convert.ToDouble(view.get_Parameter(BuiltInParameter.VIEW_SCALE).AsInteger());
                XYZ viewOrigin = view.Origin;
                XYZ viewUpDir = view.UpDirection;
                XYZ viewRightDir = view.RightDirection;
                XYZ viewDir = view.ViewDirection;
                Plane viewPlane = Plane.CreateByNormalAndOrigin(viewDir, viewOrigin);

                //Project picked point to calculate tag placement
                XYZ picketPtOnView = Utils.ProjectPointToPlane(pickedPoint, viewPlane);
                XYZ pickedPtOnDim = dim.Curve.Project(picketPtOnView).XYZPoint;
                XYZ projectDir = Line.CreateBound(picketPtOnView, pickedPtOnDim).Direction;

                //Set tag orientation
                double angleToUp = projectDir.AngleOnPlaneTo(viewUpDir, viewDir);
                bool tagAlignmentIsVertical = false;
                TagOrientation tagOrientation = TagOrientation.Horizontal;
                if ((angleToUp < 0.25 * Math.PI) || (angleToUp > 0.75 * Math.PI && angleToUp < 1.25 * Math.PI) || (angleToUp > 1.75 * Math.PI))
                {
                    tagOrientation = TagOrientation.Vertical;
                    tagAlignmentIsVertical = true;
                }

                //Set position av tag:
                XYZ tagPoint = pickedPtOnDim;
                double dotProduct = 0.0;
                if (tagAlignmentIsVertical)
                {
                    dotProduct = viewUpDir.DotProduct(projectDir);
                }
                else
                {
                    dotProduct = viewRightDir.DotProduct(projectDir);
                }

                if (dotProduct < -0.01)
                {
                    tagPoint = tagPoint.Add(projectDir.Normalize().Multiply(viewScale * mmToFeet * 50));
                }
                else
                {
                    tagPoint = tagPoint.Add(projectDir.Normalize().Multiply(viewScale * mmToFeet * 3));
                }

                // Get reference of picked rebar, this can be used to attach rebar to dimension line
                Options opt = new Options();
                opt.View = view;
                opt.ComputeReferences = true;
                opt.IncludeNonVisibleObjects = true;
                List<Line> geomLines = new List<Line>();
                GeometryElement rebarGeom = rebar.get_Geometry(opt);
                Reference rebRef = null;
                foreach (GeometryObject geomObj in rebarGeom)
                {
                    Line geomLine = geomObj as Line;
                    if (null != geomLine)
                    {
                        XYZ p = geomLine.Direction;
                        XYZ q = rebarSegment.Direction;
                        bool isParallel = p.CrossProduct(q).IsZeroLength();

                        if (isParallel == false)
                        {
                            continue;
                        }
                        XYZ endPointOfRebar = rebarSegment.GetEndPoint(1);
                        IntersectionResult ir = geomLine.Project(
                          endPointOfRebar);
                        if (ir == null)
                            continue; // end point of rebar segment is not on the reference curve.

                        if (Math.Abs(ir.Distance) != 0)
                            continue; // end point of rebar segment is not on the reference curve.
                        rebRef = geomLine.Reference;
                    }
                }

                using (Transaction t1 = new Transaction(doc, "Isolate and tag Rebar"))
                {
                    t1.Start();
                    for (int i = 0; i < rebar.NumberOfBarPositions; i++)
                    {
                        if (i == barPositionIndex)
                        {
                            rebar.SetBarHiddenStatus(doc.ActiveView, barPositionIndex, false);
                        }
                        else
                        {
                            rebar.SetBarHiddenStatus(doc.ActiveView, i, true);
                        }
                    }
                    ElementId tagId = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RebarTags).ToElementIds().FirstOrDefault();
                    IndependentTag.Create(doc, tagId, doc.ActiveView.Id, pickedRebarRef, false, tagOrientation, tagPoint);

                    //Recreate dimension with added reference:
                    if (rebRef != null)
                    {
                        ReferenceArray refArray = dim.References;
                        refArray.Append(rebRef);
                        Line dimLine = dim.Curve as Line;
                        DimensionType dimType = doc.GetElement(dim.GetTypeId()) as DimensionType;
                        doc.Create.NewDimension(doc.ActiveView, dimLine, refArray, dimType);
                        doc.Delete(dim.Id);
                    }
                    t1.Commit();
                }
            }
        }

        /// <summary>
        /// Calculates the closest centerline of a rebar in a rebar set and returns the bar position index and the line.
        /// </summary>
        /// <param name="rebar">A Rebar Element (Rebar set)</param>
        /// <param name="point">A point near one of the bars in a set</param>
        /// <param name="barPositionIndex"></param>
        /// <returns></returns>
        private Line RebarLineNearestPickedPoint(Rebar rebar, XYZ point, out int barPositionIndex)
        {
            Line rebarSegment = null;
            barPositionIndex = 0;
            double distance = -1.0;
            for (int i = 0; i < rebar.NumberOfBarPositions; i++) {
                Line centerLine = null;
                IList<Curve> centerLineCurves = Utils.GetTransformedCenterLineCurvesAtPostition(rebar, i);
                if(centerLineCurves.Count > 1)
                {   
                    //prefer longest line
                    double length = 0.0;
                    foreach (Curve curve in centerLineCurves)
                    {
                        if (curve is Line)
                        {
                            Line line = curve as Line;
                            if(line.Length > length)
                            {
                                length = line.Length;
                                centerLine = line;
                            }
                        }
                    }
                }
                else
                {
                    centerLine = centerLineCurves.First() as Line;
                }
                if(i==0)
                {
                    distance = centerLine.Distance(point);
                    rebarSegment = centerLine;
                    barPositionIndex = i;
                }
                else if (centerLine.Distance(point) < distance)
                {
                    distance = centerLine.Distance(point);
                    rebarSegment = centerLine;
                    barPositionIndex = i;
                }
            }
            return rebarSegment;
        }
    } //class
} //namespace