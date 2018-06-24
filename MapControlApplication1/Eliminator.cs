using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    class Eliminator
    {
        public static ILayer GetLayerbyName(string name)
        {

            return null;
        }

        /*
         * PREPARE FOR SPATIALFILTER.GEOMETRY
         */
        public static IGeometry GetFeatureLayerGeometryUnion(IFeatureLayer featurelayer)
        {
            IGeometry geometry = null;
            ITopologicalOperator topologicaloperator;

            IFeatureCursor featurecursor = featurelayer.Search(null, false);
            IFeature feature = featurecursor.NextFeature();

            while (feature != null)
            {
                if (geometry != null)
                {
                    /*topologicaloperator 由几何对象实例化，并可调用一系列顾名思义的方法，再由几何对象本身接收这些方法的结果*/
                    topologicaloperator = geometry as ITopologicalOperator;
                    /* Union, 将一个图层中的所有feature UNION在一起 */
                    geometry = topologicaloperator.Union(feature.Shape);
                }
                else geometry = feature.Shape;
                feature = featurecursor.NextFeature();
            }

            return geometry;
        }
        
        /*
         * Under_Restriction ( area || compactness)
         */
        public static IFeatureSelection SelectbySpecification(ILayer layer)
        {
            IFeatureLayer featurelayer = layer as IFeatureLayer;
            IFeatureSelection featureselection = featurelayer as IFeatureSelection;
            IQueryFilter queryfilter = new QueryFilterClass();
            queryfilter.WhereClause = "";
            
            featureselection.SelectFeatures(queryfilter, esriSelectionResultEnum.esriSelectionResultNew, false);

            return featureselection;
        }

        /* 
         * THIS FUNCTION IS used to determine if a specific polygon is touched by the specific feature in SelectionSet
         * INPUT : Under_Restriction ( area || compactness) SelectionSet
         * 
         * Geometry::IRelationalOperator.touch()
         */
        public static bool Is_Touch(IGeometry selectedgeom, IGeometry input)
        {
            IRelationalOperator relop = selectedgeom as IRelationalOperator;
            return relop.Touches(input);
        } 

        /*
         * TODO
         */
        public static IFeatureClass Merge()
        {

            return null;
        }

        /*
         * TODO : SPEED UP QUERY 
         */ 
        public static IGeometry Buffer()
        {

            return null;
        }

        /* 
         * THIS FUNCTION IS used to choose which bigger polygons in layer 
         * is suitable for specific features in SelectionSet to Merge
         * UNDER privilige : same previous type && same region && longest public side
         */
        public static void Processing(IFeatureSelection selectedfeatureselection, ILayer layer)
        {
            /*Initialize inner loop for every feature in the layer*/
            IFeatureLayer featurelayer = layer as IFeatureLayer;
            IFeatureCursor featurecursor = featurelayer.Search(null, false);
            IFeature feature = featurecursor.NextFeature();

            /*Initialize outter loop for features in selectionset*/
            ISelectionSet selectionset = selectedfeatureselection.SelectionSet;
            ICursor cursor;
            selectionset.Search(null, false, out cursor);
            IFeatureCursor selectedfeaturecursor = cursor as IFeatureCursor;
            IFeature selectedfeature = selectedfeaturecursor.NextFeature();

            IFeatureCursor mark;

            while ( selectedfeature != null)
            {
                IGeometry selectedgeom = selectedfeature.Shape;
                double maxcommonside = 0;
                
                while ( feature != null )
                {
                    if(Is_Touch( selectedgeom, feature.Shape))
                    {
                        ITopologicalOperator intersectOp = feature.Shape as ITopologicalOperator;
                        IPolycurve commonside = (IPolycurve)intersectOp.Intersect(selectedgeom, esriGeometryDimension.esriGeometry1Dimension);
                        
                        /*THIS can be replaced by any other comparing model*/
                        if (commonside.Length > maxcommonside)
                        {
                            maxcommonside = commonside.Length;
                            mark = featurecursor;
                        }

                    }

                    feature = featurecursor.NextFeature();
                }
                selectedfeature = selectedfeaturecursor.NextFeature();
            }
        }

        public static void ELIMINATION(string name)
        {
            ILayer layer = GetLayerbyName(name);

            IFeatureSelection selectedfeature = SelectbySpecification(layer);

            Processing(selectedfeature, layer);
        }

    }
}
