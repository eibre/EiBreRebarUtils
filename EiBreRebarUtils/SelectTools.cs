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
            if (element.Category.Id.IntegerValue == (int) BuiltInCategory.OST_Rebar)
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

    public class DimensionSelectFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            if (element.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Dimensions)
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

    public class TagSelectFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            if (element is IndependentTag)
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

    public class WallOrBeamSelectFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            if (element is Wall || element.Category.Id.IntegerValue == (int) BuiltInCategory.OST_StructuralFraming)
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
    public class FloorSelectFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            if (element is Floor)
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

    //Set the attributes
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]

    public class SelectBottomLayer : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();
            List<Element> hostElements = new List<Element>();
            if (selectedIds.Any())
            {
                foreach(ElementId id in selectedIds)
                {
                    Element e = doc.GetElement(id);
                    if (RebarHostData.IsValidHost(e))
                    {
                        hostElements.Add(e);
                    }
                }
            }
            else
            {
                try
                {
                    Reference pickedRef = uidoc.Selection.PickObject(ObjectType.Element, new RebarHostSelectionFilter(), "Pick a rebar host to selct rebar in bottom, TAB to cycle, ESC to cancel");
                    if(pickedRef == null)
                    {
                        message = "Nothing was selected";
                        return Result.Failed;
                    }
                    else
                    {
                        hostElements.Add(doc.GetElement(pickedRef));
                    }
                }
                catch(Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Cancelled;
                }
            }
            ICollection<ElementId> idsToSelect = new List<ElementId>();
            foreach (Element host in hostElements)
            {
                ICollection<ElementId> rebarsInBottomLayer = GetElementIdsInLayer(doc, host, true);
                idsToSelect = idsToSelect.Concat(rebarsInBottomLayer).ToList();
            }
            uidoc.Selection.SetElementIds(idsToSelect);

            return Result.Succeeded;
        }

        internal static ICollection<ElementId> GetElementIdsInLayer(Document doc, Element host, bool bottomLayer)
        {
            RebarHostData hostData = RebarHostData.GetRebarHostData(host);
            ICollection<ElementId> rebarsInHost = hostData.GetRebarsInHost().Select(r => r.Id).ToList();
            BoundingBoxXYZ box = host.get_BoundingBox(null);
            double midZ = 0.5 * (box.Max.Z + box.Min.Z);
            Outline outline;
            if (bottomLayer)
            {
                outline = new Outline(box.Min, new XYZ(box.Max.X, box.Max.Y, midZ));
            }
            else
            {
                outline = new Outline(new XYZ(box.Min.X, box.Min.Y, midZ), box.Max);
            }

            BoundingBoxIsInsideFilter filter = new BoundingBoxIsInsideFilter(outline);
            FilteredElementCollector collector = new FilteredElementCollector(doc, rebarsInHost);
            return collector.WherePasses(filter).ToElementIds();
            
        }
    } //class

    //Set the attributes
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]

    public class SelectTopLayer : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();
            List<Element> hostElements = new List<Element>();
            if (selectedIds.Any())
            {
                foreach (ElementId id in selectedIds)
                {
                    Element e = doc.GetElement(id);
                    if (RebarHostData.IsValidHost(e))
                    {
                        hostElements.Add(e);
                    }
                }
            }
            else
            {
                try
                {
                    Reference pickedRef = uidoc.Selection.PickObject(ObjectType.Element, new RebarHostSelectionFilter(), "Pick a rebar host to selct rebar in bottom, TAB to cycle, ESC to cancel");
                    if (pickedRef == null)
                    {
                        message = "Nothing was selected";
                        return Result.Failed;
                    }
                    else
                    {
                        hostElements.Add(doc.GetElement(pickedRef));
                    }
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Cancelled;
                }
            }
            ICollection<ElementId> idsToSelect = new List<ElementId>();
            foreach (Element host in hostElements)
            {
                ICollection<ElementId> rebarsInBottomLayer = SelectBottomLayer.GetElementIdsInLayer(doc, host, false);
                idsToSelect = idsToSelect.Concat(rebarsInBottomLayer).ToList();
            }
            uidoc.Selection.SetElementIds(idsToSelect);

            return Result.Succeeded;
        }
    } //class

    } //namespace