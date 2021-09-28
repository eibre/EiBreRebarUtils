using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Application = Autodesk.Revit.ApplicationServices.Application;
using System.IO;
using System.Diagnostics;

namespace NO.RebarUtils
{
    //Set the attributes
    [TransactionAttribute(TransactionMode.Manual)]
    [RegenerationAttribute(RegenerationOption.Manual)]

    public class PasteImageFromClipboard : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            string filename = "pastedImage" + "_" + DateTime.Now.ToString("yyyy-MM-dd HH.mm") + ".jpeg";
            string path = "";
            if (doc.IsModelInCloud || doc.IsDetached || doc.PathName == null)
            {
                path = Path.Combine(Path.GetTempPath(), filename);
            }
            else if(doc.IsWorkshared) {
                path = Path.Combine(Path.GetDirectoryName(doc.GetWorksharingCentralModelPath().CentralServerPath), "PastedImages", filename);
            }
            else
            {
                path = Path.Combine(Path.GetDirectoryName(doc.PathName), "PastedImages", filename);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            if (Clipboard.ContainsImage())
            {
                Image image = (Image)Clipboard.GetDataObject().GetData(DataFormats.Bitmap);
                Debug.Print(path);
                image.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            else
            {
                message = "No image on clipoboard";
                return Result.Failed;
            }

            XYZ imageLocation = new XYZ();

            try
            {
                imageLocation = uidoc.Selection.PickPoint("Pick insertion point (midpoint)");
            }
            catch(Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                message = "Command cancelled";
                return Result.Cancelled;
            }


            using (Transaction t1 = new Transaction(doc, "paste image"))
            {
                t1.Start();
                ImageTypeOptions opt = new ImageTypeOptions(path, false, ImageTypeSource.Import);
                ImagePlacementOptions pOpt = new ImagePlacementOptions();
                pOpt.Location = imageLocation;
                pOpt.PlacementPoint = BoxPlacement.Center;
                ImageType imageType = ImageType.Create(doc, opt);
                ImageInstance.Create(doc, doc.ActiveView, imageType.Id, pOpt);
                t1.Commit();
            }


            return Result.Succeeded;
        }
    } //Class
} //namespace