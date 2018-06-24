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
using ESRI.ArcGIS.Display;

namespace MapControlApplication1
{
    public partial class RendererForm : Form
    {
        #region private members
        private IMap currentMap;
        private ILayer currentLayer;
        private IActiveView currentActiveView;
        #endregion

        #region constructors
        public RendererForm()
        {
            InitializeComponent();
            this.Text = "Layer: " + currentLayer.Name;
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

        private void button1_Click(object sender, EventArgs e)
        {
            panel1.Hide();
            panel2.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            panel1.Show();
            panel2.Hide();
        }
    }
}
