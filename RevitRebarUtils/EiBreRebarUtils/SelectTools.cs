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

    public class RebarSelectFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            if (element.Category.Name == "Structural Rebar")
            {
                return true;
            }
            return false;
        }

        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }

    }

    //Set the attributes
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]

    public class SelectRebar : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            IList<Element> elementList = uidoc.Selection.PickElementsByRectangle(new RebarSelectFilter(),"Select elements");
            IList<ElementId> idlist = new List<ElementId>();
            foreach(Element e in elementList)
            {
                idlist.Add(e.Id);
            }

            uidoc.Selection.SetElementIds(idlist);
            return Result.Succeeded;
        }
    } //class
    //Set the attributes
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]

    public class SelectSameWorkset : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            //get the workset of the selected element
            ElementId selectedElementId = uidoc.Selection.GetElementIds().FirstOrDefault();

            if (selectedElementId == null) return Result.Failed;

            Element selectedElement = doc.GetElement(selectedElementId);
            Workset workset = new FilteredWorksetCollector(doc).Where(w => w.Id == selectedElement.WorksetId).FirstOrDefault();


            // filter all elements that belong to the given workset
            FilteredElementCollector elementCollector = new FilteredElementCollector(doc, doc.ActiveView.Id);
            ElementWorksetFilter elementWorksetFilter = new ElementWorksetFilter(workset.Id);
            ICollection<ElementId> worksetElemsfounds = elementCollector.WherePasses(elementWorksetFilter).ToElementIds();

            //Select all elements on the workset
            uidoc.Selection.SetElementIds(worksetElemsfounds);
            return Result.Succeeded;
        }
    } //class

    //Set the attributes
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]

    public class SelectSameCategory : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            //get the workset of the selected element
            ElementId selectedElementId = uidoc.Selection.GetElementIds().FirstOrDefault();

            if (selectedElementId == null) return Result.Failed;

            Element selectedElement = doc.GetElement(selectedElementId);

            FilteredElementCollector elementCollector = new FilteredElementCollector(doc, doc.ActiveView.Id);
            BuiltInCategory myCatEnum = (BuiltInCategory)Enum.Parse(typeof(BuiltInCategory), selectedElement.Category.Id.ToString());
            var elemsfound = elementCollector.OfCategory(myCatEnum).ToElementIds();

            uidoc.Selection.SetElementIds(elemsfound);
            return Result.Succeeded;
        }
    } //class
} //namespace