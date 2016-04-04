﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Exchange;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;

namespace Warewolf.Studio.ViewModels
{
    public class ExchangeServiceModel : IExchangeServiceModel
    {
        readonly IStudioUpdateManager _updateRepository;
        readonly IQueryManager _queryProxy;
        readonly IShellViewModel _shell;

        public ExchangeServiceModel(IStudioUpdateManager updateRepository, IQueryManager queryProxy, IShellViewModel shell, IServer server)
        {
            _updateRepository = updateRepository;
            _queryProxy = queryProxy;
            _shell = shell;
            shell.SetActiveServer(server);
        }
        public ObservableCollection<IExchangeSource> RetrieveSources()
        {
            return new ObservableCollection<IExchangeSource>(_queryProxy.FetchExchangeSources());
        }

        public void CreateNewSource()
        {
            _shell.NewResource(ResourceType.ExchangeSource.ToString(), "");
        }

        public void EditSource(IExchangeSource selectedSource)
        {
           
        }

        public string TestService(IExchangeService inputValues)
        {
            //return _updateRepository.TestDbService(inputValues);
            return "Ok";
        }

        public IEnumerable<IServiceOutputMapping> GetPluginOutputMappings(IExchangeSource action)
        {
            return new List<IServiceOutputMapping>();
        }

        public void SaveService(IExchangeSource model)
        {
            _updateRepository.Save(model);
        }

        public IStudioUpdateManager UpdateRepository
        {
            get
            {
                return _updateRepository;
            }
        }
    }
}