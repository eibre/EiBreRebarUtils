using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;

namespace EiBreRebarUtils
{
    //Set the attributes
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]

    public class RemoveDimensionValue : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            ICollection<ElementId> selectedElements = uidoc.Selection.GetElementIds();
            if (!selectedElements.Any())
            {
                try
                {
                    var pickedElements = uidoc.Selection.PickObjects(ObjectType.Element, "Pick dimensions, and hit Finish");
                    foreach (var ref1 in pickedElements) { selectedElements.Add(ref1.ElementId); }
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Cancelled;
                }
            }

            using (Transaction t = new Transaction(doc, "Override dimensions"))
            {
                t.Start();
                foreach (ElementId eId in selectedElements)
                {
                    var element = doc.GetElement(eId) as Dimension;
                    if (element.NumberOfSegments > 1)
                    {
                        foreach (DimensionSegment segment in element.Segments)
                        {
                            segment.ValueOverride = '\u0300'.ToString();
                        }
                    }
                    else { element.ValueOverride = '\u0300'.ToString(); }
                }
                t.Commit();
            }
            return Result.Succeeded;
        }
    }
} //namespace
