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

    public class TagToDim : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            Reference pickedRefTag = null;
            Reference pickedRefDim = null;
            try
            {
                pickedRefTag = uidoc.Selection.PickObject(ObjectType.Element, new TagSelectFilter() , "Pick a Tag");
                pickedRefDim = uidoc.Selection.PickObject(ObjectType.Element, new DimensionSelectFilter() , "Pick a Dimension");
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }

            View view = doc.ActiveView;

            IndependentTag tag = doc.GetElement(pickedRefTag) as IndependentTag;
            Dimension dim = doc.GetElement(pickedRefDim) as Dimension;

            if(tag == null || dim == null)
            {
                message = "There were too few elements selected, please pick a tag and a dimension.";
                return Result.Failed;
            }

            Line dimLine = dim.Curve as Line;
            XYZ dimStartPt = dim.Origin.Subtract((dimLine.Direction).Multiply((double)dim.Value / 2));
            XYZ dimEndPt = dim.Origin.Add((dimLine.Direction).Multiply((double)dim.Value / 2));
            XYZ leaderEndPt = dimStartPt;
            if (dimStartPt.DistanceTo(tag.TagHeadPosition) > dimEndPt.DistanceTo(tag.TagHeadPosition))
            {
                leaderEndPt = dimEndPt;
            }

            Plane viewPlane = Plane.CreateByNormalAndOrigin(view.ViewDirection, view.Origin);
            XYZ leaderElbowPt = null;
            XYZ tagHeadPt = null;

            XYZ tagDirection = null;
            if (tag.TagOrientation == TagOrientation.Horizontal)
            {
                tagDirection = view.RightDirection;
                if (view.RightDirection.X > 0)
                {
                    tagHeadPt = new XYZ(tag.TagHeadPosition.X, dim.Origin.Y, viewPlane.Origin.Z);
                }

            }
            else
            {
                tagDirection = view.UpDirection;
                if (view.UpDirection.Y > 0)
                {
                    tagHeadPt = new XYZ(dim.Origin.X, tag.TagHeadPosition.Y, viewPlane.Origin.Z);
                }

            }

            XYZ dimOriginProj = Utils.ProjectPointToPlane(dim.Origin, viewPlane);
            XYZ tagOriginProj = Utils.ProjectPointToPlane(tag.TagHeadPosition, viewPlane);
            Line dimLineProj = Line.CreateUnbound(dimOriginProj, dimLine.Direction);
            Line tagLineProj = Line.CreateUnbound(tagOriginProj, tagDirection);
            if (!Utils.IsParallel(dimLineProj.Direction, tagLineProj.Direction))
            {
                IntersectionResultArray results;
                dimLineProj.Intersect(tagLineProj, out results);
                IntersectionResult result = results.get_Item(0);
                leaderElbowPt = result.XYZPoint;
            }

            using (Transaction t1 = new Transaction(doc, "tagdim"))
            {
                t1.Start();
                tag.HasLeader = true;
                tag.LeaderEndCondition = LeaderEndCondition.Free;

                tag.SetLeaderEnd(tag.GetTaggedReferences().First(), leaderEndPt);
                if (leaderElbowPt != null)
                {
                    tag.SetLeaderElbow(tag.GetTaggedReferences().First(), leaderElbowPt);
                }
                else if (tagHeadPt != null)
                {
                    tag.TagHeadPosition = tagHeadPt;
                }

                t1.Commit();
            }
            return Result.Succeeded;
        }

 

    } //class
} //namespace