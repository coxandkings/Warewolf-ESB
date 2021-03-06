﻿using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Dev2.Activities.Designers2.Core.CloneInputRegion;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.WCF;
using Dev2.Studio.Core.Activities.Utils;
using Warewolf.Core;
// ReSharper disable ExplicitCallerInfoArgument

namespace Dev2.Activities.Designers2.Core.InputRegion
{
    public class WcfInputRegion : IWcfInputRegion
    {
        private readonly ModelItem _modelItem;
        private readonly IActionToolRegion<IWcfAction> _action;
        bool _isEnabled;
        // ReSharper disable once NotAccessedField.Local
        private IList<IServiceInput> _inputs;
        private bool _isInputsEmptyRows;

        public WcfInputRegion()
        {
            ToolRegionName = "WcfInputRegion";
        }

        public WcfInputRegion(ModelItem modelItem, IActionToolRegion<IWcfAction> action)
        {
            ToolRegionName = "WcfInputRegion";
            _modelItem = modelItem;
            _action = action;
            _action.SomethingChanged += SourceOnSomethingChanged;
            var inputsFromModel = _modelItem.GetProperty<List<IServiceInput>>("Inputs");
            Inputs = new List<IServiceInput>(inputsFromModel ?? new List<IServiceInput>());
            if (inputsFromModel == null)
            UpdateOnActionSelection();
            IsEnabled = action != null && action.SelectedAction != null;
        }

        private void SourceOnSomethingChanged(object sender, IToolRegion args)
        {
            // ReSharper disable once ExplicitCallerInfoArgument
            UpdateOnActionSelection();
            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged(@"Inputs");
            OnPropertyChanged(@"IsEnabled");
        }

        private void UpdateOnActionSelection()
        {
            Inputs = new List<IServiceInput>();
            IsEnabled = false;
            if (_action != null && _action.SelectedAction != null)
            {

                Inputs = _action.SelectedAction.Inputs;
                IsInputsEmptyRows = Inputs.Count < 1;
                IsEnabled = true;
            }
            OnPropertyChanged("Inputs");
        }

        public bool IsInputsEmptyRows
        {
            get
            {
                return _isInputsEmptyRows;
            }
            set
            {
                _isInputsEmptyRows = value;
                OnPropertyChanged();
            }
        }

        #region Implementation of IToolRegion

        public string ToolRegionName { get; set; }
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        public IList<IToolRegion> Dependants { get; set; }

        public IToolRegion CloneRegion()
        {

            var inputs2 = new List<IServiceInput>(Inputs.Select(a => new ServiceInput(a.Name, a.Value)
            {
                EmptyIsNull = a.EmptyIsNull,
                TypeName = a.TypeName
            }));
            return new WcfInputRegionClone()
            {
                Inputs = inputs2,
                IsEnabled = IsEnabled
            };
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
            var region = toRestore as WcfInputRegionClone;
            if (region != null)
            {
                Inputs.Clear();
                if (region.Inputs != null)
                {
                    var inp = region.Inputs.ToList();

                    Inputs = inp;
                }
                OnPropertyChanged("Inputs");
                IsInputsEmptyRows = Inputs == null || Inputs.Count == 0;
            }
        }

        public IList<string> Errors
        {
            get
            {
                IList<string> errors = new List<string>();
                return errors;
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #region Implementation of IDotNetInputRegion

        public IList<IServiceInput> Inputs
        {
            get
            {
                return _modelItem.GetProperty<List<IServiceInput>>("Inputs") ?? new List<IServiceInput>();
            }
            set
            {
                if (value.Count < 1)
                {

                }
                else
                {

                }
                _inputs = value;
                OnPropertyChanged();
                _modelItem.SetProperty("Inputs", value);
                OnPropertyChanged();
            }
        }

        #endregion
    }
}
