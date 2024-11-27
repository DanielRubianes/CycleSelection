using ArcGIS.Core.CIM;
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
    internal class RemoveCurrentButton : Button
    {
        protected override void OnClick()
        {
            QueuedTask.Run(() =>
            {
                Module1.RemoveCurrentFeature();
                if (Module1.CurrentFeature == null)
                    return;
                (MapMember, long) currentFeature = Module1.CurrentFeature.Value;
                MapView activeMap = MapView.Active;
                activeMap.Map.SetSelection(SelectionSet.FromDictionary(
                    new Dictionary<MapMember, List<long>>{
                        { currentFeature.Item1, new List<long>() {currentFeature.Item2} }
                    })
                );
                Module1.ZoomToSelected();
            });
        }
    }
}