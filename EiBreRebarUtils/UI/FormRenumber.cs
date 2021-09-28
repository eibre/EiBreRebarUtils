using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.DB;

namespace NO.RebarUtils
{
    public partial class FormRenumber : System.Windows.Forms.Form
    {
        //constructor
        public FormRenumber(Document doc,string[] partitions, Dictionary<string, string[]> rebarNumbers)
        {
            InitializeComponent();
            this.doc = doc;
            this.rebarNumbers = rebarNumbers;
            comboPartition.Items.AddRange(partitions);
            comboPartition.SelectedItem = 0;
            comboRebarNumber.Items.AddRange(rebarNumbers[partitions.First()]);
        }
        
        //Properties:
        private Document doc { get; set; }
        private Dictionary<string, string[]> rebarNumbers { get; set; }
        public string partition { get; set; }
        public int fromNumber { get; set; }
        public int toNumber { get; set; }

        //Events:
        private void comboPartitions_SelectedIndexChanged(object sender, EventArgs e)
        {
            try 
            {
                comboRebarNumber.Items.Clear();
                comboRebarNumber.Items.AddRange(rebarNumbers[comboPartition.Text]);
            }
            catch (KeyNotFoundException)
            {
                comboRebarNumber.Items.Clear();
            }
        }

        private void buttonChange_Clicked(object sender, EventArgs e)
        {
            partition = comboPartition.Text;
            fromNumber = int.Parse(comboRebarNumber.Text);
            toNumber = int.Parse(textNewNumber.Text);
        }
        private void buttonClose_Clicked(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
