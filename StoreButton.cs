﻿using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.KnowledgeGraph;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycleSelection
{
    internal class StoreButton : Button
    {
        protected override void OnClick()
        {
            QueuedTask.Run(() =>
            {
                MapView activeMap = MapView.Active;
                if (activeMap == null )
                    return;
                Module1.SelectionSet = activeMap.Map.GetSelection();

                Dictionary<MapMember, List<long>> selection = Module1.SelectionSet.ToDictionary();
                List<(MapMember, long)> selectionList = selection.SelectMany( kvp => kvp.Value.Select( oid => (kvp.Key, oid) ) ).ToList();
                if (selectionList.Count > 0)
                {
                    Module1.SelectedFeatures = selectionList;
                    Module1.ActivateState("CycleSelection_StoredSelection");
                }
                else
                    Module1.ClearFeatures();
            });
        }
    }
}
