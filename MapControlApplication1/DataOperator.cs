using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;

namespace MapControlApplication1
{
    class DataOperator
    {
        /// <summary>
        /// name_matching
        /// </summary>
        /// <param name="map"></param>
        /// <param name="layername"></param>
        /// <returns></returns>
        public static IFeatureLayer GetFeatureLayerByName(IMap map, string layername)
        {
            for (int i = 0; i < map.LayerCount; i++)
            {
                if (map.get_Layer(i).Name == layername)
                {
                    return (IFeatureLayer)map.get_Layer(i);
                }
            }
            return null;
        }

        /// <summary>
        /// method: CreateShp
        /// </summary>
        /// <param name="sParendirectory"></param>
        /// <param name="sWorkSpaceName"></param>
        /// <param name="sFileName"></param>
        /// <param name="fType"></param>
        /// <param name="pSpatialRef"></param>
        /// <returns></returns>
        public static IFeatureClass CreateShp(string sParendirectory, string sWorkSpaceName, string sFileName, string fType, ISpatialReference pSpatialRef)
        {
            if (System.IO.Directory.Exists(sParendirectory + sWorkSpaceName))
            {
                System.IO.Directory.Delete(sParendirectory + sWorkSpaceName, true);
            }

            //open() a Workspace through name
            IWorkspaceFactory workSpaceFactory = new ShapefileWorkspaceFactoryClass();
            IWorkspaceName workSpaceName = workSpaceFactory.Create(sParendirectory, sWorkSpaceName, null, 0);
            IName pName = workSpaceName as IName;
            IWorkspace _WorkSpace = (IWorkspace)pName.Open();
            //IFeatureWorkspace
            IFeatureWorkspace featureWorkSpace = _WorkSpace as IFeatureWorkspace;
            //prepare necessary fields:
            //fields
            IFields pFields = new FieldsClass();
            IFieldsEdit fieldsEdit = pFields as IFieldsEdit;

            //NECESSARY field1: OID
            IFieldEdit fieldEdit = new FieldClass();
            //field_name
            fieldEdit.Name_2 = "OID";
            fieldEdit.AliasName_2 = "序号";
            //field_type
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeOID;
            //IFieldsEdit.AddField
            fieldsEdit.AddField((IField)fieldEdit);

            //NECESSARY field2: geometry
            fieldEdit = new FieldClass();
            fieldEdit.Name_2 = "Shape";
            fieldEdit.AliasName_2 = "形状";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;
            //IFieldsEdit.GeometryDef
            IGeometryDefEdit geometryDefEdit = new GeometryDefClass();
            switch(fType)
            {
                case "Point": geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint; break;
                case "Polyline": geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolyline; break;
                case "Polygon": geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon; break;
            }
            //SpatialReference
            if (pSpatialRef != null)
            {
                geometryDefEdit.SpatialReference_2 = pSpatialRef;
            }
            fieldEdit.GeometryDef_2 = geometryDefEdit;
            fieldsEdit.AddField((IField)fieldEdit);

            //Another attribute field: Name
            fieldEdit = new FieldClass();
            fieldEdit.Name_2 = "Name";
            fieldEdit.AliasName_2 = "名称";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldsEdit.AddField((IField)fieldEdit);

            //CORE Method
            //IFeatureWorkspace.CreateFeatureClass(...) to release the Shp
            IFeatureClass featureClass = featureWorkSpace.CreateFeatureClass(sFileName, pFields, null, null, esriFeatureType.esriFTSimple, "Shape", "");
            if (featureClass == null)
            {
                MessageBox.Show("创建失败");
                return null;
            }
            else
            {
                MessageBox.Show("创建成功");
                return featureClass;
            }

        }

        /// <summary>
        /// distinguish ESRI.ArcGIS.Carto & ESRI.ArcGIS.Geodatabase;
        /// </summary>
        /// <param name="pFeatureClass"></param>
        /// <param name="sLayerName"></param>
        /// <param name="m_map"></param>
        public static void AddFeatureClass_Map(IFeatureClass pFeatureClass, string sLayerName, IMap m_map)
        {
            //to have the new featureclass on map, we need to create a new layer
            IFeatureLayer pFeatureLayer = new FeatureLayerClass();
            pFeatureLayer.FeatureClass = pFeatureClass;
            pFeatureLayer.Name = sLayerName;
            //AddLayer(ILayer)
            ILayer pLayer = pFeatureLayer as ILayer;
            m_map.AddLayer(pLayer);
            //Refresh()
            IActiveView activeview = m_map as IActiveView;
            activeview.Refresh();
        }



    }
}
