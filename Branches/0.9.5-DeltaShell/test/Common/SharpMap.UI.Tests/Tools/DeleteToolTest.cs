using DelftTools.Utils.Editing;
using GeoAPI.Extensions.Feature;
using GisSharpBlog.NetTopologySuite.Geometries;
using NetTopologySuite.Extensions.Features;
using NUnit.Framework;
using Rhino.Mocks;
using SharpMap.Api;
using SharpMap.Api.Editors;
using SharpMap.Data.Providers;
using SharpMap.Editors;
using SharpMap.Layers;
using SharpMap.UI.Forms;
using SharpMap.UI.Tools;

namespace SharpMap.UI.Tests.Tools
{
    [TestFixture]
    public class DeleteToolTest
    {
        private static readonly MockRepository mocks = new MockRepository();

        [Test]
        public void CanDeleteWithoutEditableObject()
        {
            MapControl mapControl = new MapControl();

            //mapControl.ActivateTool(selectTool);

            VectorLayer vectorLayer = new VectorLayer();
            FeatureCollection layer2Data = new FeatureCollection();
            vectorLayer.DataSource = layer2Data;
            layer2Data.FeatureType = typeof(Feature);
            
            layer2Data.Add(new Point(4, 5));
            layer2Data.Add(new Point(0, 1));
            mapControl.Map.Layers.Add(vectorLayer);

            mapControl.SelectTool.Select((IFeature)layer2Data.Features[0]);

            mapControl.DeleteTool.DeleteSelection();
        }

        [Test]
        public void NopDeleteDoesNotTriggerBeginEdit()
        {
            MapControl mapControl = new MapControl();

            SelectTool selectTool = mapControl.SelectTool;

            VectorLayer vectorLayer = new VectorLayer();
            FeatureCollection layer2Data = new FeatureCollection();
            vectorLayer.DataSource = layer2Data;
            layer2Data.FeatureType = typeof(Feature);

            layer2Data.Add(new Point(4, 5));
            layer2Data.Add(new Point(0, 1));
            mapControl.Map.Layers.Add(vectorLayer);

            var featureMutator = mocks.StrictMock<IFeatureInteractor>();
            var editableObject = mocks.StrictMock<IEditableObject>();

            featureMutator.Expect(fm => fm.EditableObject).Return(editableObject).Repeat.Any();
            featureMutator.Expect(fm => fm.AllowDeletion()).Return(false).Repeat.Any();
            featureMutator.Expect(fm => fm.Delete()).Repeat.Never();
            editableObject.Expect(eo => eo.BeginEdit(null)).IgnoreArguments().Repeat.Never(); //never expect BeginEdit!
            editableObject.Expect(eo => eo.EndEdit()).IgnoreArguments().Repeat.Never();
            
            mocks.ReplayAll();
            
            selectTool.Select((IFeature)layer2Data.Features[0]);

            selectTool.SelectedFeatureInteractors.Clear();
            selectTool.SelectedFeatureInteractors.Add(featureMutator); //inject our own feature editor

            mapControl.DeleteTool.DeleteSelection();

            mocks.VerifyAll();
        }

        [Test]
        public void ActualDeleteTriggersBeginEdit()
        {
            MapControl mapControl = new MapControl();

            SelectTool selectTool = mapControl.SelectTool;

            VectorLayer vectorLayer = new VectorLayer();
            FeatureCollection layer2Data = new FeatureCollection();
            vectorLayer.DataSource = layer2Data;
            layer2Data.FeatureType = typeof(Feature);

            layer2Data.Add(new Point(4, 5));
            layer2Data.Add(new Point(0, 1));
            mapControl.Map.Layers.Add(vectorLayer);

            var featureMutator = mocks.StrictMock<IFeatureInteractor>();
            var editableObject = mocks.StrictMock<IEditableObject>();

            featureMutator.Expect(fm => fm.EditableObject).Return(editableObject).Repeat.Any();
            featureMutator.Expect(fm => fm.AllowDeletion()).Return(true).Repeat.Any();
            featureMutator.Expect(fm => fm.Delete()).Repeat.Once();
            editableObject.Expect(eo => eo.BeginEdit(null)).IgnoreArguments().Repeat.Once(); //expect BeginEdit!
            editableObject.Expect(eo => eo.EndEdit()).IgnoreArguments().Repeat.Once();

            mocks.ReplayAll();

            selectTool.Select((IFeature)layer2Data.Features[0]);

            selectTool.SelectedFeatureInteractors.Clear();
            selectTool.SelectedFeatureInteractors.Add(featureMutator); //inject our own feature editor

            mapControl.DeleteTool.DeleteSelection();

            mocks.VerifyAll();
        }
    }
}