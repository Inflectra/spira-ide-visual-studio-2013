// Guids.cs
// MUST match guids.h
using System;

namespace Inflectra.SpiraTest.IDEIntegration.VisualStudio2012
{
    static class GuidList
    {
        public const string guidSpiraExplorerPkgString = "6fdddea3-588b-4fe1-acb7-9c5b03bccbad";
        public const string guidSpiraExplorerCmdSetString = "37778466-feef-4cea-bcb4-6c7a0a7c252b";
        public const string guidToolWindowPersistanceString = "cfe746fb-e114-49da-8cb5-d2f8a1b0274e";
        public static readonly Guid guidSpiraExplorerCmdSet = new Guid(guidSpiraExplorerCmdSetString);
    };
}