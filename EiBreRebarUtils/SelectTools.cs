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

            if (!doc.IsWorkshared) return Result.Failed;

            List<WorksetId> worksetIds = uidoc.Selection.GetElementIds().Select(id => doc.GetElement(id)).Select(e => e.WorksetId).Distinct().ToList();
            ICollection<ElementId> elementIdsWithSameWorkset = new List<ElementId>();

            foreach (var worksetsId in worksetIds)
            {
                ElementWorksetFilter elementWorksetFilter = new ElementWorksetFilter(worksetsId);
                var ids = new FilteredElementCollector(doc, doc.ActiveView.Id).WherePasses(elementWorksetFilter).ToElementIds();
                foreach (var id in ids)
                {
                    elementIdsWithSameWorkset.Add(id);
                }
            }
            if (elementIdsWithSameWorkset.Any())
            {
                uidoc.Selection.SetElementIds(elementIdsWithSameWorkset);
                return Result.Succeeded;
            }
            else
            {
                return Result.Succeeded;
            }
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

            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();

            if (selectedIds.Any())
            {
                List<ElementId> selectedCategories = selectedIds.ToList().Select(x => doc.GetElement(x)).Select(x => x.Category.Id).Distinct().ToList();
                ElementMulticategoryFilter multiCategoryFilter = new ElementMulticategoryFilter(selectedCategories);
                ICollection<ElementId> idsToSelect = new FilteredElementCollector(doc, doc.ActiveView.Id).WherePasses(multiCategoryFilter).ToElementIds();
                uidoc.Selection.SetElementIds(idsToSelect);
            }
            else
            {
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    } //class
} //namespace