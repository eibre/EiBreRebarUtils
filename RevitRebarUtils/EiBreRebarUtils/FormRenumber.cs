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

namespace EiBreRebarUtils
{
    public partial class FormRenumber : System.Windows.Forms.Form
    {
        //constructor
        public FormRenumber(Document doc,string[] partitions)
        {
            InitializeComponent();
            this.doc = doc;
            comboPartition.Items.AddRange(partitions);
            comboPartition.SelectedItem = 0;
            comboRebarNumber.Items.AddRange(RenumberRebar.GetRebarNumbers(doc, partitions.First()));
        }
        
        //Properties:
        private Document doc { get; set; }
        public string partition { get; set; }
        public int fromNumber { get; set; }
        public int toNumber { get; set; }

        //Events:
        private void comboPartitions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboPartition.Text != string.Empty)
            {
                comboRebarNumber.Items.Clear();
                comboRebarNumber.Items.AddRange(RenumberRebar.GetRebarNumbers(doc, comboPartition.Text));
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
