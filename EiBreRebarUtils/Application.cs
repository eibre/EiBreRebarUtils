using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Drawing;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System.Windows.Media.Imaging;
using Autodesk.Revit.ApplicationServices;

namespace NO.RebarUtils
{
    public class CsAddPanel : IExternalApplication
    {
        // Both OnStartup and OnShutdown must be implemented as public method
        public Result OnStartup(UIControlledApplication application)
        {
            // Registrere event:
            //application.ControlledApplication.FileExporting += new EventHandler<FileExportingEventArgs>(AskForParameterUpdates);
            // Create a custom ribbon tab
            String tabName = "RebarUtils";
            application.CreateRibbonTab(tabName);
            // Add a new ribbon panel
            RibbonPanel ribbonPanel = application.CreateRibbonPanel(tabName, "EiBre Rebar Utils");
            string thisAssemblyPath = Assembly.GetExecutingAssembly().Location;
            // buttons: Copy Rebar, Renumber Rebar, Tag all, cycle tag,  Visibility, Selection, Other

            //Pushbutton copy rebar
            PushButtonData pushDataCopyRebar = new PushButtonData("cmdCopyRebar", "Copy Rebar", thisAssemblyPath, "NO.RebarUtils.CopyRebar");
            pushDataCopyRebar.ToolTip = "Copies rebar from line based elements such as beams, walls, slanted columns to other line based elements.";
            PushButton pushButtonCopyRebar = ribbonPanel.AddItem(pushDataCopyRebar) as PushButton;
            pushButtonCopyRebar.LargeImage = BitmapToImageSource(Properties.Resources.copyRebar);

            //Pushbutton for rebar renumbering
            //----------------------------------------------------------------
            PushButtonData pushDataRenumber = new PushButtonData("cmdRenumberRebar", "Renumber", thisAssemblyPath, "NO.RebarUtils.RenumberRebar");
            pushDataRenumber.ToolTip = "Change a Rebar Number";
            PushButton pushButtonRenumber = ribbonPanel.AddItem(pushDataRenumber) as PushButton;
            pushButtonRenumber.LargeImage = BitmapToImageSource(Properties.Resources.renumber);

            // Create a push button to trigger the tag all rebars command.

            PushButtonData buttonData = new PushButtonData("cmdTagAllRebarsInHost",
               "Tag rebars \nin host", thisAssemblyPath, "NO.RebarUtils.TagAllRebarsInHost");
            
            PushButton pushButton = ribbonPanel.AddItem(buttonData) as PushButton;
            pushButton.ToolTip = "Tag all rebars in a host, the bars must be visible in active view";
            pushButton.LargeImage = BitmapToImageSource(Properties.Resources.TagAllRebarsInHost);
                              

            // Create a push button to cycle tags
            //--------------------------------------------------------------
            PushButtonData buttonData5 = new PushButtonData("cmdCycleTag", "Cycle Tag\nLeader", thisAssemblyPath, "NO.RebarUtils.CycleTagLeader");
            PushButton pushButton5 = ribbonPanel.AddItem(buttonData5) as PushButton;
            pushButton5.ToolTip = "Cycle between attached end, free end and no leader";
            pushButton5.LargeImage = BitmapToImageSource(Properties.Resources.CycleTagLeader);

            //PULLDOWN to edit Rebar visibility
            //-----------------------------------------------------------------
            PushButtonData pushData6 = new PushButtonData("cmdSetUnobscuredInView", "Set Unobscured", thisAssemblyPath, "NO.RebarUtils.SetUnobscuredInView");
            pushData6.ToolTip = "Set selected rebars unboscured in view. Applies to all rebars in view if none is selected";

            PushButtonData pushData7 = new PushButtonData("cmdSetObscuredInView", "Set Obscured", thisAssemblyPath, "NO.RebarUtils.SetObscuredInView");
            pushData7.ToolTip = "Set selected rebars oboscured in view. Applies to all rebars in view if none is selected";

            PushButtonData pushData8 = new PushButtonData("cmdSetSolidInView", "Set Solid", thisAssemblyPath, "NO.RebarUtils.SetSolidInView");
            pushData8.ToolTip = "Set selected rebars solid in a 3D view. Applies to all rebars in view if none is selected";

            PushButtonData pushData9 = new PushButtonData("cmdSetNotSolidInView", "Set Not Solid", thisAssemblyPath, "NO.RebarUtils.SetNotSolidInView");
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
            //Create pushbutton data to select all tagged rebars in a view
            PushButtonData pushSelectedTagData = new PushButtonData("cmdSelectTaggedRebar", "Select tagged rebars", thisAssemblyPath, "NO.RebarUtils.SelectTaggedRebars");
            pushSelectedTagData.ToolTip = "Selects all tagged Rebar elements in view";

            //Create pushbutton data to select all tagged rebars in a view
            PushButtonData pushUnselectedTagData = new PushButtonData("cmdSelectUntaggedRebar", "Select un-tagged rebars", thisAssemblyPath, "NO.RebarUtils.SelectUntaggedRebars");
            pushUnselectedTagData.ToolTip = "Selects all un-tagged Rebar elements in view";
            
            PushButtonData pushData10 = new PushButtonData("cmdSelectRebar", "Rebar Filter", thisAssemblyPath, "NO.RebarUtils.SelectRebar");
            pushData10.ToolTip = "Rebar Filter";
  
            PushButtonData pushData11 = new PushButtonData("cmdSelectWorkset", "Select Same Workset", thisAssemblyPath, "NO.RebarUtils.SelectSameWorkset");
            pushData11.ToolTip = "When an element is selected, use this command to select all elements on the same workset visible in view. ";

            PushButtonData pushData12 = new PushButtonData("cmdSelectCategory", "Select Same Category", thisAssemblyPath, "NO.RebarUtils.SelectSameCategory");
            pushData12.ToolTip = "When an element is selected, use this command to select all elements of the same Category visible in view.";

            PushButtonData pushSelectTopLayer = new PushButtonData("cmdSelectTopLayer", "Select Top Layer (OK)", thisAssemblyPath, "NO.RebarUtils.SelectTopLayer");
            pushSelectTopLayer.ToolTip = "Select rebar that is completely inside the bounding box of the top half of a Rebar Host.";

            PushButtonData pushSelectBottomLayer = new PushButtonData("cmdSelectBottomLayer", "Select Bottom Layer (UK)", thisAssemblyPath, "NO.RebarUtils.SelectBottomLayer");
            pushSelectBottomLayer.ToolTip = "Select rebar that is completely inside the bounding box of the bottom half of a Rebar Host.";

            pullDownButtonSelect.AddPushButton(pushSelectedTagData);
            pullDownButtonSelect.AddPushButton(pushUnselectedTagData);
            pullDownButtonSelect.AddPushButton(pushData10);
            pullDownButtonSelect.AddPushButton(pushData11);
            pullDownButtonSelect.AddPushButton(pushData12);
            pullDownButtonSelect.AddPushButton(pushSelectTopLayer);
            pullDownButtonSelect.AddPushButton(pushSelectBottomLayer);

            //Pushbutton RebarInBend
            PushButtonData pushDataRebarInBend = new PushButtonData("cmdRebarInBend", "Rebar in bend", thisAssemblyPath, "NO.RebarUtils.RebarInBend");
            pushDataRebarInBend.ToolTip = "Pick a rebar set to add rebars in all bends with same diameter as the bent bar.";
            PushButton pushButtonRebarInBend = ribbonPanel.AddItem(pushDataRebarInBend) as PushButton;
            pushButtonRebarInBend.LargeImage = BitmapToImageSource(Properties.Resources.RebarInBend);

            //Pushbutton PickRebarToIsolateAndTag
            PushButtonData pushDataPickRebarToIsolateAndTag = new PushButtonData("cmdPickRebarToIsolateAndTag", "Pick and tag", thisAssemblyPath, "NO.RebarUtils.PickRebarToIsolateAndTag");
            pushDataPickRebarToIsolateAndTag.ToolTip = "Pick a rebar from a rebar set and a dimension line to isolate the rebar, attach it to the dimension line and place a tag";
            PushButton pushButtonPickRebarToIsolateAndTag = ribbonPanel.AddItem(pushDataPickRebarToIsolateAndTag) as PushButton;
            pushButtonPickRebarToIsolateAndTag.LargeImage = BitmapToImageSource(Properties.Resources.PickRebarToIsolateAndTag);

            //Pushbutton RebarParameterFromText
            PushButtonData pushDataRebarParameterFromText = new PushButtonData("cmdRebarParameterFromText", "Parameters\nfrom text", thisAssemblyPath, "NO.RebarUtils.RebarParameterFromText");
            pushDataRebarParameterFromText.ToolTip = "Set bar type (diameter), layout rule, spacing, partition and comment from a string. e.g. ø12c200-P UK";
            PushButton pushButtonRebarParameterFromText = ribbonPanel.AddItem(pushDataRebarParameterFromText) as PushButton;
            pushButtonRebarParameterFromText.LargeImage = BitmapToImageSource(Properties.Resources.RebarParameterFromText);
            
            //Pushbutton Schedule Mark Update
            PushButtonData pushDataScheduleMark = new PushButtonData("cmdSceduleMarkUpdate", "Schedule Mark Update", thisAssemblyPath, "NO.RebarUtils.ScheduleMarkUpdate");
            pushDataScheduleMark.ToolTip = "Combine the value from Partition and Rebar Number to Schedule Mark.";

            //Pushbutton sum of geometry
            PushButtonData pushDataSum = new PushButtonData("cmdSumGeometry", "Sum of geometry", thisAssemblyPath, "NO.RebarUtils.SumGeometry");
            pushDataSum.ToolTip = "Returns the sum of Length, Area and Volume of the selected elements.";

            //Pushbutton Move from internal to shared
            PushButtonData pushDataMoveInternalShared = new PushButtonData("cmdMoveFromInternalToShared", "Move from internal to shared", thisAssemblyPath, "NO.RebarUtils.MoveFromInternalToShared");
            pushDataMoveInternalShared.ToolTip = "This command moves and rotates a link or any element from the internal coordinate system to the shared coordinate system. This is useful if you insert links Origin to Origin because Surevey Point to Survey Point doesn't exist.";
            //Create a pushbutton to remove dimension values
            PushButtonData pushDataRemoveDimVal = new PushButtonData("cmdRemoveDimensionValue", "Remove\ndim. value", thisAssemblyPath, "NO.RebarUtils.RemoveDimensionValue");
            pushDataRemoveDimVal.ToolTip = "Removes the dimension values from dimension lines.";

            //Pushbutton CopyRebarNumberFromScheduleMark
            PushButtonData pushDataCopyRebarNumberFromScheduleMark = new PushButtonData("cmdCopyRebarNumberFromScheduleMark", "Copy Rebar Number from Shedule Mark", thisAssemblyPath, "NO.RebarUtils.CopyRebarNumberFromScheduleMark");
            pushDataCopyRebarNumberFromScheduleMark.ToolTip = "This command asks the user to select a partition and tries to change all Rebar Number values to match the last part of Schedue Mark. All schedule marks must begin with the partition name, and there can only be one number extracted from Schedule Mark for each Rebar Number.";

            //Pushbutton TagToDim
            PushButtonData pushDataTagToDim = new PushButtonData("cmdTagToDim", "Connect tag to dimension", thisAssemblyPath, "NO.RebarUtils.TagToDim");
            pushDataTagToDim.ToolTip = "Pick a tag and a dimension to attach the tag leader to the nearest endpoint of the dimesion.";

            //Pushbutton ActiveViewToOpenSheet
            PushButtonData pushDataActiveViewToOpenSheet = new PushButtonData("cmdActiveViewToOpenSheet", "Add active view to open sheet", thisAssemblyPath, "NO.RebarUtils.ActiveViewToOpenSheet");
            pushDataActiveViewToOpenSheet.ToolTip = "Adds the active view to a open sheet. If more than one sheet is open you are prompted to choose one.";

            //Pushbutton DisallowJoin
            PushButtonData pushDataDisallowJoin = new PushButtonData("cmdDisallowJoin", "Disallow Join", thisAssemblyPath, "NO.RebarUtils.DisallowJoin");
            pushDataDisallowJoin.ToolTip = "Pick walls and/or structural framing to disallow join in both ends. End the selection by clicking finish in top left corner.";
            //Pushbutton ColumnsCutFloors
            PushButtonData pushDataColumnsCutFloor = new PushButtonData("cmdColumnsCutFloor", "Columns Cut Floor", thisAssemblyPath, "NO.RebarUtils.SwitchJoinOrder");
            pushDataColumnsCutFloor.ToolTip = "Pick floors to switch join order such that floors are cut by columns. End the selection by clicking finish in top left corner.";
            //Pushbutton PasteImageFromClipboard
            PushButtonData pushDataPasteImage = new PushButtonData("cmdPasteImageFromClipboard", "Paste Image From Clipboard", thisAssemblyPath, "NO.RebarUtils.PasteImageFromClipboard");
            pushDataPasteImage.ToolTip = "Copy a image to clipboard and paste it. The image will be saved in a new folder in the same location as your central file. You will be prompted to pick a point which is the center poit of the image";
            //Pushbutton About
            PushButtonData pushDataAbout = new PushButtonData("cmdAbout", "About", thisAssemblyPath, "NO.RebarUtils.About");
            pushDataAbout.ToolTip = "About..";

            //PULLDOWN BUTTON OTHER
            PulldownButtonData pullDataOther = new PulldownButtonData("cmdOther", "Other tools");
            PulldownButton pullDownButtonOther = ribbonPanel.AddItem(pullDataOther) as PulldownButton;

            pullDownButtonOther.AddPushButton(pushDataRemoveDimVal);
            pullDownButtonOther.AddPushButton(pushDataScheduleMark);
            pullDownButtonOther.AddPushButton(pushDataTagToDim);
            pullDownButtonOther.AddPushButton(pushDataSum);
            pullDownButtonOther.AddPushButton(pushDataMoveInternalShared);
            pullDownButtonOther.AddPushButton(pushDataActiveViewToOpenSheet);
            pullDownButtonOther.AddPushButton(pushDataDisallowJoin);
            pullDownButtonOther.AddPushButton(pushDataColumnsCutFloor);
            pullDownButtonOther.AddPushButton(pushDataPasteImage);
            pullDownButtonOther.AddSeparator();
            pullDownButtonOther.AddPushButton(pushDataAbout);
            

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