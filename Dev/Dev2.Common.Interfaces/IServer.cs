
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Toolbox;
using Newtonsoft.Json;

namespace Dev2.Common.Interfaces
{
    public interface IServer:IResource
    {
        //Task<bool> Connect();
        List<IResource> Load();
        Task<IExplorerItem> LoadExplorer();
        IList<IServer> GetServerConnections();
        IList<IToolDescriptor> LoadTools();
        [JsonIgnore]
        IExplorerRepository ExplorerRepository { get; }
        [JsonIgnore]
        IStudioUpdateManager UpdateRepository { get; }
        [JsonIgnore]
        IQueryManager QueryProxy { get; }
        bool IsConnected{get;}
        bool AllowEdit { get; }
        void ReloadTools();
        void Disconnect();
        void Edit();
        List<IWindowsGroupPermission> Permissions { get; set; }
        Guid EnvironmentID { get; set; }
        Guid? ServerID { get; }
        event PermissionsChanged PermissionsChanged;
        event NetworkStateChanged NetworkStateChanged;
        event ItemAddedEvent ItemAddedEvent;

        string GetServerVersion();
        string GetServerInformationalVersion();

        void Connect();
        Task<bool> ConnectAsync();
        string DisplayName { get; set; }
        bool HasLoaded { get;  }
        bool CanDeployTo { get; }
        bool CanDeployFrom { get; }

        IServer Clone();

        string GetMinSupportedVersion();
    }

    public delegate void PermissionsChanged(PermissionsChangedArgs args);

    public class PermissionsChangedArgs
    {
        public List<IWindowsGroupPermission> WindowsGroupPermissions { get; set; }

        public PermissionsChangedArgs(List<IWindowsGroupPermission> windowsGroupPermissions)
        {
            WindowsGroupPermissions = windowsGroupPermissions;
        }
    }

    public delegate void NetworkStateChanged(INetworkStateChangedEventArgs args,IServer server);
    public delegate void ItemAddedEvent(IExplorerItem args);
}