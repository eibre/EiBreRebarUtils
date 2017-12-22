//Add references
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
  
    //Helper class
    public class RebarHelper
    {
        public static List<ElementId> TaggedRebarsInView(UIDocument uidoc)
        {
            Document doc = uidoc.Document;

            //get rebar tags
            ICollection<Element> fec1 = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_RebarTags).ToElements();
            List<ElementId> taggedRebarIds = new List<ElementId>();
            foreach (IndependentTag tag1 in fec1)
            {
                Element e = tag1.GetTaggedLocalElement();

                //Multirebar tags are attached to a dimension, and wee need to find what rebar the dimension is referencing.
                if (e is Dimension)
                {
                    Dimension dim1 = (Dimension)e;
                    ReferenceArray ref1 = dim1.References;
                    HashSet<ElementId> refIds = new HashSet<ElementId>();
                    foreach (Reference r in ref1)
                    {
                        refIds.Add(r.ElementId);
                    }
                    taggedRebarIds.AddRange(refIds.ToList());
                }
                //Add the rebars to a list
                else { taggedRebarIds.Add(tag1.GetTaggedLocalElement().Id); }
            }
            return taggedRebarIds;
        }
    }

    //Select Tagged Rebars External Command
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class SelectTaggedRebars : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            uidoc.Selection.SetElementIds(RebarHelper.TaggedRebarsInView(uidoc));
            return Result.Succeeded;
        }
    } //class


    //Select Un-Tagged Rebars External Command
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class SelectUntaggedRebars : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Here is the code:
            Application app = commandData.Application.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            ICollection<ElementId> fec2 = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_Rebar).ToElementIds();

            HashSet<ElementId> uniqueTaggedRebars = new HashSet<ElementId>(RebarHelper.TaggedRebarsInView(uidoc));
            HashSet<ElementId> uniqueAllRebars = new HashSet<ElementId>(fec2);
            ICollection<ElementId> untaggedRebars = new List<ElementId>(uniqueAllRebars.Except(uniqueTaggedRebars));
            uidoc.Selection.SetElementIds(untaggedRebars);

            return Result.Succeeded;
        }
    } //class

    //Set the attributes
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]

    //create a external command class
    public class TagAllRebarsInHost : IExternalCommand
    {

        private void SetWorkPlane(View view1, Document doc1)
        {
            Plane plane1 = Plane.CreateByNormalAndOrigin(view1.ViewDirection, view1.Origin);
            using (Transaction t1 = new Transaction(doc1, "Set new wokplane"))
            {
                t1.Start();
                SketchPlane sp1 = SketchPlane.Create(doc1, plane1);
                doc1.ActiveView.SketchPlane = sp1;
                t1.Commit();
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            Application app = commandData.Application.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            View view1 = doc.ActiveView;
            const double mmTofeet = 1.0 / 304.8;

            //Check if the view is a 3D view, and if it is locked:
            if (view1.ViewType == ViewType.ThreeD)
            {
                View3D view3D1 = (View3D)view1;
                if (!view3D1.IsLocked)
                {
                    TaskDialog.Show("Error", "3D-view must be locked to place tags");
                    return Result.Cancelled;
                }
            }

            //Check if the view has a usable sketchplane active, create and set one if not.
            if (view1.SketchPlane != null)
            {
                XYZ existingSPdir = view1.SketchPlane.GetPlane().Normal;
                if (Math.Abs(view1.ViewDirection.AngleTo(existingSPdir)) > 000.1)
                {
                    SetWorkPlane(view1, doc);
                }
            }
            else SetWorkPlane(view1, doc);

            //Get the rebar host: Use selected element if exactly one is selected, else ask user to pick a element that is host for rebar.
            Element host1 = null;
            XYZ topPt1 = new XYZ();
            try
            {
                ICollection<ElementId> selectedElementIds = uidoc.Selection.GetElementIds();
                if (selectedElementIds.Count == 1)
                {
                    host1 = doc.GetElement(selectedElementIds.First());
                }
                else host1 = doc.GetElement(uidoc.Selection.PickObject(ObjectType.Element, new RebarHostSelectionFilter(), "Pick a rebar host").ElementId);

                //ui: Pick a point where the top tag should be placed (host tag)
                topPt1 = uidoc.Selection.PickPoint("Pick a point to place tags");
            }

            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            /*
            //Select all rebars and get their hostId, pick the ones with hostId==host1.Id
            FilteredElementCollector fec1 = new FilteredElementCollector(doc, view1.Id).OfCategory(BuiltInCategory.OST_Rebar);
            List<Autodesk.Revit.DB.Structure.Rebar> rebarList = fec1.Cast<Autodesk.Revit.DB.Structure.Rebar>().Where(e => e.GetHostId() == host1.Id).ToList();
            */
            
            //Get rebars in host:
            IList<Autodesk.Revit.DB.Structure.Rebar> rebarList = RebarHostData.GetRebarHostData(host1).GetRebarsInHost();

            //Get View Scale and updirection from active view
            double scale1 = System.Convert.ToDouble(view1.get_Parameter(BuiltInParameter.VIEW_SCALE).AsInteger());
            XYZ viewDirUp1 = view1.UpDirection;
            double tagSize1 = 3.5 * mmTofeet;
            double spacing1 = tagSize1 + (2.0 * mmTofeet);
            XYZ deltaVec = viewDirUp1.Normalize().Multiply(spacing1 * scale1);



            //Calculate the spacing of the tags
            int numberOfTags = rebarList.Count();
            XYZ tempPt = topPt1;
            List<XYZ> ptList = new List<XYZ>();
            for (int i = 0; i < numberOfTags; i++)
            {
                tempPt = tempPt.Subtract(deltaVec);
                ptList.Add(tempPt);
            }


            using (Transaction t2 = new Transaction(doc, "Create tag"))
            {
                t2.Start();
#if RVT2017
                doc.Create.NewTag(view1, host1, false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, topPt1);
#else
                IndependentTag.Create(doc, view1.Id, new Reference(host1), false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, topPt1);
#endif
                for (int i = 0; i < numberOfTags; i++)
                {
#if RVT2017 
                    doc.Create.NewTag(view1, rebarList[i], false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, ptList[i]);
#else
                    IndependentTag.Create(doc, view1.Id, new Reference(rebarList[i]), false, TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, ptList[i]);
#endif
                }

                t2.Commit();
            }

            //The external command requres a return.
            return Result.Succeeded;
        }

    } //class

    //Cycle  tags External Command
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]
    public class CycleTagLeader : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            ElementId elementId1 = uidoc.Selection.GetElementIds().FirstOrDefault();
            if (elementId1 == null)
            {
                TaskDialog.Show("Warning", "Select a tag before running this command");
                return Result.Failed;
            }
            IndependentTag selectedTag = doc.GetElement(elementId1) as IndependentTag;
            if (selectedTag == null)
            {
                TaskDialog.Show("Warning", "Select a tag before running this command");
                return Result.Failed;
            }
            Transaction t1 = new Transaction(doc,"Tag Leader Cycle");
            t1.Start();
            if (selectedTag.HasLeader == true)
            {
                if (selectedTag.LeaderEndCondition == LeaderEndCondition.Free)
                {
                    selectedTag.LeaderEndCondition = LeaderEndCondition.Attached;
                }
                else
                {
                    selectedTag.HasLeader = false;
                }
            }
            else
            {
                selectedTag.HasLeader = true;
                selectedTag.LeaderEndCondition = LeaderEndCondition.Free;
            }
            t1.Commit();
            return Result.Succeeded;
        }
    } //class
} //namespace