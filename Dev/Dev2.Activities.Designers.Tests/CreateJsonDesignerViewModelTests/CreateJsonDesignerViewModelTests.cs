using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.CreateJSON;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable InconsistentNaming
namespace Dev2.Activities.Designers.Tests.CreateJsonDesignerViewModelTests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class CreateJsonDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CreateJsonDesignerViewModel_Construct")]
        public void CreateJsonDesignerViewModel_Construct_IsInstanceOfActivityViewModelBase_True()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var createJsonDesignerViewModel = CreateViewModel();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(createJsonDesignerViewModel, typeof(ActivityDesignerViewModel));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CreateJsonDesignerViewModel_Construct")]
        public void CreateJsonDesignerViewModel_Construct_IsInstanceOfActivityCollectionViewModelBaseOfJsonMappingTo_True()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var createJsonDesignerViewModel = CreateViewModel();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(createJsonDesignerViewModel, typeof(ActivityCollectionDesignerViewModel<JsonMappingTo>));
        }

        [TestMethod]
        [TestCategory("CreateJsonDesignerViewModel_Constructor")]
        [Owner("Hagashen Naidu")]
        public void CreateJsonDesignerViewModel_Constructor_CollectionNameInitialized()
        {
            //------------Setup for test--------------------------
            const string ExpectedCollectionName = "JsonMappings";
            var mockModel = new Mock<ModelItem>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            mockModel.Setup(s => s.Properties).Returns(propertyCollection.Object);

            //------------Execute Test---------------------------
            var vm = CreateViewModel();

            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedCollectionName, vm.CollectionName, "Collection Name not initialized on Multi Assign load");
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CreateJsonDesignerViewModel_ErrorsProperty")]
        public void CreateJsonDesignerViewModel_ErrorsProperty_Constructor_IsNull()
        {
            //------------Setup for test--------------------------
            var createJsonDesignerViewModel = CreateViewModel();
            //------------Execute Test---------------------------
            var errorInfos = createJsonDesignerViewModel.Errors;
            //------------Assert Results-------------------------
            Assert.IsNull(errorInfos);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CreateJsonDesignerViewModel_CustomAction")]
        public void CreateJsonDesignerViewModel_CustomAction_WhenSourceNameChangedToScalar_UpdatesDestinationNameScalarName()
        {
            //------------Setup for test--------------------------
            var jsonMappingTo = new JsonMappingTo();
            var act = new DsfCreateJsonActivity { JsonMappings = new List<JsonMappingTo> { jsonMappingTo } };
            var vm = CreateViewModel(act);
            var mi = vm.ModelItemCollection[0];
            vm.CurrentModelItem = mi;
            //------------Execute Test---------------------------
            jsonMappingTo.SourceName = "[[var]]";
            //------------Assert Results-------------------------
            Assert.AreEqual("var",mi.GetProperty<string>("DestinationName"));
            Assert.AreEqual("var",jsonMappingTo.DestinationName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CreateJsonDesignerViewModel_CustomAction")]
        public void CreateJsonDesignerViewModel_CustomAction_WhenSourceNameChangedToRecsetWithField_UpdatesDestinationNameRecsetName()
        {
            //------------Setup for test--------------------------
            var jsonMappingTo = new JsonMappingTo();
            var act = new DsfCreateJsonActivity { JsonMappings = new List<JsonMappingTo> { jsonMappingTo } };
            var vm = CreateViewModel(act);
            var mi = vm.ModelItemCollection[0];
            vm.CurrentModelItem = mi;
            //------------Execute Test---------------------------
            jsonMappingTo.SourceName = "[[rec().set]]";
            //------------Assert Results-------------------------
            Assert.AreEqual("rec",mi.GetProperty<string>("DestinationName"));
            Assert.AreEqual("rec", jsonMappingTo.DestinationName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CreateJsonDesignerViewModel_CustomAction")]
        public void CreateJsonDesignerViewModel_CustomAction_WhenSourceNameChangedToRecsetNoField_UpdatesDestinationNameRecsetName()
        {
            //------------Setup for test--------------------------
            var jsonMappingTo = new JsonMappingTo();
            var act = new DsfCreateJsonActivity { JsonMappings = new List<JsonMappingTo> { jsonMappingTo } };
            var vm = CreateViewModel(act);
            var mi = vm.ModelItemCollection[0];
            vm.CurrentModelItem = mi;
            //------------Execute Test---------------------------
            jsonMappingTo.SourceName = "[[rec(*)]]";
            //------------Assert Results-------------------------
            Assert.AreEqual("rec",mi.GetProperty<string>("DestinationName"));
            Assert.AreEqual("rec", jsonMappingTo.DestinationName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CreateJsonDesignerViewModel_CustomAction")]
        public void CreateJsonDesignerViewModel_CustomAction_WhenSourceNameChangedToNonVariable_DoesNotUpdatesDestinationName()
        {
            //------------Setup for test--------------------------
            var jsonMappingTo = new JsonMappingTo();
            var act = new DsfCreateJsonActivity { JsonMappings = new List<JsonMappingTo> { jsonMappingTo } };
            var vm = CreateViewModel(act);
            var mi = vm.ModelItemCollection[0];
            vm.CurrentModelItem = mi;
            //------------Execute Test---------------------------
            jsonMappingTo.SourceName = "rec(*)";
            //------------Assert Results-------------------------
            Assert.IsTrue(string.IsNullOrEmpty(jsonMappingTo.DestinationName));
            Assert.IsTrue(string.IsNullOrEmpty(mi.GetProperty<string>("DestinationName")));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("CreateJsonDesignerViewModel_CustomAction")]
        public void CreateJsonDesignerViewModel_CustomAction_WhenSourceNameChangedToVariableButDestinationAlreadyPopulated_DoesNotUpdatesDestinationName()
        {
            //------------Setup for test--------------------------
            var jsonMappingTo = new JsonMappingTo();
            var act = new DsfCreateJsonActivity { JsonMappings = new List<JsonMappingTo> { jsonMappingTo } };
            var vm = CreateViewModel(act);
            var mi = vm.ModelItemCollection[0];
            vm.CurrentModelItem = mi;
            jsonMappingTo.SourceName = "[[rec(*)]]";
            //------------Assert Preconditions-------------------
            Assert.AreEqual("rec", mi.GetProperty<string>("DestinationName"));
            Assert.AreEqual("rec", jsonMappingTo.DestinationName);
            //------------Execute Test---------------------------
            jsonMappingTo.SourceName = "[[a]]";
            //------------Assert Results-------------------------
            Assert.AreEqual("rec", mi.GetProperty<string>("DestinationName"));
            Assert.AreEqual("rec", jsonMappingTo.DestinationName);
        }

        public static CreateJsonDesignerViewModel CreateViewModel()
        {
            return CreateViewModel(new DsfCreateJsonActivity());
        }
        
        public static CreateJsonDesignerViewModel CreateViewModel(DsfCreateJsonActivity activity)
        {
            var createJsonDesignerViewModel = new CreateJsonDesignerViewModel(ModelItemUtils.CreateModelItem(activity));
            return createJsonDesignerViewModel;
        }
    }
}