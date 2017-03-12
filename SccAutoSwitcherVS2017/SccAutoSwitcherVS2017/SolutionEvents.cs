﻿using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System.IO;

namespace SccAutoSwitcherVS2017
{
    public partial class SccAutoSwitcherVS2017
    {
        public const string SVN_DIR = ".svn";
        public const string GIT_DIR = ".git";
        public const string MERCURIAL_DIR = ".hg";

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterClosingChildren(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterMergeSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpeningChildren(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeClosingChildren(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeOpeningChildren(IVsHierarchy pHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterBackgroundSolutionLoadComplete()
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProjectBatch(bool fIsBackgroundIdleBatch)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeBackgroundSolutionLoadBegins()
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLoadProjectBatch(bool fIsBackgroundIdleBatch)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeOpenSolution(string pszSolutionFilename)
        {
            DirectoryInfo currdir = new DirectoryInfo(Path.GetDirectoryName(pszSolutionFilename));

            _CurrentSolutionRcsType = RcsType.Unknown;
            while (true)
            {
                if (Directory.Exists(Path.Combine(currdir.FullName, SVN_DIR)))
                {
                    SccAutoSwitcherVS2017.RegisterPrimarySourceControlProvider(RcsType.Subversion);
                    _CurrentSolutionRcsType = RcsType.Subversion;
                    break;
                }

                if (Directory.Exists(Path.Combine(currdir.FullName, GIT_DIR)))
                {
                    SccAutoSwitcherVS2017.RegisterPrimarySourceControlProvider(RcsType.Git);
                    _CurrentSolutionRcsType = RcsType.Git;
                    break;
                }

                if (Directory.Exists(Path.Combine(currdir.FullName, MERCURIAL_DIR)))
                {
                    SccAutoSwitcherVS2017.RegisterPrimarySourceControlProvider(RcsType.Mercurial);
                    _CurrentSolutionRcsType = RcsType.Mercurial;
                    break;
                }

                if (currdir.Parent == null)
                    break;

                currdir = currdir.Parent;
            }

            return VSConstants.S_OK;
        }

        public int OnQueryBackgroundLoadProjectBatch(out bool pfShouldDelayLoadToNextIdle)
        {
            pfShouldDelayLoadToNextIdle = false;
            return VSConstants.S_OK;
        }
    }
}
