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

    public class ActiveViewToOpenSheet : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            ElementId sheetId = null;

            if (doc.ActiveView is ViewSheet)
            {
                message = "Active view can not be a sheet!";
                return Result.Failed;
            }

            List<ElementId> sheedIds = new List<ElementId>();
            foreach (UIView uiview in uidoc.GetOpenUIViews())
            {
                if (doc.GetElement(uiview.ViewId) is ViewSheet)
                {
                    sheedIds.Add(uiview.ViewId);
                }
            }

            if (sheedIds.Count == 0)
            {
                message = "No sheets are open, or a viewport is activated.";
                return Result.Failed;
            }

            int selectedSheet = 0;
            if (sheedIds.Count > 1)
            {
                List<string> sheetNames = new List<string>();
                foreach (ElementId id in sheedIds)
                {
                    Element e = doc.GetElement(id);
                    sheetNames.Add(e.Name);
                }
                WindowSelectSheet dialog = new WindowSelectSheet(sheetNames);
                dialog.ShowDialog();
                if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
                {
                    selectedSheet = dialog.comboSheet.SelectedIndex;
                }
                else
                {
                    return Result.Cancelled;
                }

            }
            sheetId = sheedIds[selectedSheet];


            if (Viewport.CanAddViewToSheet(doc, sheetId, doc.ActiveView.Id))
            {
                using (Transaction t1 = new Transaction(doc, "Add active view to opened sheet"))
                {
                    t1.Start();
                    Viewport.Create(doc, sheetId, doc.ActiveView.Id, new XYZ(0, 0, 0));
                    t1.Commit();
                }
            }
            else
            {
                message = "Active view can not be added to sheet. Maybe it is on a sheet already?";
                return Result.Failed;
            }

            uidoc.RequestViewChange(doc.GetElement(sheetId) as View);
  

            return Result.Succeeded;
        }
    } // class
} //namespace
