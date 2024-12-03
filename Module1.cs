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

        public static void ClearFeatures()
        {
            SelectionSet = null;
            SelectedFeatures = null;
            CurrentFeature = null;
            DeactivateState("CycleSelection_StoredSelection");
        }

        public static void NextFeature()
        {
            if (SelectedFeatures == null)
                return;
            if (CurrentFeature == null)
                CurrentFeature = SelectedFeatures[0];
            int currentIdx = SelectedFeatures.IndexOf(CurrentFeature.Value);
            CurrentFeature = SelectedFeatures[currentIdx == -1 ? 0 : (currentIdx + 1) % SelectedFeatures.Count];
        }

        public static void PreviousFeature()
        {
            if (SelectedFeatures == null)
                return;
            if (CurrentFeature == null)
                CurrentFeature = SelectedFeatures[0];
            int currentIdx = SelectedFeatures.IndexOf(CurrentFeature.Value);
            CurrentFeature = SelectedFeatures[currentIdx == 0 ? ^1 : (currentIdx - 1) % SelectedFeatures.Count];
        }

        public static void RemoveCurrentFeature()
        {
            if (SelectedFeatures == null)
                return;
            if (SelectedFeatures.Count < 2)
            {
                MapView.Active.Map.ClearSelection();
                ClearFeatures();
                return;
            }
            if (CurrentFeature == null)
                return;

            (MapMember, long) removeFeature = CurrentFeature.Value;
            int currentIdx = SelectedFeatures.IndexOf(CurrentFeature.Value);

            CurrentFeature = SelectedFeatures[currentIdx == -1 ? 0 : (currentIdx + 1) % SelectedFeatures.Count];
            SelectedFeatures.RemoveAt(currentIdx);
            
            Dictionary<MapMember, List<long> > selection = SelectionSet.ToDictionary();

            // Remove object id from corresponding list in SelectionSet dict
            selection[removeFeature.Item1] = selection[removeFeature.Item1].Where(item => item != removeFeature.Item2).ToList();

            // Remove map members with empty lists
            selection = selection.Where(kvp => kvp.Value.Any()).ToDictionary();

            SelectionSet = SelectionSet.FromDictionary(selection);
        }

        public static Boolean SelectCurrentFeature()
        {
            if (CurrentFeature == null)
                return false;
            (MapMember, long) currentFeature = CurrentFeature.Value;
            MapView activeMap = MapView.Active;
            activeMap.Map.SetSelection(SelectionSet.FromDictionary(
                new Dictionary<MapMember, List<long>>{
                        { currentFeature.Item1, new List<long>() {currentFeature.Item2} }
                })
            );
            return true;
        }

        public static void ZoomToSelected()
        {
            if (MapView.Active.Map.SelectionCount == 0 || CurrentFeature == null)
                return;
            MapView.Active.ZoomToSelected();
            Camera mapCamera = MapView.Active.Camera;
            mapCamera.Scale *= 1.5;
            MapView.Active.ZoomTo(mapCamera, TimeSpan.Zero);
        }

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            return true;
        }

        #endregion Overrides

        #region Toggle States
        /// <summary>
        /// Activate  the specified state. State is identified via
        /// its name. Listen for state changes via the DAML <b>condition</b> attribute
        /// </summary>
        /// <param name="stateID"></param>
        public static void ActivateState(string stateID)
        {
            if ( !(FrameworkApplication.State.Contains(stateID)) )
            {
                FrameworkApplication.State.Activate(stateID);
            }
        }
        /// <summary>
        /// Activate or Deactivate the specified state. State is identified via
        /// its name. Listen for state changes via the DAML <b>condition</b> attribute
        /// </summary>
        /// <param name="stateID"></param>
        public static void DeactivateState(string stateID)
        {
            if (FrameworkApplication.State.Contains(stateID))
            {
                FrameworkApplication.State.Deactivate(stateID);
            }
        }
        #endregion Toggle State

    }
}
