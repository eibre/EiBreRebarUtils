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

    public class ScheduleMarkUpdate : IExternalCommand
    {
        public static void ScheduleMarkUpdater(Document doc)
        {
            IList<Element> elements1 = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rebar).WhereElementIsNotElementType().ToElements();

            foreach (var element in elements1)
            {
                if (null != element.get_Parameter(BuiltInParameter.REBAR_ELEM_SCHEDULE_MARK))
                {
                    Parameter scheduleMark = element.get_Parameter(BuiltInParameter.REBAR_ELEM_SCHEDULE_MARK);

                    string rebarNumber1 = element.get_Parameter(BuiltInParameter.REBAR_NUMBER).AsString();
                    string partition1 = element.get_Parameter(BuiltInParameter.NUMBER_PARTITION_PARAM).AsString();
                    string combinedParameter = "";
                    if (partition1.Contains("LM") || partition1.Contains("RM"))
                    {
                        combinedParameter = partition1;
                    }
                    else
                    {
                        combinedParameter = partition1 + rebarNumber1;
                    }

                    if (scheduleMark.AsString() != combinedParameter)
                    {
                        scheduleMark.Set(combinedParameter);
                    }
                }
            }
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            using (Transaction t1 = new Transaction(doc, "Update parameter: Schedule Mark"))
            {
                t1.Start();
                ScheduleMarkUpdater(doc);
                doc.Regenerate();
                t1.Commit();
            }
            return Result.Succeeded;
        }
    } //class

    //Set the attributes
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]

    public class SumGeometry : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            //Promt to select elements or use the seleceted elements
            List<Element> elements1 = new List<Element>();
            ICollection<ElementId> selectedElementIds = uidoc.Selection.GetElementIds();
            if (!selectedElementIds.Any())
            {
                IList<Reference> ref1 = uidoc.Selection.PickObjects(ObjectType.Element, "Select elements and click finish");
                foreach (var r in ref1)
                {
                    elements1.Add(doc.GetElement(r.ElementId));
                }
            }
            else
            {
                foreach (var id in selectedElementIds)
                {
                    elements1.Add(doc.GetElement(id));
                }
            }

            //Get the parameters:
            double sumLength = 0;
            double sumArea = 0;
            double sumVolume = 0;
            int count = elements1.Count;


            foreach (Element e in elements1)
            {
                Parameter length = e.GetParameters("Length").FirstOrDefault();
                Parameter area = e.GetParameters("Area").FirstOrDefault();
                Parameter volume = e.GetParameters("Volume").FirstOrDefault();

                if (length != null)
                {
                    sumLength += UnitUtils.ConvertFromInternalUnits(length.AsDouble(), length.GetUnitTypeId());
                }
                else sumLength = 0;
                if (area != null)
                {
                    sumArea += UnitUtils.ConvertFromInternalUnits(area.AsDouble(), area.GetUnitTypeId());
                }
                else sumArea = 0;
                if (volume != null)
                {
                    sumVolume += UnitUtils.ConvertFromInternalUnits(volume.AsDouble(), volume.GetUnitTypeId());
                }
                else sumVolume = 0;

            }
            TaskDialog.Show("Sum of geometry", "Sums of " + count.ToString() + " elements: \nSum of length: " + sumLength.ToString() + "\nSum of Area: " + sumArea.ToString() + "\nSum of Volume: " + sumVolume.ToString());
        
            return Result.Succeeded;
        }
    } //class

} //namespace