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

    public class CopyRebar : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            bool SourceIsWallFoundation = false;

            Reference ref1 = uidoc.Selection.PickObject(ObjectType.Element, new RebarHostSelectionFilter(), "Pick a rebar host to copy from");
            IList<Reference> ref2List = uidoc.Selection.PickObjects(ObjectType.Element, new RebarHostSelectionFilter(), "Pick a rebar host to copy to");


            
            using (Transaction t1 = new Transaction(doc, "Copy rebar"))
            {
                t1.Start();
                foreach (Reference ref2 in ref2List)
                {
                    Element sourceHost = doc.GetElement(ref1.ElementId);
                    Element targetHost = doc.GetElement(ref2.ElementId);
                    ICollection<ElementId> elementIdToBeDeleted = new List<ElementId>();
                    WallFoundation sourceWallFoundation = null;
                    WallFoundation targetWallFoundation = null;
                    //Workaround for wall foundations: They have no location lines, but are completely dependent on their walls. So by copiing the host wall, both the wall foundation and the rebars follows along
                    if (sourceHost is WallFoundation && targetHost is WallFoundation)
                    {
                        SourceIsWallFoundation = true;
                        sourceWallFoundation = (WallFoundation)sourceHost;
                        sourceHost = doc.GetElement(sourceWallFoundation.WallId);
                        targetWallFoundation = (WallFoundation)targetHost;
                        targetHost = doc.GetElement(targetWallFoundation.WallId);
                    }
                    //STEP 1: Copy element to random location
                    //5000 is a random chosen number, surprisingly it matters if its copied in Y or Z direction:
                    ICollection<ElementId> copiedElements = ElementTransformUtils.CopyElement(doc, sourceHost.Id, new XYZ(0, 5000, 0));
                    Element copiedElement = doc.GetElement(copiedElements.FirstOrDefault());
                    elementIdToBeDeleted.Add(copiedElement.Id);

                    //STEP 2: Relocate copy to match the target 
                    if (copiedElement.Location is LocationCurve && targetHost.Location is LocationCurve)
                    {
                        LocationCurve locationTo = targetHost.Location as LocationCurve;
                        LocationCurve locationCopied = copiedElement.Location as LocationCurve;
                        locationCopied.Curve = locationTo.Curve;
                    }
                    else if (copiedElement.Location is LocationPoint && targetHost.Location is LocationPoint)
                    {
                        LocationPoint locationTo = targetHost.Location as LocationPoint;
                        LocationPoint locationCopied = copiedElement.Location as LocationPoint;
                        locationCopied.Point = locationTo.Point;
                        if (locationCopied.Rotation != locationTo.Rotation)
                        {
                            Transform trans1 = Transform.CreateTranslation(new XYZ(0, 0, 1));
                            XYZ origin1 = locationTo.Point;
                            XYZ endAxis = trans1.OfPoint(origin1);
                            Line axis1 = Line.CreateBound(origin1, endAxis);
                            locationCopied.Rotate(axis1, locationTo.Rotation);
                        }
                    }
                    else
                    {
                        TaskDialog.Show("Warning", "Pick two Rebar Hosts with the same location type. Only elements with locatoion lines and location points are supported. Elements such as beams, walls and slanted columns do typically have location lines. Floors and slabs are not supported.");
                        t1.RollBack();
                        return Result.Cancelled;
                    }


                    //STEP 3: Change the type of the copied element to match the target element
                    //TODO: Change types for wall foundations and change heights/offsets for walls
                    try
                    {
                        if (targetHost.GetTypeId() != copiedElement.GetTypeId())
                        {
                            copiedElement.ChangeTypeId(targetHost.GetTypeId());
                        }

                        if (targetHost is Wall && sourceHost is Wall)
                        {
                            copiedElement.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).Set(targetHost.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET).AsDouble());
                            copiedElement.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).Set(targetHost.get_Parameter(BuiltInParameter.WALL_TOP_OFFSET).AsDouble());
                        }
                    }
                    catch { }

                    doc.Regenerate();

                    //Step 3: Change rebar host from copied element to target element
                    IList<Rebar> copiedRebars = new List<Rebar>();

                    if (SourceIsWallFoundation)
                    {
                        //Need to find the foundation that was created by copying the wall
                        WallFoundation wallFoundation = new FilteredElementCollector(doc)
                            .OfClass(typeof(WallFoundation))
                            .WhereElementIsNotElementType().Cast<WallFoundation>()
                            .FirstOrDefault(q => q.WallId == copiedElement.Id);
                        elementIdToBeDeleted.Add(wallFoundation.Id);
                        copiedRebars = RebarHostData.GetRebarHostData(wallFoundation).GetRebarsInHost();
                        if (targetWallFoundation != null)
                        {
                            foreach (Rebar r in copiedRebars)
                            {
                                r.SetHostId(doc, targetWallFoundation.Id);
                            }
                        }

                    }
                    else
                    {
                        copiedRebars = RebarHostData.GetRebarHostData(copiedElement).GetRebarsInHost();
                        foreach (Rebar r in copiedRebars)
                        {
                            r.SetHostId(doc, targetHost.Id);
                        }
                    }

                    //Step 4: Delete the copied element
                    doc.Delete(elementIdToBeDeleted);

                }

                t1.Commit();
            }
            return Result.Succeeded;
        }
    } //Class
    public class RebarHostSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            if (RebarHostData.GetRebarHostData(element) != null)
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
    } //class
} //namespace