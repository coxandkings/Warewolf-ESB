/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Communication;
using Dev2.Services.Security;
using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Web;

namespace Dev2.Runtime.Security
{
    public class ServerAuthorizationService : AuthorizationServiceBase
    {
        private readonly ConcurrentDictionary<Tuple<string, string>, Tuple<bool, DateTime>> _cachedRequests = new ConcurrentDictionary<Tuple<string, string>, Tuple<bool, DateTime>>();

        // Singleton instance - lazy initialization is used to ensure that the creation is thread-safe
        private static readonly Lazy<ServerAuthorizationService> TheInstance = new Lazy<ServerAuthorizationService>(() => new ServerAuthorizationService(new ServerSecurityService()));

        public static IAuthorizationService Instance { get { return TheInstance.Value; } }

        private readonly TimeSpan _timeOutPeriod;
        private readonly IPerformanceCounter _perfCounter;

        protected ServerAuthorizationService(ISecurityService securityService)
            : base(securityService, true)
        {
            _timeOutPeriod = securityService.TimeOutPeriod;
            try
            {
                _perfCounter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter("Count of Not Authorised errors");
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e);
            }
        }

        public int CachedRequestCount { get { return _cachedRequests.Count; } }

        public override bool IsAuthorized(AuthorizationContext context, string resource)
        {
            bool authorized;

            VerifyArgument.IsNotNull("resource", resource);

            Tuple<string, string> requestKey = new Tuple<string, string>(ClaimsPrincipal.Current.Identity.Name, resource);
            Tuple<bool, DateTime> authorizedRequest;
            if (_cachedRequests.TryGetValue(requestKey, out authorizedRequest) && DateTime.Now.Subtract(authorizedRequest.Item2) < _timeOutPeriod)
            {
                authorized = authorizedRequest.Item1;
            }
            else
            {
                authorized = IsAuthorized(ClaimsPrincipal.Current, context, resource);
            }

            if (!authorized) // TODO : Do we need to check pending request for user?
            {
                if (ResultsCache.Instance.ContainsPendingRequestForUser(ClaimsPrincipal.Current.Identity.Name))
                {
                    authorized = true;
                }
            }
            else
            {
                authorizedRequest = new Tuple<bool, DateTime>(authorized, DateTime.Now);
                _cachedRequests.AddOrUpdate(requestKey, authorizedRequest, (tuple, tuple1) => authorizedRequest);
            }

            if (!authorized)
            {
                if (_perfCounter != null)
                {
                    _perfCounter.Increment();
                }
            }
            return authorized;
        }

        public override bool IsAuthorized(IAuthorizationRequest request)
        {
            VerifyArgument.IsNotNull("request", request);
            bool authorized;
            Tuple<bool, DateTime> authorizedRequest;
            if (_cachedRequests.TryGetValue(request.Key, out authorizedRequest) && DateTime.Now.Subtract(authorizedRequest.Item2) < _timeOutPeriod)
            {
                authorized = authorizedRequest.Item1;
            }
            else
            {
                authorized = IsAuthorizedImpl(request);
            }

            // Only in the case when permissions change and we need to still fetch results ;)
            if (!authorized && (request.RequestType == WebServerRequestType.HubConnect || request.RequestType == WebServerRequestType.EsbFetchExecutePayloadFragment))
            {
                // TODO : Check that the ResultsCache contains data to fetch for the user ;)
                var identity = request.User.Identity;
                if (ResultsCache.Instance.ContainsPendingRequestForUser(identity.Name))
                {
                    authorized = true;
                }
            }
            else
            {
                // normal execution
                authorizedRequest = new Tuple<bool, DateTime>(authorized, DateTime.Now);
                _cachedRequests.AddOrUpdate(request.Key, authorizedRequest, (tuple, tuple1) => authorizedRequest);
            }
            if (!authorized)
            {
                if (_perfCounter != null)
                {
                    _perfCounter.Increment();
                }
            }
            return authorized;
        }

        private bool IsAuthorizedImpl(IAuthorizationRequest request)
        {
            var result = false;
            switch (request.RequestType)
            {
                case WebServerRequestType.WebGetDecisions:
                case WebServerRequestType.WebGetDialogs:
                case WebServerRequestType.WebGetServices:
                case WebServerRequestType.WebGetSources:
                case WebServerRequestType.WebGetSwitch:
                    result = IsAuthorized(request.User, AuthorizationContext.View, GetResource(request));
                    break;

                case WebServerRequestType.WebGet:
                case WebServerRequestType.WebGetContent:
                case WebServerRequestType.WebGetImage:
                case WebServerRequestType.WebGetScript:
                case WebServerRequestType.WebGetView:
                    result = IsAuthorized(request.User, AuthorizationContext.Any, GetResource(request));
                    break;

                case WebServerRequestType.WebInvokeService:
                    var authorizationContext = IsWebInvokeServiceSave(request.Url.AbsolutePath) ? AuthorizationContext.Contribute : AuthorizationContext.View;
                    result = IsAuthorized(request.User, authorizationContext, GetResource(request));
                    break;

                case WebServerRequestType.WebExecuteService:
                case WebServerRequestType.WebExecuteSecureWorkflow:
                case WebServerRequestType.WebExecutePublicWorkflow:
                case WebServerRequestType.WebBookmarkWorkflow:
                    result = IsAuthorized(request.User, AuthorizationContext.Execute, GetResource(request));
                    break;

                case WebServerRequestType.WebExecuteInternalService:
                    result = IsAuthorized(request.User, AuthorizationContext.Any, GetResource(request));
                    break;

                case WebServerRequestType.HubConnect:
                    result = IsAuthorizedToConnect(request.User);
                    break;

                case WebServerRequestType.WebExecuteGetLogFile:
                case WebServerRequestType.EsbSendMemo:
                case WebServerRequestType.EsbAddDebugWriter:
                case WebServerRequestType.EsbExecuteCommand:
                case WebServerRequestType.EsbSendDebugState:
                case WebServerRequestType.EsbWrite:
                case WebServerRequestType.EsbOnConnected:
                case WebServerRequestType.EsbFetchExecutePayloadFragment:
                case WebServerRequestType.ResourcesSendMemo:
                case WebServerRequestType.WebExecuteGetRootLevelApisJson:
                case WebServerRequestType.WebExecuteGetApisJsonForFolder:
                    result = IsAuthorizedToConnect(request.User);
                    break;
            }

            if (!result)
            {
                var user = "NULL USER";
                // ReSharper disable ConditionIsAlwaysTrueOrFalse

                if (request.User.Identity != null)
                // ReSharper restore ConditionIsAlwaysTrueOrFalse
                {
                    user = request.User.Identity.Name;
                    DumpPermissionsOnError(request.User);
                }

                // ReSharper disable InvokeAsExtensionMethod
                Dev2Logger.Error("AUTH ERROR FOR USER : " + user);
                // ReSharper restore InvokeAsExtensionMethod
            }

            return result;
        }

        private static string GetResource(IAuthorizationRequest request)
        {
            var resource = request.QueryString["rid"];
            if (string.IsNullOrEmpty(resource))
            {
                switch (request.RequestType)
                {
                    case WebServerRequestType.WebExecuteService:
                        resource = GetWebExecuteName(request.Url.AbsolutePath);
                        break;

                    case WebServerRequestType.WebBookmarkWorkflow:
                        resource = GetWebBookmarkName(request.Url.AbsolutePath);
                        break;

                    case WebServerRequestType.WebExecuteInternalService:
                        resource = GetWebExecuteName(request.Url.AbsolutePath);
                        break;
                }
            }
            return string.IsNullOrEmpty(resource) ? null : resource;
        }

        protected override void RaisePermissionsChanged()
        {
            _cachedRequests.Clear();
            base.RaisePermissionsChanged();
        }

        private static string GetWebExecuteName(string absolutePath)
        {
            var startIndex = GetNameStartIndex(absolutePath);
            return startIndex.HasValue ? HttpUtility.UrlDecode(absolutePath.Substring(startIndex.Value, absolutePath.Length - startIndex.Value)) : null;
        }

        private static string GetWebBookmarkName(string absolutePath)
        {
            var startIndex = GetNameStartIndex(absolutePath);
            if (startIndex.HasValue)
            {
                var endIndex = absolutePath.IndexOf("/instances/", startIndex.Value, StringComparison.InvariantCultureIgnoreCase);
                if (endIndex != -1)
                {
                    return HttpUtility.UrlDecode(absolutePath.Substring(startIndex.Value, endIndex - startIndex.Value));
                }
            }

            return null;
        }

        private static int? GetNameStartIndex(string absolutePath)
        {
            var startIndex = absolutePath.IndexOf("services/", StringComparison.InvariantCultureIgnoreCase);
            if (startIndex == -1)
            {
                return startIndex;
            }

            startIndex += 9;
            return startIndex;
        }

        private static bool IsWebInvokeServiceSave(string absolutePath)
        {
            return absolutePath.EndsWith("/save", StringComparison.InvariantCultureIgnoreCase);
        }

        protected override void OnDisposed()
        {
            if (SecurityService != null)
            {
                SecurityService.Dispose();
            }
        }
    }
}