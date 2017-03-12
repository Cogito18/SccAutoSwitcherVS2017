# SccAutoSwitcherVS2017

A Visual Studio extension that auto loads Scc providers
depending on presence of reserved repository file
or directories.
Supported Scc providers are:

 * AnkhSVN *(Subversion, default)*;
 * VisualSVN *(Subversion)*;
 * Visual Studio Tools for Git *(Git, default)*;
 * Git Source Control Provider *(Git)*;
 * HgSccPackage *(mercurial, default)*;
 * VisualHG *(mercurial)*.
 
More providers can be added, provided they are regular
Scc providers and there exists an easy way to detect
proper RCS type by checking file or directories presence
starting from solution root directory.

It supports Visual Studio 2017. License is MIT.

Based on SccAutoSwitcher by Francesco Pretto (https://github.com/ceztko/SccAutoSwitcher).

### Options dialog page

SccAutoSwitcher offers a very simple options dialog page
allowing to change the default Scc provider loading
priority for different RCS types. To explain allowed
values, let's take Suversion Scc providers as an example:

* **Default**: means AnkhSvn is loaded first, if it's
  found, otherwise the latter is loaded.
* **AnkhSvn**: will always try to load AnkhSvn,
  regardless of extension(s) install status;
* **VisualSvn**: will always try to load VisualSvn,
  regardless of extension(s) install status;
* **Disabled**: won't try to switch Scc provider for
  Subversion repositories, effectively disabling
  SccAutoSwitcher for this RCS.