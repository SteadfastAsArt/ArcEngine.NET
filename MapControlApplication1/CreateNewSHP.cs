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
    public partial class CreateNewSHP : Form
    {
        #region private members
        private IMap currentMap;

        #endregion

        #region constructor
        public CreateNewSHP()
        {
            InitializeComponent();
        }
        #endregion

        #region Properties

        public IMap CurrentMap
        {
            set
            {
                currentMap = value;
            }
        }

        #endregion

        /// <summary>
        /// path_choosing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            // Form: FolderBrowserDialog
            FolderBrowserDialog browserDialog = new FolderBrowserDialog();
            // DialogResult.OK
            if (browserDialog.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = browserDialog.SelectedPath;
            }
        }

        /// <summary>
        /// button: confirm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            ISpatialReference currentSpatialRef = currentMap.SpatialReference;
            IFeatureClass featureClass = DataOperator.CreateShp(textBox3.Text, textBox2.Text, textBox1.Text, comboBox1.SelectedItem.ToString(), currentSpatialRef);
            if (featureClass == null)
            {
                MessageBox.Show("FAIL");
                return;
            }

            DataOperator.AddFeatureClass_Map(featureClass, textBox1.Text, currentMap);

            this.Close();
        }

        /// <summary>
        /// button: cancel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
