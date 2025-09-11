# HUE: HisaCat's Unity Extensions

A collection of useful unity plugins and scripts

## Features

See [Documentation](Documentation~/).

---

## Installation guide

You can install this package using [UPM](#via-upm-unity-package-manager) or [Git Subtree](#via-git-subtree).

## Via UPM (Unity Package Manager)

### Prerequisites

- Install the [Git client](https://git-scm.com/) (minimum version 2.14.0) on your computer.
- On Windows, add the Git executable path to the `PATH` system environment variable.
- Install the [Git LFS client](https://git-lfs.com/) on your computer.
- This repository is private, so you need to follow the instructions in [Use private repositories with HTTPS Git URLs](https://docs.unity3d.com/6000.2/Documentation/Manual/upm-config-https-git.html) for authentication.

### Procedure

To install a UPM package from a Git URL:
1. Open the [Package Manager window](https://docs.unity3d.com/6000.2/Documentation/Manual/upm-ui-access.html) via the editor menu `(Editor -> Window -> Package Manager)`
2. Open the **Add (+)** menu in the Package Manager’s toolbar.
3. Select **Add package from git url…** menu.
4. Paste the Git URL: `https://github.com/hisacat/HisaCat-Unity-Extensions.git`  
    > Also, you can specify specific branch, version, commit with `#` prefix:
    > - Specific branch: `https://github.com/hisacat/HisaCat-Unity-Extensions.git#develop`
    > - Specific version: `https://github.com/hisacat/HisaCat-Unity-Extensions.git#v0.0.1`
    > - Commit hash: `https://github.com/hisacat/HisaCat-Unity-Extensions.git#b038ec42e781438ce946e2c7cfaac5e9fd183898`
5. Select **Install**.

Also, you can refer to the following official documents:
- [Install a UPM package from a Git URL](https://docs.unity3d.com/6000.2/Documentation/Manual/upm-ui-giturl.html)

---

## Via Git Subtree

For contributors, you can use Git Subtree to install this package in your local repository.  
After this, you can **push** your commits or **pull** this repository from your local repository.

### Prerequisites

- Your Unity project must be a Git repository.

### Procedure

1. Install subtree
```bash
git checkout 
git remote add hue git@github.com:hisacat/HisaCat-Unity-Extensions.git
git subtree add --prefix=Packages/HisaCat-Unity-Extensions hue develop
```

2. Bootstrap push
```bash
git subtree split --prefix=Packages/HisaCat-Unity-Extensions -b hue-split
git push hue hue-split:develop
```

3. Add push & pull aliases
```bash
git config alias.huepull '!git fetch hue && git subtree pull --prefix=Packages/HisaCat-Unity-Extensions hue develop'
git config alias.huepush 'subtree push --prefix=Packages/HisaCat-Unity-Extensions hue develop'
```

### Usage

Push:
```bash
git huepush
```
Pull:
```bash
git huepull
```

### Remove
``` bash
git rm -r Packages/HisaCat-Unity-Extensions
```

---

## Requirements

- Unity 2020.3 or higher

---

## CHANGELOG

See [CHANGELOG](CHANGELOG.md).

---

## [LICENSE](LICENSE.md)

> This package and all associated files and assets are proprietary and confidential.
> Unauthorized copying, modification, distribution, sublicensing, or use, in whole or in part, is strictly prohibited without the prior written consent of **HisaCat**.

---

## Contact

HisaCat

[ahisacat@gmail.com](mailto:ahisacat@gmail.com)

[ [X (Twitter)](https://x.com/ahisacat) | [GitHub](https://github.com/hisacat) | [Discord](https://discord.com/users/295816286213242880) ]
