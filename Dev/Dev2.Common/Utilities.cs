using System;
using System.Security.Principal;

namespace Dev2.Common
{
    public static class Utilities
    {
        public static void PerformActionInsideImpersonatedContext(IPrincipal userPrinciple, Action actionToBePerformed)
        {
            if(userPrinciple == null)
            {
                actionToBePerformed();
            }
            else
            {
                WindowsIdentity identity = userPrinciple.Identity as WindowsIdentity;
                WindowsImpersonationContext impersonationContext = null;
                if(identity != null)
                {
                    if(identity.IsAnonymous)
                    {
                        identity = ServerUser.Identity as WindowsIdentity;
                    }
                    if(identity != null)
                    {
                        impersonationContext = identity.Impersonate();
                    }
                }
                try
                {
                    actionToBePerformed();
                }
                catch(Exception)
                {
                    if(impersonationContext != null)
                    {
                        impersonationContext.Undo();
                    }
                    identity = ServerUser.Identity as WindowsIdentity;
                    if(identity != null)
                    {
                        impersonationContext = identity.Impersonate();
                    }
                    try
                    {
                        actionToBePerformed();
                    }
                    catch(Exception)
                    {
                        //Ignore
                    }
                }
                finally
                {
                    if(impersonationContext != null)
                    {
                        impersonationContext.Undo();
                    }
                }
            }
        }

        public static IPrincipal ServerUser
        {
            get;
            set;
        }
    }
}