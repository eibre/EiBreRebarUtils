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

    public class MoveFromInternalToShared : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Get Project Base Point (PBP) and it's parameters.
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_ProjectBasePoint);
            BasePoint element = (BasePoint)collector.ToElements().FirstOrDefault();
            double angle = element.get_Parameter(BuiltInParameter.BASEPOINT_ANGLETON_PARAM).AsDouble();
            double EW = element.get_Parameter(BuiltInParameter.BASEPOINT_EASTWEST_PARAM).AsDouble();
            double NS = element.get_Parameter(BuiltInParameter.BASEPOINT_NORTHSOUTH_PARAM).AsDouble();
            double elevation = element.get_Parameter(BuiltInParameter.BASEPOINT_ELEVATION_PARAM).AsDouble();
            // Get the coordinates of the Project Base Point (PBP) in case it's moved unclipped.
            XYZ PBP = element.get_BoundingBox(null).Max;

            // Calculating the distances between internal origo and shared origo. EW and NS is measured in the directions given by the angle to true north.
            double x = EW * Math.Cos(angle) - NS * Math.Sin(angle) - PBP.X;
            double y = NS * Math.Cos(angle) + EW * Math.Sin(angle) - PBP.Y;
            double z = elevation - PBP.Z;

            XYZ SharedOrigo = new XYZ(-x, -y, -z);
            Line axis = Line.CreateUnbound(SharedOrigo, new XYZ(0, 0, 1));

            Element selectedLink;
            try
            {
                selectedLink = doc.GetElement(uidoc.Selection.PickObject(ObjectType.Element, "Pick a link to move from Origin (internal) to Surveypoint."));
            }
            catch(Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }

            if (selectedLink == null) return Result.Failed;

            using (Transaction t1 = new Transaction(doc, "Move link from internal origin to survey point"))
            {
                t1.Start();
                    selectedLink.Location.Move(SharedOrigo);
                    selectedLink.Location.Rotate(axis, angle);
                t1.Commit();
            }
        
            return Result.Succeeded;
        }

    } //Class
} //namespace