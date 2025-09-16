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

**Performance Note**: As the number of commits increases after subtree installation, push operations become significantly slower due to Git subtree's nature of processing the entire commit history.  
To maintain optimal performance when making changes to this repository, we recommend reinstalling the subtree before committing and pushing changes. The `huepull` alias below implements this approach automatically.

1. Add subtree remote
```bash
git remote add hue git@github.com:hisacat/HisaCat-Unity-Extensions.git
```

2. Add push & pull aliases
```bash
git config alias.huepull '!git fetch hue && git rm -r Packages/cat.hisa.hisacat-unity-extensions && git commit -m "chore: remove subtree package for re-install" && git subtree add --prefix=Packages/cat.hisa.hisacat-unity-extensions hue develop'
# Option - Just pull without re-installing:
# git config alias.huepull '!git fetch hue && git subtree pull --prefix=Packages/cat.hisa.hisacat-unity-extensions hue develop
git config alias.huepush 'subtree push --prefix=Packages/cat.hisa.hisacat-unity-extensions hue develop'
```

### Usage

Pull
```bash
git huepull
```

Push:
```bash
git huepush
```

### Remove
``` bash
git rm -r Packages/HisaCat-Unity-Extensions
```

---

## Via Local Clone (Recommended for Maintainers)

For contributors who want to develop this package inside a Unity project without the performance drawbacks of Git Subtree, you can simply **clone the package directly into the `Packages/` folder**.  

This method ensures:
- **Fast operations**: No heavy subtree history processing on push or pull.  
- **Seamless development**: The package is included in the Unity project as normal source code, so IDE features (IntelliSense, navigation, etc.) work immediately.  
- **Transparency for teammates**: When committed, only the source code is included in the game repository. The inner `.git/` directory of the package is ignored automatically, so other contributors see plain source files without needing any extra setup.  

### Procedure

Run the following command in the root of your Unity project repository:

1. Add git embed alias
```bash
git config --global alias.embed \
'!f() {
  if [ $# -lt 2 ]; then
    echo "usage: git embed <repo> <dir> [<git clone options>]" >&2
    return 1
  fi

  repo="$1"
  dir="$2"
  shift 2

  case "$dir" in ""|"/"|".")
    echo "[ERR] invalid dir: \"$dir\"" >&2
    return 1;;
  esac

  if ! git rev-parse --is-inside-work-tree >/dev/null 2>&1; then
    echo "[ERR] not inside a git repository" >&2
    return 1
  fi

  if [ -e "$dir" ]; then
    printf "[WARN] \"%s\" already exists. Delete and re-embed (clone) it? [y/N]: " "$dir"
    IFS= read -r ans
    case "$ans" in
      [yY]) echo "[INFO] Proceeding to replace \"$dir\"..." ;;
      *)    echo "[CANCEL] User canceled. \"$dir\" was not modified."
            return 1;;
    esac
  fi

  rm -rf "$dir" &&
  mkdir -p "$dir" &&
  touch "$dir/.gitkeep" &&
  git add -f "$dir/.gitkeep" &&
  rm -rf "$dir" &&
  git clone "$repo" "$dir" "$@" &&
  git add -A "$dir"

  code=$?
  if [ $code -eq 0 ]; then
    echo "[OK] Embedded \"$repo\" into \"$dir\" successfully."
  else
    echo "[ERR] Failed to embed \"$repo\" into \"$dir\" (code $code)" >&2
  fi
  return $code
}; f'
```

2. Clone
```bash
git embed git@github.com:hisacat/HisaCat-Unity-Extensions.git Packages/cat.hisa.hisacat-unity-extensions --branch develop
```

### Notes
- The .gitkeep file is created only once to ensure that Git recognizes the Packages/cat.hisa.hisacat-unity-extensions/ directory as a valid tracked folder before it is replaced by the cloned repository.
- After the repository is cloned, the .gitkeep file is removed, leaving only the actual package source code.
- From the game repository root, committing this folder will include only the package source code; the internal .git/ directory of the cloned repository is ignored automatically.
- To update or push changes to the package repository itself, simply run git pull or git push inside the Packages/cat.hisa.hisacat-unity-extensions/ directory.

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
