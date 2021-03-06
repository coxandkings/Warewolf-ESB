﻿using System;
using System.Activities.Presentation.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.Extensions;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Runtime.Hosting;
using Dev2.Services.Events;
using Dev2.Studio.Core.Messages;
// ReSharper disable ClassWithVirtualMembersNeverInherited.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ExplicitCallerInfoArgument

namespace Dev2.Activities.Designers2.DropBox2016.Delete
{
    public class DropBoxDeleteViewModel : FileActivityDesignerViewModel, INotifyPropertyChanged
    {
        private ObservableCollection<DropBoxSource> _sources;
        private readonly IEventAggregator _eventPublisher;
        private readonly IDropboxSourceManager _sourceManager;
        private string _deletePath;
        private string _result;

        [ExcludeFromCodeCoverage]
        // ReSharper disable once UnusedMember.Global
        public DropBoxDeleteViewModel(ModelItem modelItem)
            : this(modelItem, EventPublishers.Aggregator, new DropboxSourceManager())
        {
            this.RunViewSetup();
        }

        public DropBoxDeleteViewModel(ModelItem modelItem, IEventAggregator eventPublisher, IDropboxSourceManager sourceManager)
            : base(modelItem, "File Or Folder", String.Empty)
        {
            _eventPublisher = eventPublisher;
            _sourceManager = sourceManager;
            ThumbVisibility = Visibility.Visible;
            EditDropboxSourceCommand = new RelayCommand(o => EditDropBoxSource(), p => IsDropboxSourceSelected);
            NewSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CreateOAuthSource);
            // ReSharper disable once VirtualMemberCallInContructor
            // ReSharper disable once VirtualMemberCallInContructor
            Sources = LoadOAuthSources();
            AddTitleBarLargeToggle();
            EditDropboxSourceCommand.RaiseCanExecuteChanged();
        }

        public ICommand NewSourceCommand { get; set; }
        public IResourceCatalog ResourceManager
        {
            get
            {
                return ResourceCatalog.Instance;
            }
        }
        public DropBoxSource SelectedSource
        {
            get
            {
                var oauthSource = GetProperty<DropBoxSource>();
                return oauthSource ?? GetProperty<DropBoxSource>();
            }
            // ReSharper disable once ExplicitCallerInfoArgument
            set
            {
                SetProperty(value);
                EditDropboxSourceCommand.RaiseCanExecuteChanged();
                OnPropertyChanged("IsDropboxSourceSelected");
                // ReSharper disable once RedundantArgumentDefaultValue
                OnPropertyChanged("SelectedSource");
            }
        }

        public virtual ObservableCollection<DropBoxSource> Sources
        {
            get
            {
                return _sources;
            }
            set
            {
                SetProperty(value);
                _sources = value;
                // ReSharper disable once RedundantArgumentDefaultValue
                OnPropertyChanged("Sources");
            }
        }

        public RelayCommand EditDropboxSourceCommand { get; private set; }
        public bool IsDropboxSourceSelected
        {
            get
            {
                return SelectedSource != null;
            }
        }
        public string DeletePath
        {
            get
            {
                _deletePath = GetProperty<string>();
                return _deletePath;
            }
            set
            {
                _deletePath = value;
                SetProperty(value);
                OnPropertyChanged();
            }
        }
        public string Result
        {
            get
            {
                _result = GetProperty<string>();
                return _result;
            }
            set
            {
                _result = value;
                SetProperty(value);
                OnPropertyChanged();
            }
        }

        private void EditDropBoxSource()
        {
            CustomContainer.Get<IShellViewModel>().OpenResource(SelectedSource.ResourceID, CustomContainer.Get<IShellViewModel>().ActiveServer);
        }

        public void CreateOAuthSource()
        {
            _eventPublisher.Publish(new ShowNewResourceWizard("DropboxSource"));
            Sources = LoadOAuthSources();
            OnPropertyChanged("Sources");
        }


        #region Overrides of ActivityDesignerViewModel
        [ExcludeFromCodeCoverage]
        public override void Validate()
        {
        }
        [ExcludeFromCodeCoverage]
        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            if (mainViewModel != null)
            {
                mainViewModel.HelpViewModel.UpdateHelpText(helpText);
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        [ExcludeFromCodeCoverage]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<DropBoxSource> LoadOAuthSources()
        {
            Sources = _sourceManager.FetchSources<DropBoxSource>().ToObservableCollection();
            return Sources;
        }
    }
}
