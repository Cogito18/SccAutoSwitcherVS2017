//------------------------------------------------------------------------------
// <copyright file="SccAutoSwitcherVS2017.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace SccAutoSwitcherVS2017
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [Guid(SccAutoSwitcherVS2017.PackageGuidString)]
    //[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(VSConstants.UICONTEXT.NoSolution_string)]       // Load if no solution
    [ProvideOptionPage(typeof(SwitcherOptions), "Scc Auto Switcher VS2017", "Scc Providers", 101, 106, true)] // Options dialog page
    public sealed partial class SccAutoSwitcherVS2017 : Package, IVsSolutionEvents3, IVsSolutionLoadEvents
    {
        /// <summary>
        /// SccAutoSwitcherVS2017 GUID string.
        /// </summary>
        public const string PackageGuidString = "781a2a9b-cd3d-4da2-b83a-ca47f20c0560";

        private static DTE2 _DTE2;

        private static IVsRegisterScciProvider _VsRegisterScciProvider;
        private static IVsShell _VsShell;
        private static WritableSettingsStore _SettingsStore;
        private static RcsType _CurrentSolutionRcsType;

        /// <summary>
        /// Initializes a new instance of the <see cref="SccAutoSwitcherVS2017"/> class.
        /// </summary>
        public SccAutoSwitcherVS2017()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            _CurrentSolutionRcsType = RcsType.Unknown;

            IVsExtensibility extensibility = GetService<IVsExtensibility>();
            _DTE2 = (DTE2)extensibility.GetGlobalsObject(null).DTE;

            IVsSolution solution = GetService<SVsSolution>() as IVsSolution;
            int hr;
            uint pdwCookie;
            hr = solution.AdviseSolutionEvents(this, out pdwCookie);
            Marshal.ThrowExceptionForHR(hr);

            _VsShell = GetService<SVsShell>() as IVsShell;
            _VsRegisterScciProvider = GetService<IVsRegisterScciProvider>();
            _SettingsStore = GetWritableSettingsStore();
        }

        public static void RegisterPrimarySourceControlProvider(RcsType rcsType)
        {
            int hr;
            Guid packageGuid = new Guid();
            Guid sccProviderGuid = new Guid();
            SccProvider providerToLoad = SccProvider.Unknown;
            bool enabled = false;

            switch (rcsType)
            {
                case RcsType.Subversion:
                    {
                        enabled = RegisterSubversionScc(out packageGuid, out sccProviderGuid, out providerToLoad);
                        break;
                    }
                case RcsType.Git:
                    {
                        enabled = RegisterGitScc(out packageGuid, out sccProviderGuid, out providerToLoad);
                        break;
                    }
                case RcsType.Mercurial:
                    {
                        enabled = RegisterMercurialScc(out packageGuid, out sccProviderGuid, out providerToLoad);
                        break;
                    }
            }

            if (!enabled)
                return;

            SccProvider currentSccProvider = GetCurrentSccProvider();
            if (providerToLoad == currentSccProvider)
                return;

            int installed;
            hr = _VsShell.IsPackageInstalled(ref packageGuid, out installed);
            Marshal.ThrowExceptionForHR(hr);
            if (installed == 0)
                return;

            hr = _VsRegisterScciProvider.RegisterSourceControlProvider(sccProviderGuid);
            Marshal.ThrowExceptionForHR(hr);
        }

        /// <returns>false if handling the scc provider is disabled for this Rcs type</returns>
        private static bool RegisterGitScc(out Guid packageGuid, out Guid sccProviderGuid, out SccProvider provider)
        {
            GitSccProvider gitProvider = GetGitSccProvider();

            if (gitProvider == GitSccProvider.Default)
                gitProvider = GetDefaultGitSccProvider();

            if (gitProvider == GitSccProvider.Disabled)
            {
                packageGuid = new Guid();
                sccProviderGuid = new Guid();
                provider = SccProvider.Unknown;
                return false;
            }

            switch (gitProvider)
            {
                case GitSccProvider.VisualStudioToolsForGit:
                    {
                        packageGuid = new Guid(VSToolsForGitPackagedId);
                        sccProviderGuid = new Guid(VSToolsForGitSccProviderId);
                        provider = SccProvider.VisualStudioToolsForGit;
                        return true;
                    }
                case GitSccProvider.GitSourceControlProvider:
                    {
                        packageGuid = new Guid(GitScpPackagedId);
                        sccProviderGuid = new Guid(GitScpSccProviderId);
                        provider = SccProvider.GitSourceControlProvider;
                        return true;
                    }
                default:
                    throw new Exception();
            }
        }

        /// <returns>false if handling the scc provider is disabled for this Rcs type</returns>
        private static bool RegisterSubversionScc(out Guid packageGuid, out Guid sccProviderGuid, out SccProvider provider)
        {
            SubversionSccProvider svnProvider = GetSubversionSccProvider();

            if (svnProvider == SubversionSccProvider.Default)
                svnProvider = GetDefaultSubversionSccProvider();

            if (svnProvider == SubversionSccProvider.Disabled)
            {
                packageGuid = new Guid();
                sccProviderGuid = new Guid();
                provider = SccProvider.Unknown;
                return false;
            }

            switch (svnProvider)
            {
                case SubversionSccProvider.AnkhSVN:
                    {
                        packageGuid = new Guid(AnkhSvnPackageId);
                        sccProviderGuid = new Guid(AnkhSvnSccProviderId);
                        provider = SccProvider.AnkhSvn;
                        return true;
                    }
                case SubversionSccProvider.VisualSVN:
                    {
                        packageGuid = new Guid(VisualSvnPackageId);
                        sccProviderGuid = new Guid(VisualSvnSccProviderId);
                        provider = SccProvider.VisualSVN;
                        return true;
                    }
                default:
                    throw new Exception();
            }
        }

        /// <returns>false if handling the scc provider is disabled for this Rcs type</returns>
        private static bool RegisterMercurialScc(out Guid packageGuid, out Guid sccProviderGuid, out SccProvider provider)
        {
            MercurialSccProvider mercurialProvider = GetMercurialSccProvider();

            if (mercurialProvider == MercurialSccProvider.Default)
                mercurialProvider = GetDefaultMercurialSccProvider();

            if (mercurialProvider == MercurialSccProvider.Disabled)
            {
                packageGuid = new Guid();
                sccProviderGuid = new Guid();
                provider = SccProvider.Unknown;
                return false;
            }

            switch (mercurialProvider)
            {
                case MercurialSccProvider.HgSccPackage:
                    {
                        packageGuid = new Guid(HgSccPackagePackageId);
                        sccProviderGuid = new Guid(HgSccPackageSccProviderId);
                        provider = SccProvider.HgSccPackage;
                        return true;
                    }
                case MercurialSccProvider.VisualHG:
                    {
                        packageGuid = new Guid(VisualHGPackageId);
                        sccProviderGuid = new Guid(VisualHGSccProviderId);
                        provider = SccProvider.VisualHG;
                        return true;
                    }
                default:
                    throw new Exception();
            }
        }

        private static string GetRegUserSettingsPath()
        {
            string version = _DTE2.Version;
            string suffix = GetSuffix(_DTE2.CommandLineArguments);
            return @"Software\Microsoft\VisualStudio\" + version + suffix;
        }

        private static string GetSuffix(string args)
        {
            string[] tokens = args.Split(' ', '\t');
            int foundIndex = -1;
            int it = 0;
            foreach (string str in tokens)
            {
                if (str.Equals("/RootSuffix", StringComparison.InvariantCultureIgnoreCase))
                {
                    foundIndex = it + 1;
                    break;
                }

                it++;
            }

            if (foundIndex == -1 || foundIndex >= tokens.Length)
                return String.Empty;

            return tokens[foundIndex];
        }

        public WritableSettingsStore GetWritableSettingsStore()
        {
            var shellSettingsManager = new ShellSettingsManager(this);
            return shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
        }

        private void GetService<T>(out T service)
        {
            service = (T)GetService(typeof(T));
        }

        private T GetService<T>()
        {
            return (T)GetService(typeof(T));
        }

        public static DTE2 DTE2
        {
            get { return _DTE2; }
        }

        #endregion

    }
}
