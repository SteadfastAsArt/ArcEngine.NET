using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;

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
using ESRI.ArcGIS.Output;


namespace MapControlApplication1
{
    public sealed partial class MainForm : Form
    {
        #region class private members
        private IMapControl3 m_mapControl = null;
        private string m_mapDocumentName = string.Empty;
        #endregion

        #region class constructor
        public MainForm()
        {
            InitializeComponent();
        }
        #endregion

        private void MainForm_Load(object sender, EventArgs e)
        {
            //get the MapControl
            m_mapControl = (IMapControl3)axMapControl1.Object;

            //disable the Save menu (since there is no document yet)
            menuSaveDoc.Enabled = false;
        }

        #region Main Menu event handlers
        private void menuNewDoc_Click(object sender, EventArgs e)
        {
            //execute New Document command
            ICommand command = new CreateNewDocument();
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        private void menuOpenDoc_Click(object sender, EventArgs e)
        {
            //execute Open Document command
            ICommand command = new ControlsOpenDocCommandClass();
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        private void menuSaveDoc_Click(object sender, EventArgs e)
        {
            //execute Save Document command
            if (m_mapControl.CheckMxFile(m_mapDocumentName))
            {
                //create a new instance of a MapDocument
                IMapDocument mapDoc = new MapDocumentClass();
                mapDoc.Open(m_mapDocumentName, string.Empty);

                //Make sure that the MapDocument is not readonly
                if (mapDoc.get_IsReadOnly(m_mapDocumentName))
                {
                    MessageBox.Show("Map document is read only!");
                    mapDoc.Close();
                    return;
                }

                //Replace its contents with the current map
                mapDoc.ReplaceContents((IMxdContents)m_mapControl.Map);

                //save the MapDocument in order to persist it
                mapDoc.Save(mapDoc.UsesRelativePaths, false);

                //close the MapDocument
                mapDoc.Close();
            }
        }

        private void menuSaveAs_Click(object sender, EventArgs e)
        {
            //execute SaveAs Document command
            ICommand command = new ControlsSaveAsDocCommandClass();
            command.OnCreate(m_mapControl.Object);
            command.OnClick();
        }

        private void menuExitApp_Click(object sender, EventArgs e)
        {
            //exit the application
            Application.Exit();
        }
        #endregion

        //listen to MapReplaced event in order to update the statusbar and the Save menu
        private void axMapControl1_OnMapReplaced(object sender, IMapControlEvents2_OnMapReplacedEvent e)
        {
            //get the current document name from the MapControl
            m_mapDocumentName = m_mapControl.DocumentFilename;

            //if there is no MapDocument, diable the Save menu and clear the statusbar
            if (m_mapDocumentName == string.Empty)
            {
                menuSaveDoc.Enabled = false;
                statusBarXY.Text = string.Empty;
            }
            else
            {
                //enable the Save manu and write the doc name to the statusbar
                menuSaveDoc.Enabled = true;
                statusBarXY.Text = System.IO.Path.GetFileName(m_mapDocumentName);

                //enable pagelayout control to have same view
                axPageLayoutControl1.LoadMxFile(m_mapDocumentName);
            }
        }

        private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            statusBarXY.Text = string.Format("{0}, {1}  {2}", e.mapX.ToString("#######.##"), e.mapY.ToString("#######.##"), axMapControl1.MapUnits.ToString().Substring(4));
        }
        
        /*
         * Show attributes_table 
         */
        private void tableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataBoard dataBoard = new DataBoard();
            dataBoard.CurrentMap = axMapControl1.Map;
            dataBoard.CurrentLayer = m_Layer;
            dataBoard.Text = "LayerName:" + m_Layer.Name;
            dataBoard.Show();
        }

        /*
         * Right_Click TOCControl
         * @param m_Layer
         */
        public ILayer m_Layer = null;
        private void axTOCControl1_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            if (e.button == 2)
            {
                esriTOCControlItem Item = esriTOCControlItem.esriTOCControlItemNone;
                IBasicMap pBasicMap = null;
                ILayer pLayer = null;
                object other = null;
                object index = null;
                axTOCControl1.HitTest(e.x, e.y, ref Item, ref pBasicMap, ref pLayer, ref other, ref index);
                if (Item == esriTOCControlItem.esriTOCControlItemLayer)
                {
                    m_Layer = pLayer;
                    contextMenuStrip1.Show(Control.MousePosition);
                }
            }
        }

        //public IActiveView mainActiveView;
        private void axMapControl1_OnExtentUpdated(object sender, IMapControlEvents2_OnExtentUpdatedEvent e)
        {
            //mainActiveView = axMapControl1.ActiveView;
        }

        /*
         * Dataview
         */ 
        private void radioButton1_Click(object sender, EventArgs e)
        {
            radioButton2.Checked = false;
            radioButton1.Checked = true;
            exportToolStripMenuItem.Enabled = false;

            axPageLayoutControl1.Hide();
            axMapControl1.Show();

            //construct relationship between map and TOC/toolbar
            axToolbarControl1.SetBuddyControl(axMapControl1.Object);
            axTOCControl1.SetBuddyControl(axMapControl1.Object);

            //axMapControl1.Extent = IDisplayTransformation.VisibleBounds; (IEnvelope)
            IActiveView activeView = axPageLayoutControl1.ActiveView.FocusMap as IActiveView;
            IDisplayTransformation displayTransformation = activeView.ScreenDisplay.DisplayTransformation;
            axMapControl1.Extent = displayTransformation.VisibleBounds;
            axMapControl1.ActiveView.Refresh();

        }
        
        /*
         * Pagelayout
         */ 
        private void radioButton2_Click(object sender, EventArgs e)
        {
            radioButton1.Checked = false;
            radioButton2.Checked = true;
            exportToolStripMenuItem.Enabled = true;

            axPageLayoutControl1.Show();
            axMapControl1.Hide();

            //construct relationship between pagelayout and TOC/toolbar
            axToolbarControl1.SetBuddyControl(axPageLayoutControl1.Object);
            axTOCControl1.SetBuddyControl(axPageLayoutControl1.Object);

            // IDisplayTransformation.VisibleBounds = axMapControl1.Extent; (IEnvelope)
            IActiveView pagelayoutView = axPageLayoutControl1.ActiveView.FocusMap as IActiveView;
            IDisplayTransformation pDisplaytrans = pagelayoutView.ScreenDisplay.DisplayTransformation;
            pDisplaytrans.VisibleBounds = axMapControl1.Extent;
            axPageLayoutControl1.ActiveView.Refresh();

        }

        /*
         * Export
         */ 
        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string outPath = OutputClass.OutPath();

            IExport pExport = OutputClass.OutExport(outPath);

            IActiveView outActiveView = axPageLayoutControl1.ActiveView;

            pExport.ExportFileName = outPath;
            double outResolution = axMapControl1.ActiveView.ScreenDisplay.DisplayTransformation.Resolution;

            IPrintAndExport outPrintnExport = new PrintAndExportClass();
            outPrintnExport.Export(outActiveView, pExport, outResolution, true, null);

            MessageBox.Show("输出成功");
        }

        /*
         * Query
         */ 
        private void queryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Query FormQueary = new Query();
            FormQueary.CurrentMap = axMapControl1.Map;
            FormQueary.Show();
        }

        /*
         * Renderer(symbolization)
         */ 
        private void renderingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RendererForm rendererForm = new RendererForm();
            rendererForm.Show();
        }

        /*
         * New SHP
         */ 
        private void shapefileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateNewSHP createNewSHPForm = new CreateNewSHP();
            createNewSHPForm.CurrentMap = axMapControl1.Map;
            createNewSHPForm.Show();
        }

        /*
         * FieldsEditing
         */
        private void propertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditFieldsForm editFieldsForm = new EditFieldsForm();
            editFieldsForm.CurrentLayer = m_Layer;
            editFieldsForm.Show();
        }


    }
}