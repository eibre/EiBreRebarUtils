using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Drawing;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;

namespace EiBreRebarUtils
{
    public class CsAddPanel : IExternalApplication
    {
        // Both OnStartup and OnShutdown must be implemented as public method
        public Result OnStartup(UIControlledApplication application)
        {
            // Registrere event:
            //application.ControlledApplication.FileExporting += new EventHandler<FileExportingEventArgs>(AskForParameterUpdates);

            // Add a new ribbon panel
            RibbonPanel ribbonPanel = application.CreateRibbonPanel("EiBre Rebar Utils");
            
            // buttons: Tag all, select (un)tagged, cycle tag,  Visibility, Selection, Other

            // Create a push button to trigger the tag all rebars command.
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            PushButtonData buttonData = new PushButtonData("cmdTagAllRebarsInHost",
               "Tag rebars \nin host", thisAssemblyPath, "EiBreRebarUtils.TagAllRebarsInHost");
            
            PushButton pushButton = ribbonPanel.AddItem(buttonData) as PushButton;
            pushButton.ToolTip = "Tag all rebars in a host, the bars must be visible in active view";
            pushButton.LargeImage = BitmapToImageSource(Properties.Resources.TagAllRebarsInHost);


            //Select tagged/untagged
            //--------------------------------------
            //Create pushbutton data to select all tagged rebars in a view
            PushButtonData pushSelectedTagData = new PushButtonData("cmdSelectTaggedRebar", "Select \ntagged rebars", thisAssemblyPath, "EiBreRebarUtils.SelectTaggedRebars");
            pushSelectedTagData.ToolTip = "Selects all tagged Rebar elements in view";
            pushSelectedTagData.LargeImage = BitmapToImageSource(Properties.Resources.selectTagged);


            //Create pushbutton data to select all tagged rebars in a view
            PushButtonData pushUnselectedTagData = new PushButtonData("cmdSelectUntaggedRebar", "Select un-\ntagged rebars", thisAssemblyPath, "EiBreRebarUtils.SelectUntaggedRebars");
            pushUnselectedTagData.ToolTip = "Selects all un-tagged Rebar elements in view";
            pushUnselectedTagData.LargeImage = BitmapToImageSource(Properties.Resources.selectNotTagged);

            //Create splitbutton for tag selecting
            SplitButtonData sbData1 = new SplitButtonData("Tag Selection", "Split");
            SplitButton sb1 = ribbonPanel.AddItem(sbData1) as SplitButton;
            sb1.AddPushButton(pushSelectedTagData);
            sb1.AddPushButton(pushUnselectedTagData);


            //Create a pushbutton to remove dimension values
            //-----------------------------------------------------------

            PushButtonData pushDataRemoveDimVal = new PushButtonData("cmdRemoveDimensionValue", "Remove\ndim. value", thisAssemblyPath, "EiBreRebarUtils.RemoveDimensionValue");
            pushDataRemoveDimVal.ToolTip = "Removes the dimension values from dimension lines.";
           

            // Create a push button to cycle tags
            //--------------------------------------------------------------
            PushButtonData buttonData5 = new PushButtonData("cmdCycleTag", "Cycle Tag\nLeader", thisAssemblyPath, "EiBreRebarUtils.CycleTagLeader");
            PushButton pushButton5 = ribbonPanel.AddItem(buttonData5) as PushButton;
            pushButton5.ToolTip = "Cycle between attached end, free end and no leader";
            pushButton5.LargeImage = BitmapToImageSource(Properties.Resources.CycleTagLeader);

            //PULLDOWN to edit Rebar visibility
            //-----------------------------------------------------------------
            PushButtonData pushData6 = new PushButtonData("cmdSetUnobscuredInView", "Set Unobscured", thisAssemblyPath, "EiBreRebarUtils.SetUnobscuredInView");
            pushData6.ToolTip = "Set selected rebars unboscured in view. Applies to all rebars in view if none is selected";

            PushButtonData pushData7 = new PushButtonData("cmdSetObscuredInView", "Set Obscured", thisAssemblyPath, "EiBreRebarUtils.SetObscuredInView");
            pushData7.ToolTip = "Set selected rebars oboscured in view. Applies to all rebars in view if none is selected";

            PushButtonData pushData8 = new PushButtonData("cmdSetSolidInView", "Set Solid", thisAssemblyPath, "EiBreRebarUtils.SetSolidInView");
            pushData8.ToolTip = "Set selected rebars solid in a 3D view. Applies to all rebars in view if none is selected";

            PushButtonData pushData9 = new PushButtonData("cmdSetNotSolidInView", "Set Not Solid", thisAssemblyPath, "EiBreRebarUtils.SetNotSolidInView");
            pushData9.ToolTip = "Set selected rebars NOT solid in a 3D view. Applies to all rebars in view if none is selected";

            PulldownButtonData pullDataVisibility = new PulldownButtonData("cmdRebarVisibility", "Rebar Visibility");
            PulldownButton pullDownButtonVisibility = ribbonPanel.AddItem(pullDataVisibility) as PulldownButton;

            pullDownButtonVisibility.LargeImage = BitmapToImageSource(Properties.Resources.visibility);
            pullDownButtonVisibility.AddPushButton(pushData6);
            pullDownButtonVisibility.AddPushButton(pushData7);
            pullDownButtonVisibility.AddPushButton(pushData8);
            pullDownButtonVisibility.AddPushButton(pushData9);


            //PULLDOWNGROUP Selection Tools
            //-----------------------------------------------------------------
            PulldownButtonData pullDataSelect = new PulldownButtonData("cmdRebarSelect", "Selection Tools");
            PulldownButton pullDownButtonSelect = ribbonPanel.AddItem(pullDataSelect) as PulldownButton;
            pullDownButtonSelect.LargeImage = BitmapToImageSource(Properties.Resources.select);

            PushButtonData pushData10 = new PushButtonData("cmdSelectRebar", "Rebar Filter", thisAssemblyPath, "EiBreRebarUtils.SelectRebar");
            pushData10.ToolTip = "Rebar Filter";
  
            PushButtonData pushData11 = new PushButtonData("cmdSelectWorkset", "Select Same Workset", thisAssemblyPath, "EiBreRebarUtils.SelectSameWorkset");
            pushData11.ToolTip = "When an element is selected, use this command to select all elements on the same workset visible in view. ";

            PushButtonData pushData12 = new PushButtonData("cmdSelectCategory", "Select Same Category", thisAssemblyPath, "EiBreRebarUtils.SelectSameCategory");
            pushData12.ToolTip = "When an element is selected, use this command to select all elements of the same Category visible in view.";

            pullDownButtonSelect.AddPushButton(pushData10);
            pullDownButtonSelect.AddPushButton(pushData11);
            pullDownButtonSelect.AddPushButton(pushData12);

            //Pushbutton for rebar renumbering
            //----------------------------------------------------------------
            PushButtonData pushData13 = new PushButtonData("cmdRenumberRebar", "Renumber", thisAssemblyPath, "EiBreRebarUtils.RenumberRebar");
            pushData13.ToolTip = "Change a Rebar Number";

            //Pushbutton Schedule Mark Update
            PushButtonData pushDataScheduleMark = new PushButtonData("cmdSceduleMarkUpdate", "Schedule Mark Update", thisAssemblyPath, "EiBreRebarUtils.ScheduleMarkUpdate");
            pushDataScheduleMark.ToolTip = "Combine the value from Partition and Rebar Number to Schedule Mark.";

            //Pushbutton sum of geometry
            PushButtonData pushDataSum = new PushButtonData("cmdSumGeometry", "Sum of geometry", thisAssemblyPath, "EiBreRebarUtils.SumGeometry");
            pushDataSum.ToolTip = "Returns the sum of Length, Area and Volume of the selected elements.";

            //Pushbutton copy rebar
            PushButtonData pushDataCopyRebar = new PushButtonData("cmdCopyRebar", "Copy Rebar", thisAssemblyPath, "EiBreRebarUtils.CopyRebar");
            pushDataCopyRebar.ToolTip = "Copies rebar from line based elements such as beams, walls, slanted columns to other line based elements.";

            //Pushbutton Move from internal to shared
            PushButtonData pushDataMoveInternalShared = new PushButtonData("cmdMoveFromInternalToShared", "Move from internal to shared", thisAssemblyPath, "EiBreRebarUtils.MoveFromInternalToShared");

            //PULLDOWN BUTTON OTHER
            PulldownButtonData pullDataOther = new PulldownButtonData("cmdOther", "Other tools");
            PulldownButton pullDownButtonOther = ribbonPanel.AddItem(pullDataOther) as PulldownButton;
            pullDownButtonOther.AddPushButton(pushData13);
            pullDownButtonOther.AddPushButton(pushDataRemoveDimVal);
            pullDownButtonOther.AddPushButton(pushDataScheduleMark);
            pullDownButtonOther.AddPushButton(pushDataSum);
            pullDownButtonOther.AddPushButton(pushDataCopyRebar);
            pullDownButtonOther.AddPushButton(pushDataMoveInternalShared);
            pullDownButtonOther.LargeImage = BitmapToImageSource(Properties.Resources.other);
            
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            // nothing to clean up in this simple case
            return Result.Succeeded;
        }
        
        private void AskForParameterUpdates(Object sender, FileExportingEventArgs args)
        {
            TaskDialog td1 = new TaskDialog("Oppdatering av parameter før eksport");
            td1.MainInstruction = "Vil du oppdatere Schedule Mark?";
            td1.MainContent = "Slår sammen Partition og Rebar number og lagrer det til Schedule Number";
            td1.CommonButtons = TaskDialogCommonButtons.Yes | TaskDialogCommonButtons.No;
                TaskDialogResult td1Result = td1.Show();
            if (td1Result == TaskDialogResult.Yes)
            {
                TaskDialog.Show("test", "Du trykka på Yes! Is read only: "+args.Document.IsReadOnly.ToString());
                ScheduleMarkUpdate.ScheduleMarkUpdater(args.Document);
            }
        }

        //Credit for this method: https://stackoverflow.com/questions/22499407/how-to-display-a-bitmap-in-a-wpf-image
        BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }
    } //class
} // namespace