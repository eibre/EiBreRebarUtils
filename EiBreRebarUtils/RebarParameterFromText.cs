using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

    public class RebarParameterFromText : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            WindowTextInput dialog = new WindowTextInput();
            dialog.ShowDialog();
            string input = "";
            if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
            {
                input = dialog.textInput.Text;
            }   
            else
            {
                return Result.Cancelled;
            }

            ICollection<ElementId> selectedElementIds = uidoc.Selection.GetElementIds();

            List<Rebar> selectedRebars = selectedElementIds.Select(id => doc.GetElement(id)).Cast<Rebar>().ToList();
            if(selectedRebars.Count < 1)
            {
                selectedRebars.Add(doc.GetElement(uidoc.Selection.PickObject(ObjectType.Element, "pick rebar")) as Rebar);
           
            }

            //string input = "ø12c200-P UK";

            Units units = doc.GetUnits();

            //DIAMETER
            //Diameter is a type property of rebar type. Match name?
            FilteredElementCollector fec = new FilteredElementCollector(doc).OfClass(typeof(RebarBarType));
            string diameter = Regex.Match(input, @"ø(\d*)").Groups[1].Value;
            RebarBarType type = fec.Cast<RebarBarType>().First(q => q.Name.Contains(diameter));


            //SPACING
            string spacingString = Regex.Match(input, @"c(\d*)").Groups[1].Value;
            double spacing = double.Parse(spacingString);
            DisplayUnitType displayUnitSpacing = units.GetFormatOptions(UnitType.UT_Reinforcement_Spacing).DisplayUnits;
            spacing = UnitUtils.ConvertToInternalUnits(spacing, displayUnitSpacing);

            //PARTITION
            string partitionString = Regex.Match(input, @"-(\w*)").Groups[1].Value;

            //COMMENTS
            string comments = Regex.Match(input, @"\s(.*)").Groups[1].Value;

            foreach(Rebar rebar in selectedRebars)
            {
                Transaction t1 = new Transaction(doc, "Set parameters");
                t1.Start();
                rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_LAYOUT_RULE).Set(2);
                rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_BAR_SPACING).Set(spacing);
                rebar.ChangeTypeId(type.Id);
                rebar.get_Parameter(BuiltInParameter.NUMBER_PARTITION_PARAM).Set(partitionString);
                rebar.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(comments);
                t1.Commit();
            }


            return Result.Succeeded;
        }
    } // class
} //namespace
