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

namespace NO.RebarUtils
{
    //Set the attributes
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]

    public class RenumberRebar : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            NumberingSchema schema = NumberingSchema.GetNumberingSchema(doc, NumberingSchemaTypes.StructuralNumberingSchemas.Rebar);
            var partitions = schema.GetNumberingSequences();
            if (!partitions.Any())
            {
                message = "No partitions in the document";
                return Result.Failed;
            }
            Dictionary<string, string[]> rebarNumbers = new Dictionary<string, string[]>(); 
            foreach(string p in partitions)
            {
                rebarNumbers[p] = GetRebarNumbers(doc, p);
            }
            string[] sortedPartitions = partitions.OrderBy(q => q).ToArray();
            using (FormRenumber form1 = new FormRenumber(doc, sortedPartitions, rebarNumbers))
            {
                form1.ShowDialog();
                if (form1.DialogResult == System.Windows.Forms.DialogResult.Cancel) return Result.Cancelled;

                else
                {
                    string partition = form1.partition;
                    int fromNumber = form1.fromNumber;
                    int toNumber = form1.toNumber;
                    Transaction t = new Transaction(doc, "Change rebar number");
                    t.Start();
                    IList<ElementId> ids = schema.ChangeNumber(partition, fromNumber, toNumber);
                    t.Commit();
                    message = "Changed numbers on rebars with id" + string.Join(", ",ids.Select(i=>i.ToString()));
                    uidoc.Selection.SetElementIds(ids);
                }
            }
            return Result.Succeeded;
        }

        public static string[] GetRebarNumbers(Document doc, string partition)
        {
            NumberingSchema schema = NumberingSchema.GetNumberingSchema(doc, NumberingSchemaTypes.StructuralNumberingSchemas.Rebar);
            IList<IntegerRange> ranges = schema.GetNumbers(partition);
            List<int[]> list = new List<int[]>();
            foreach (var range in ranges)
            {
                list.Add(Enumerable.Range(range.Low, range.High - range.Low + 1).ToArray());
            }
            int[] numbers = list.SelectMany(i => i).ToArray();
            return numbers.Select(i => i.ToString()).ToArray();
        }
    } //class


    //Set the attributes
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]

    public class CopyRebarNumberFromScheduleMark : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            Document doc = commandData.Application.ActiveUIDocument.Document;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;

            NumberingSchema schema = NumberingSchema.GetNumberingSchema(doc, NumberingSchemaTypes.StructuralNumberingSchemas.Rebar);
            IList<string> partitions = schema.GetNumberingSequences();
            if (!partitions.Any())
            {
                message = "No partitions in the document";
                return Result.Failed;
            }
             
            WindowSelectPartition dialog = new WindowSelectPartition(partitions);
            dialog.ShowDialog();
            if (dialog.DialogResult.HasValue && dialog.DialogResult.Value)
            {
                string selectedPartition = dialog.comboPartition.SelectedValue.ToString();
            }   
            else
            {
                return Result.Cancelled;
            }

            //TODO: Renumber

            return Result.Succeeded;
        }
    } // class
} //namespace
