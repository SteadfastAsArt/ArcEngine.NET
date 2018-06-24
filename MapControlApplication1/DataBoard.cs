using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.ADF;
using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.AnalysisTools;
using ESRI.ArcGIS.DataManagementTools;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geoprocessing;
using ESRI.ArcGIS.Geoprocessor;


namespace MapControlApplication1
{
    public partial class DataBoard : Form
    {
        #region private members
        private IMap currentMap;
        private ILayer currentLayer;
        private IActiveView currentActiveView;
        #endregion 

        #region constructor
        public DataBoard()
        {
            InitializeComponent();
        }
        /*public DataBoard(IMap input)
        {
            InitializeComponent();
            currentMap = input;
        }*/
        #endregion

        #region Properties

        public IMap CurrentMap
        {
            set
            {
                currentMap = value;
            }
        }

        public ILayer CurrentLayer
        {
            set
            {
                currentLayer = value;
            }
        }

        public IActiveView CurrentView
        {
            set
            {
                currentActiveView = value;
            }
        }

        #endregion

        private void DataBoard_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        /*
         * Get Attribute Table
         */ 
        private void LoadData()
        {
            IFeatureLayer pFeatureLayer = currentLayer as IFeatureLayer;

            //Header
            // Add(dataColumn) * numFields
            DataTable dataTable = new DataTable();
            for (int i = 0; i < pFeatureLayer.FeatureClass.Fields.FieldCount; i++)
            {
                DataColumn dataColumn = new DataColumn();
                dataColumn.ColumnName = pFeatureLayer.FeatureClass.Fields.Field[i].Name.ToString();
                dataTable.Columns.Add(dataColumn);
            }

            //Get first feature in the layer
            IFeature pFeature;
            IFeatureCursor pFeatureCursor = pFeatureLayer.Search(null, false);
            pFeature = pFeatureCursor.NextFeature();

            //Fill in each cell(dataRow[i]) with feature.get_Value(i)
            //Add(dataRow)
            while (pFeature != null)
            {
                DataRow dataRow = dataTable.NewRow();
                for (int i = 0; i < dataTable.Columns.Count; i++)
                {
                    if (pFeature.Fields.Field[i].Type.ToString() == "esriFieldTypeGrometry")
                    {
                        dataRow[i] = pFeature.Shape.GeometryType.ToString();
                    }
                    else
                    {
                        dataRow[i] = pFeature.get_Value(i);
                    }
                }
                dataTable.Rows.Add(dataRow);
                pFeature = pFeatureCursor.NextFeature();
            }

            //dataGridView.DataSource : class DataTable
            dataGridView1.DataSource = dataTable;
        }

        /*
         * Double Click a row to get the Extent of a certain feature
         */ 
        private void dataGridView1_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int idx = e.RowIndex;
            this.Text = idx.ToString();
            
            //MainForm mFrm = (MainForm)this.Owner;
            
            IFeature pFeature;
            IFeatureLayer pFeatureLayer = currentLayer as IFeatureLayer;
            IFeatureCursor pFeatureCursor = pFeatureLayer.Search(null, false);
            pFeature = pFeatureCursor.NextFeature();
            for(int i = 0; i < idx; i++)
            {
                pFeature = pFeatureCursor.NextFeature();
            }

            IActiveView pActiveView = currentMap as IActiveView;
            pActiveView.Extent = pFeature.Extent;
            pActiveView.Refresh();
        }

    }
}
