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
using ESRI.ArcGIS.Geometry;

namespace MapControlApplication1
{
    public partial class Query : Form
    {
        #region class private members
        private IMap currentMap;
        private IFeatureLayer currentFeatureLayer;
        private string currentFieldName;
        #endregion

        #region Constructors
        public Query()
        {
            InitializeComponent();
            this.Width = 455;
        }
        #endregion

        #region Properties

        public int MaxWidth
        {
            get { return 700; }
        }

        public int MinWidth
        {
            get { return 455; }
        }

        public IMap CurrentMap
        {
            set
            {
                currentMap = value;
            }
        }

        #endregion

        /// <summary>
        ///     comboBoxSourceLayers.Items.Add(layername);
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Query_Load(object sender, EventArgs e)
        {
            int num_layer = currentMap.LayerCount;
            for (int i = 0; i < num_layer; i++)
            {
                string layername = currentMap.get_Layer(i).Name;
                comboBoxSourceLayers.Items.Add(layername);
            }
            if (num_layer > 0)
            {
                comboBoxSourceLayers.SelectedIndex = 0;
                comboBoxSelectingResult.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// checkedListBoxTargetLayers
        /// </summary>
        private void SpatialQueryLoad()
        {
            checkedListBoxTargetLayers.Items.Clear();
            int num_layer = currentMap.LayerCount;
            for (int i = 0; i < num_layer; i++)
            {
                string layername = currentMap.get_Layer(i).Name;
                checkedListBoxTargetLayers.Items.Add(layername);
            }
            if (num_layer > 0)
            {
                comboBoxSourceLayers.SelectedIndex = 0;
                comboBoxSpatialOperation.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Show spatial query panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button15_Click_1(object sender, EventArgs e)
        {
            if (this.Width == this.MinWidth)
            {
                this.panel1.Visible = true;
                this.Width = this.MaxWidth;
                this.button15.Text = "SpaQuery >>";

                SpatialQueryLoad();
            }
            else
            {
                this.panel1.Visible = false;
                this.Width = this.MinWidth;
                this.button15.Text = "QUERY++";
            }
        }

        /// <summary>
        /// listBoxFields.Items.Add("\"" + pField.Name + "\"");
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxSourceLayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBoxFields.Items.Clear();
            listBoxValues.Items.Clear();

            for (int i = 0; i < currentMap.LayerCount; i++)
            {
                if (currentMap.get_Layer(i).Name == comboBoxSourceLayers.SelectedItem.ToString())
                {
                    currentFeatureLayer = currentMap.get_Layer(i) as IFeatureLayer;
                    label3.Text = "SELECT * FROM " + currentFeatureLayer.Name;
                    break;
                }
            }

            int num_field = currentFeatureLayer.FeatureClass.Fields.FieldCount;
            for (int i = 0; i < num_field; i++)
            {
                IField pField = currentFeatureLayer.FeatureClass.Fields.get_Field(i);
                if (pField.Type != esriFieldType.esriFieldTypeGeometry)
                {
                    listBoxFields.Items.Add("\"" + pField.Name + "\"");
                }
            }

            button1.Enabled = false;
            textBoxWhere.Clear();
        }

        /// <summary>
        /// currentFieldName
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBoxValues.Items.Clear();
            button1.Enabled = true;

            string str = listBoxFields.SelectedItem.ToString();
            str = str.Substring(1);
            str = str.Substring(0, str.Length - 1);

            currentFieldName = str;
        }

        /// <summary>
        /// get unique value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            //IFeatureLayer.FeatureClass
            IDataset dataSet = currentFeatureLayer.FeatureClass as IDataset;
            //Iquerydef's class can't instantiate by its own
            IQueryDef queryDef = ((IFeatureWorkspace)dataSet.Workspace).CreateQueryDef();
            queryDef.Tables = dataSet.Name;
            queryDef.SubFields = "DISTINCT(" + currentFieldName + ")";  //only one column
            //IQueryDef.Evaluate: ICursor
            ICursor pCursor = queryDef.Evaluate();
            //ICursor.NextRow: IRow
            IRow pRow = pCursor.NextRow();
            //IFields.FindField(string)
            IFields pFields = currentFeatureLayer.FeatureClass.Fields;
            IField pField = pFields.get_Field(pFields.FindField(currentFieldName));

            while (pRow != null)
            {
                //to distinguish string with single quotes
                if (pField.Type == esriFieldType.esriFieldTypeString)
                {
                    listBoxValues.Items.Add("\'" + pRow.get_Value(0).ToString() + "\'");
                }
                else
                {
                    listBoxValues.Items.Add(pRow.get_Value(0).ToString());
                }
                //update pRow by ICursor.NextRow
                pRow = pCursor.NextRow();
            }

        }

        /// <summary>
        /// attribute + spatial selection altogether in one
        /// </summary>
        private void SelectFeatures()
        {
            //complemented by the same class of IFeatureLayer
            IFeatureSelection featureSelection = currentFeatureLayer as IFeatureSelection;
            //IQueryFilter is implemented by coclass QueryFilter
            IQueryFilter queryFilter = new QueryFilterClass();
            
            //IQueryFilter.WhereClause
            queryFilter.WhereClause = textBoxWhere.Text;
            //this part of code can be both used by two ways of selection
            switch (comboBoxSelectingResult.SelectedIndex)
            {
                case 0: //create new selection
                    currentMap.ClearSelection();
                    featureSelection.SelectFeatures(queryFilter, esriSelectionResultEnum.esriSelectionResultNew, false);
                    break;
                case 1: //add to current selection
                    featureSelection.SelectFeatures(queryFilter, esriSelectionResultEnum.esriSelectionResultAdd, false);
                    break;
                case 2:
                    featureSelection.SelectFeatures(queryFilter, esriSelectionResultEnum.esriSelectionResultXOR, false);
                    break;
                case 3:
                    featureSelection.SelectFeatures(queryFilter, esriSelectionResultEnum.esriSelectionResultAnd, false);
                    break;
                default:
                    currentMap.ClearSelection();
                    featureSelection.SelectFeatures(queryFilter, esriSelectionResultEnum.esriSelectionResultNew, false);
                    break;
            }
            
            //class SpatialFilter : QueryFilter
            ISpatialFilter spatialFilter = new SpatialFilterClass();

            //attribute + spatial query
            if (this.Width == this.MaxWidth)
            {
                ICursor pCursor;
                featureSelection.SelectionSet.Search(null, false, out pCursor);
                
                IFeatureCursor featureCursor = pCursor as IFeatureCursor;
                IFeature pFeature = featureCursor.NextFeature();

                IGeometry pGeometry = null;
                ITopologicalOperator topologicalOperator;

                while (pFeature != null)
                {
                    if (pGeometry != null)
                    {
                        //this temp topo_Buffer is used to create a searching range by ITopologicalOperator.Buffer
                        ITopologicalOperator topo_Buffer = pFeature.Shape as ITopologicalOperator;
                        //Union is a add-to-existing method
                        topologicalOperator = pGeometry as ITopologicalOperator;
                        pGeometry = topologicalOperator.Union(topo_Buffer.Buffer(double.Parse(textBox1.Text)));
                    }
                    //first in
                    else
                    {
                        topologicalOperator = pFeature.Shape as ITopologicalOperator;
                        pGeometry = topologicalOperator.Buffer(double.Parse(textBox1.Text));
                    }
                    //update pFeature using IFeatureCursor.NextFeature()
                    pFeature = featureCursor.NextFeature();
                }

                //ISpatialFilter.Geometry
                spatialFilter.Geometry = pGeometry;

                switch (comboBoxSpatialOperation.SelectedIndex)
                {
                    case 0: spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects; break;
                    case 1: spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelWithin; break;
                    case 2: spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains; break;
                    case 3: spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelTouches; break;
                    case 4: spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses; break;
                    default: spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects; break;
                }
                //spatial query's real selection is on the targetlayer
                featureSelection.Clear();
                for (int i = 0; i < checkedListBoxTargetLayers.CheckedItems.Count; i++)
                {
                    IFeatureLayer pFeatureLayer = DataOperator.GetFeatureLayerByName(currentMap, checkedListBoxTargetLayers.CheckedItems[i].ToString());
                    featureSelection = pFeatureLayer as IFeatureSelection;
                    featureSelection.SelectFeatures((IQueryFilter)spatialFilter, esriSelectionResultEnum.esriSelectionResultAdd, false);
                }

            }

            IActiveView activeView = currentMap as IActiveView;
            //PartialRefresh: esriViewDrawPhase
            activeView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, activeView.Extent);
        }

        /// <summary>
        /// button_OK
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button13_Click(object sender, EventArgs e)
        {
            SelectFeatures();
            this.Close();
        }

        /// <summary>
        /// button_apply
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button12_Click(object sender, EventArgs e)
        {
            SelectFeatures();
        }

        /// <summary>
        /// button_close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button14_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// double_click to add field name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxFields_DoubleClick(object sender, EventArgs e)
        {
            textBoxWhere.Text += listBoxFields.SelectedItem;
        }

        /// <summary>
        /// double_click to add values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxValues_DoubleClick(object sender, EventArgs e)
        {
            textBoxWhere.Text += listBoxValues.SelectedItem;
        }

        /// <summary>
        /// whether to apply a distance
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //esriUnits.
            if (checkBox1.Checked)
            {
                textBox1.Enabled = true;
            }
            else
            {
                textBox1.Enabled = false;
            }
        }


    }
}
