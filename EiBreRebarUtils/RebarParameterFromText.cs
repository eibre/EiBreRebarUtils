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

            string defaultText = "ø12c200-P UK";
            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();
            List<Rebar> selectedRebars = new List<Rebar>();
            foreach(ElementId id in selectedIds)
            {
                Element e = doc.GetElement(id);
                if (e is Rebar)
                {
                    Rebar r = e as Rebar;
                    selectedRebars.Add(r);
                }
                else if (e is IndependentTag)
                {
                    IndependentTag tag = e as IndependentTag;
                    if (tag.GetTaggedLocalElement() is Rebar)
                    {
                        selectedRebars.Add(tag.GetTaggedLocalElement() as Rebar);
                    }
                }
            }

            if (selectedRebars.Count < 1)
            {
                Element pickedElement = null;
                try
                {
                    pickedElement = doc.GetElement(uidoc.Selection.PickObject(ObjectType.Element, "pick rebar"));
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    return Result.Cancelled;
                }
                if (pickedElement is Rebar)
                {
                    selectedRebars.Add(pickedElement as Rebar);
                }
            }
            else if(selectedRebars.Count == 1)
            {
                Rebar rebar = selectedRebars.First();
                defaultText = GetDiameter(rebar) + GetSpacing(rebar) + GetPartition(rebar) + GetComments(rebar);
            }

            string input = "";
            WindowTextInput dialog = new WindowTextInput(defaultText);
            dialog.ShowDialog();
            if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
            {
                input = dialog.textInput.Text;
            }   
            else
            {
                return Result.Cancelled;
            }

            Units units = doc.GetUnits();

            //DIAMETER
            //Diameter is a type property of rebar type. In Norconsult the diameter is included in type name.
            FilteredElementCollector fec = new FilteredElementCollector(doc).OfClass(typeof(RebarBarType));
            string diameter = Regex.Match(input, @"ø(\d*)").Groups[1].Value;
            RebarBarType type = fec.Cast<RebarBarType>().First(q => q.Name.Contains(diameter));
            
            //SPACING
            string spacingString = Regex.Match(input, @"c(\d*)").Groups[1].Value;

            //PARTITION
            string partitionString = Regex.Match(input, @"-(\w*)").Groups[1].Value;

            //COMMENTS
            string comments = Regex.Match(input, @"\s(.*)").Groups[1].Value;

            foreach(Rebar rebar in selectedRebars)
            {
                Transaction t1 = new Transaction(doc, "Set parameters");
                t1.Start();
                rebar.ChangeTypeId(type.Id);
        
                SetSpacing(rebar, spacingString);

                rebar.get_Parameter(BuiltInParameter.NUMBER_PARTITION_PARAM).Set(partitionString);
                rebar.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).Set(comments);
                t1.Commit();
            }

            return Result.Succeeded;
        }

        private static string GetDiameter(Rebar rebar)
        {
            Parameter diameterParam = rebar.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER);
            DisplayUnitType unitType = diameterParam.DisplayUnitType;
            double diameter = diameterParam.AsDouble();
            diameter = UnitUtils.ConvertFromInternalUnits(diameter, unitType);
            return "ø" + diameter.ToString();
        }

        private static string GetSpacing(Rebar rebar)
        {
            Parameter layoutParam = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_LAYOUT_RULE);
            if (layoutParam.AsInteger() == 0)
            {
                return "";
            }
            Parameter spacingParam = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_BAR_SPACING);
            DisplayUnitType unitType = spacingParam.DisplayUnitType;
            double spacing = spacingParam.AsDouble();
             spacing =  UnitUtils.ConvertFromInternalUnits(spacing, unitType);
            return "c" + spacing.ToString("0");
        }

        private static string GetPartition(Rebar rebar)
        {
            string partition = rebar.get_Parameter(BuiltInParameter.NUMBER_PARTITION_PARAM).AsString();
            if (partition == "")
            {
                return "";
            }
            else
            {
                return "-" + partition;
            }
        }

        private static string GetComments(Rebar rebar)
        {
            string comments = rebar.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS).AsString();
            if (comments == "")
            {
                return "";
            }
            else
            {
                return " " + comments;
            }
        }

        private static void SetSpacing(Rebar rebar, string spacingString)
        {
            if(spacingString == "")
            {
                //Set layout to single
                rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_LAYOUT_RULE).Set(0);
            }
            else
            {
                //Set layout to maximum spacing:
                rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_LAYOUT_RULE).Set(2);

                Parameter spacingParam = rebar.get_Parameter(BuiltInParameter.REBAR_ELEM_BAR_SPACING);
                DisplayUnitType displayUnitSpacing = spacingParam.DisplayUnitType;
                double spacing = double.Parse(spacingString);
                spacing = UnitUtils.ConvertToInternalUnits(spacing, displayUnitSpacing);
                spacingParam.Set(spacing);
            }
        }
    } // class
} //namespace