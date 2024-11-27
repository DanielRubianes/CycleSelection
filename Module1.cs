using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Editing.Attributes;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Internal.Mapping;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CycleSelection
{
    internal class Module1 : Module
    {
        private static Module1 _this = null;

        #region Class Variables
        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("CycleSelection_Module");
        public static SelectionSet SelectionSet = null;
        public static List<(MapMember, long)> SelectedFeatures = null;
        public static (MapMember, long)? CurrentFeature = null;
        #endregion

        public static void NextFeature()
        {
            if (SelectedFeatures == null)
                return;
            if (CurrentFeature == null)
                CurrentFeature = SelectedFeatures[0];
            int currentIdx = SelectedFeatures.IndexOf(CurrentFeature.Value);
            CurrentFeature = SelectedFeatures[currentIdx == -1 ? 0 : (currentIdx + 1) % SelectedFeatures.Count];
        }

        public static void RemoveCurrentFeature()
        {
            if (SelectedFeatures == null)
                return;
            if (CurrentFeature == null)
                return;
            (MapMember, long) currentFeature = CurrentFeature.Value;
            int currentIdx = SelectedFeatures.IndexOf(CurrentFeature.Value);

            CurrentFeature = SelectedFeatures[currentIdx == -1 ? 0 : (currentIdx + 1) % SelectedFeatures.Count];
            SelectedFeatures.RemoveAt(currentIdx);
            
            Dictionary<MapMember, List<long> > selection = SelectionSet.ToDictionary();

            // Remove object id from corresponding list in SelectionSet dict
            selection[currentFeature.Item1] = selection[currentFeature.Item1].Where(item => item != currentFeature.Item2).ToList();

            // Remove map members with empty lists
            selection = selection.Where(kvp => kvp.Value.Any()).ToDictionary();

            SelectionSet = SelectionSet.FromDictionary(selection);

            if (SelectedFeatures.Count == 0)
            {
                MapView.Active.Map.ClearSelection();
                SelectionSet = null;
                SelectedFeatures = null;
                CurrentFeature = null;
            }
        }

        public static void ZoomToSelected()
        {
            MapView activeMap = MapView.Active;
            if (activeMap.Map.SelectionCount == 0 || CurrentFeature == null) return;
            activeMap.ZoomToSelected();
            Camera mapCamera = activeMap.Camera;
            mapCamera.Scale *= 1.5;
            activeMap.ZoomTo(mapCamera, TimeSpan.Zero);
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

    }
}
