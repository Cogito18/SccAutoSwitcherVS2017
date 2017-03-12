using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace SccAutoSwitcherVS2017
{
    public class SwitcherOptions : DialogPage
    {
        [DisplayName("Git Provider")]
        [Category("Scc Providers")]
        public GitSccProvider GitProvider
        {
            get { return SccAutoSwitcherVS2017.GetGitSccProvider(); }
            set { SccAutoSwitcherVS2017.SetGitSccProvider(value); }
        }

        [DisplayName("Subversion Provider")]
        [Category("Scc Providers")]
        public SubversionSccProvider SubversionProvider
        {
            get { return SccAutoSwitcherVS2017.GetSubversionSccProvider(); }
            set { SccAutoSwitcherVS2017.SetSubversionSccProvider(value); }
        }

        [DisplayName("Mercurial Provider")]
        [Category("Scc Providers")]
        public MercurialSccProvider MercurialProvider
        {
            get { return SccAutoSwitcherVS2017.GetMercurialSccProvider(); }
            set { SccAutoSwitcherVS2017.SetMercurialSccProvider(value); }
        }

        [DisplayName("Perforce Provider")]
        [Category("Scc Providers")]
        public PerforceSccProvider PerforceProvider
        {
            get { return SccAutoSwitcherVS2017.GetPerforceSccProvider(); }
            set { SccAutoSwitcherVS2017.SetPerforceSccProvider(value); }
        }
    }

    public enum GitSccProvider
    {
        Default = 0,

        [Description("Git Source Control Provider")]
        GitSourceControlProvider,

        [Description("Visual Studio Tools for Git")]
        VisualStudioToolsForGit,

        Disabled
    }

    public enum SubversionSccProvider
    {
        Default = 0,

        [Description("VisualSVN")]
        VisualSVN,

        [Description("AnkhSVN")]
        AnkhSVN,

        Disabled
    }

    public enum MercurialSccProvider
    {
        Default = 0,

        [Description("HgSccPackage")]
        HgSccPackage,

        [Description("VisualHG")]
        VisualHG,

        Disabled
    }

    public enum PerforceSccProvider
    {
        Default = 0,

        [Description("P4VS")]
        P4VS,

        Disabled
    }
}