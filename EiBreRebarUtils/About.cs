﻿using System;
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

    public class About : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Application app = commandData.Application.Application;
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string gitHubUrl = @"https://github.com/eibre/EiBreRebarUtils";
            string mainInstruction = $"Version: {version}\n\nReport bugs and requests at {gitHubUrl}\n\nLicense: MIT";

            TaskDialog.Show("About", mainInstruction);

            return Result.Succeeded;
        }
    } //class
} //namespace