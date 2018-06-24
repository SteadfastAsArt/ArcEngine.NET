using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;


namespace MapControlApplication1
{
    public partial class EditFieldsForm : Form
    {
        #region private members
        private ILayer currentLayer;
        private object before;
        #endregion

        #region constructor
        public EditFieldsForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Properties

        public ILayer CurrentLayer
        {
            set
            {
                currentLayer = value;
            }
        }

        #endregion

        /// <summary>
        /// Load Existing Fields
        /// </summary>
        private void Load_ExistingFields()
        {
            //ILayerFields
            ILayerFields layerfields = currentLayer as ILayerFields;
            //add exited through ILayerFields
            for (int i = 0; i < layerfields.FieldCount; i++)
            {
                int index = this.dataGridView1.Rows.Add();

                dataGridView1.Rows[index].Cells[0].Value = layerfields.Field[i].Name.ToString();

                switch (layerfields.Field[i].Type)
                {
                    case esriFieldType.esriFieldTypeOID:
                        dataGridView1.Rows[index].Cells[1].Value = Column2.Items[6];
                        dataGridView1.Rows[index].Cells[1].ReadOnly = true;
                        break;
                    case esriFieldType.esriFieldTypeGeometry:
                        dataGridView1.Rows[index].Cells[1].Value = Column2.Items[7];
                        dataGridView1.Rows[index].Cells[1].ReadOnly = true;
                        break;
                    case esriFieldType.esriFieldTypeSmallInteger: dataGridView1.Rows[index].Cells[1].Value = Column2.Items[0]; break;
                    case esriFieldType.esriFieldTypeInteger: dataGridView1.Rows[index].Cells[1].Value = Column2.Items[1]; break;
                    case esriFieldType.esriFieldTypeSingle: dataGridView1.Rows[index].Cells[1].Value = Column2.Items[2]; break;
                    case esriFieldType.esriFieldTypeDouble: dataGridView1.Rows[index].Cells[1].Value = Column2.Items[3]; break;
                    case esriFieldType.esriFieldTypeString: dataGridView1.Rows[index].Cells[1].Value = Column2.Items[4]; break;
                    case esriFieldType.esriFieldTypeDate: dataGridView1.Rows[index].Cells[1].Value = Column2.Items[5]; break;
                }
            }
        }

        /// <summary>
        /// EditFieldsForm_Load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditFieldsForm_Load(object sender, EventArgs e)
        {
            Load_ExistingFields();
            this.Text = "Layer: " + currentLayer.Name.ToString();
        }

        /// <summary>
        /// CellBeginEdit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            string msg = String.Format("Editing Cell at ({0}, {1})", e.ColumnIndex, e.RowIndex);
            this.Text = msg;
            before = this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
        }

        /// <summary>
        /// CellEndEdit
        /// can't edit existing fields' properties in this mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            string msg = String.Format("Finished Editing Cell at ({0}, {1})", e.ColumnIndex, e.RowIndex);
            this.Text = msg;

            ILayerFields layerfields = currentLayer as ILayerFields;
            int num_fields = layerfields.FieldCount;

            if (e.RowIndex < num_fields)
            {
                MessageBox.Show("You are not allowed to edit existing fields' names");
                this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = before;
            }

        }

        /// <summary>
        /// button: confirm
        /// to check the fields intended to be added
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            ILayerFields layerfields = currentLayer as ILayerFields;

            int num_fields = layerfields.FieldCount;
            int num_rows = dataGridView1.RowCount;

            IFeatureLayer featurelayer = currentLayer as IFeatureLayer;

            for (int i = num_fields; i < num_rows - 1; i++)
            {
                //valid input:
                if (dataGridView1.Rows[i].Cells[0].Value != null && dataGridView1.Rows[i].Cells[1].Value != null)
                {
                    IFieldEdit fieldedit = new FieldClass();
                    fieldedit.Name_2 = dataGridView1.Rows[i].Cells[0].Value.ToString();
                    fieldedit.AliasName_2 = dataGridView1.Rows[i].Cells[0].Value.ToString();

                    switch (dataGridView1.Rows[i].Cells[1].Value.ToString())
                    {
                        case "Short Integer": fieldedit.Type_2 = esriFieldType.esriFieldTypeSmallInteger; break;
                        case "Long Integer": fieldedit.Type_2 = esriFieldType.esriFieldTypeInteger; break;
                        case "Float": fieldedit.Type_2 = esriFieldType.esriFieldTypeSingle; break;
                        case "Double": fieldedit.Type_2 = esriFieldType.esriFieldTypeDouble; break;
                        case "Text": fieldedit.Type_2 = esriFieldType.esriFieldTypeString; break;
                        case "Date": fieldedit.Type_2 = esriFieldType.esriFieldTypeDate; break;
                    }

                    featurelayer.FeatureClass.AddField((IField)fieldedit);
                }
            }

            this.Close();
        }

        /// <summary>
        /// UserDeletingRow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            ILayerFields layerfields = currentLayer as ILayerFields;
            IFeatureLayer featurelayer = currentLayer as IFeatureLayer;

            int num_fields = layerfields.FieldCount;
            int num_rows = dataGridView1.RowCount;

            if (e.Row.Index < num_fields)
            {
                DialogResult dr = MessageBox.Show("Are you sure to delete the existing field?", "REMINDER", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                {
                    //todeletefield: e.Row.Index
                    IField todeletefield = layerfields.Field[e.Row.Index];
                    featurelayer.FeatureClass.DeleteField(todeletefield);
                }
                else 
                { 
                    e.Cancel = true; 
                }
            }
        }





    }
}
