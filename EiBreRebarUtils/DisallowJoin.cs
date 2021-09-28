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

    public class DisallowJoin : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            IList<Reference> ref2List = new List<Reference>();

            try
            {
                ref2List = uidoc.Selection.PickObjects(ObjectType.Element, new WallOrBeamSelectFilter(), "Pick walls and/or beams to disallow join in both ends");
            }
            catch(Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                message = "Command cancelled, Click finish in top left corner to complete the command";
                return Result.Cancelled;
            }

            using (Transaction t1 = new Transaction(doc, "Dissalow Join"))
            {
                t1.Start();
                foreach (Reference r in ref2List)
                {
                    Element e = doc.GetElement(r);
                    if (e is Wall)
                    {
                        Wall wall = e as Wall;
                        WallUtils.DisallowWallJoinAtEnd(wall, 0);
                        WallUtils.DisallowWallJoinAtEnd(wall, 1);
                    }
                    else if (e.Category.Id.IntegerValue == (int) BuiltInCategory.OST_StructuralFraming) {
                        FamilyInstance familyInstance = e as FamilyInstance;
                        StructuralFramingUtils.DisallowJoinAtEnd(familyInstance, 0);
                        StructuralFramingUtils.DisallowJoinAtEnd(familyInstance, 1);
                    }
                }

                t1.Commit();
            }
            return Result.Succeeded;
        }
    } //Class
} //namespace