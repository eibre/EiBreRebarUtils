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

    public class SwitchJoinOrder : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            ICollection<ElementId> elementIds = new List<ElementId>();

            if (uidoc.Selection.GetElementIds().Count>0)
            {
                elementIds = uidoc.Selection.GetElementIds();
            }
            else
            {
                try
                {
                    elementIds = uidoc.Selection.PickObjects(ObjectType.Element, new FloorSelectFilter(), "Pick floors").Select(q => q.ElementId).ToList();
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    message = "Command cancelled, Click finish in top left corner to complete the command";
                    return Result.Cancelled;
                }
            }


            using (Transaction t1 = new Transaction(doc, "Switch join order"))
            {
                t1.Start();
                foreach(ElementId floorId in elementIds)
                {
                    Floor floor = doc.GetElement(floorId) as Floor;
                    BoundingBoxXYZ boundingBox = floor.get_BoundingBox(doc.ActiveView);
                    BoundingBoxIntersectsFilter bbFilter = new BoundingBoxIntersectsFilter(new Outline(boundingBox.Min, boundingBox.Max));
                    FilteredElementCollector columnCollector = new FilteredElementCollector(doc, doc.ActiveView.Id).OfCategory(BuiltInCategory.OST_StructuralColumns).WherePasses(bbFilter);

                    foreach (Element column in columnCollector)
                    {
                        if (JoinGeometryUtils.AreElementsJoined(doc, floor, column))
                        {
                            if(JoinGeometryUtils.IsCuttingElementInJoin(doc, floor, column)) {
                                JoinGeometryUtils.SwitchJoinOrder(doc, floor, column);
                            }
                        }
                    }
                }
                t1.Commit();
            }
            return Result.Succeeded;
        }
    } //Class
} //namespace