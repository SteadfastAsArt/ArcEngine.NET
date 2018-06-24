using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;

using ESRI.ArcGIS.Output;

namespace MapControlApplication1
{
    class OutputClass
    {
        /// <summary>
        ///     SaveFileDialog.Filter: string
        /// </summary>
        /// <returns></returns>
        public static string OutPath()
        {
            SaveFileDialog m_save = new SaveFileDialog();
            
            m_save.Filter = "jpeg图片(*.jpg)|*.jpg|tiff图片(*.tif)|*.tif|png图片(*.png)|*.png";
            m_save.ShowDialog();

            string outPath = m_save.FileName;

            return outPath;
        }

        /// <summary>
        ///     IExport point to certain type of instantiation
        /// </summary>
        /// <param name="pOutPath"></param>
        /// <returns></returns>
        public static IExport OutExport(string pOutPath)
        {
            IExport outExport = null;

            if (pOutPath.EndsWith(".jpg"))
            {
                outExport = new ExportJPEGClass();

                //Information of Corordinates, BUT also NOT WELL worked as for .png
                IWorldFileSettings WFS = outExport as IWorldFileSettings;
                WFS.OutputWorldFile = true;
            }
            else if (pOutPath.EndsWith(".tif"))
            {
                outExport = new ExportTIFFClass();
                //Information of Corordinates
                ((IExportTIFF)outExport).GeoTiff = true;
            }
            else if (pOutPath.EndsWith(".png"))
            {
                outExport = new ExportPNGClass();
            }

            return outExport;
        }

    }
}
