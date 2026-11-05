#!/usr/bin/env python3
"""Apply locked localization patches without changing submodule HEAD or real indexes."""
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import tempfile

ROOT = Path(__file__).resolve().parents[1]
LOCK = ROOT / "patches/submodules/manifest.json"


def git(repo, *args, env=None, check=True):
    result = subprocess.run(
        ["git", "-C", str(repo), *args], env=env,
        stdout=subprocess.PIPE, stderr=subprocess.PIPE,
    )
    if check and result.returncode:
        raise RuntimeError(result.stderr.decode(errors="replace").strip() or
                           f"git {' '.join(args)} failed in {repo}")
    return result


def output(repo, *args, env=None):
    return git(repo, *args, env=env).stdout.decode().strip()


def matches(repo, tree, env):
    git(repo, "read-tree", tree, env=env)
    # read-tree creates entries without worktree stat data; refresh the temporary
    # index before diff-files so clean files are compared by content.
    refresh = git(repo, "update-index", "--refresh", env=env, check=False)
    if refresh.returncode not in (0, 1):
        raise RuntimeError(refresh.stderr.decode(errors="replace"))
    # Fresh alternate indexes avoid trusting skip-worktree/assume-unchanged flags.
    clean = git(repo, "diff-files", "--quiet", "--", env=env, check=False)
    if clean.returncode not in (0, 1):
        raise RuntimeError(clean.stderr.decode(errors="replace"))
    return clean.returncode == 0 and not git(
        repo, "ls-files", "--others", "-z", env=env
    ).stdout


def rollback(repo, base, added):
    # Preflight excluded all untracked files. Remove only patch-created paths.
    for name in added:
        path = repo / name
        if path.is_symlink() or path.is_file():
            path.unlink()
        parent = path.parent
        while parent != repo:
            try:
                parent.rmdir()
            except OSError:
                break
            parent = parent.parent
    git(repo, "restore", "--source=" + base, "--worktree", "--", ".")


def main():
    data = json.loads(LOCK.read_text(encoding="utf-8"))
    if data.get("schema") != 1 or len(data.get("submodules", [])) != 2:
        raise RuntimeError("Unsupported or incomplete patch manifest")
    names = set()
    plans = []
    with tempfile.TemporaryDirectory(prefix="autoretainer-patches-") as temp:
        for entry in data["submodules"]:
            name = entry["path"]
            if name not in {"ECommons", "AutoRetainerAPI"} or name in names:
                raise RuntimeError("Unexpected or duplicate submodule path")
            names.add(name)
            for key in ("base", "result_tree", "source_commit"):
                if not re.fullmatch(r"[0-9a-f]{40}", entry[key]):
                    raise RuntimeError(f"{name}: invalid {key}")
            patch = ROOT / "patches/submodules" / (name + ".patch")
            if entry["patch"] != patch.relative_to(ROOT).as_posix():
                raise RuntimeError(f"{name}: unexpected patch path")
            if hashlib.sha256(patch.read_bytes()).hexdigest() != entry["sha256"]:
                raise RuntimeError(f"{name}: patch checksum mismatch")
            repo = ROOT / name
            if output(repo, "rev-parse", "--show-toplevel") != str(repo):
                raise RuntimeError(f"{name}: submodule is not initialized")
            if output(repo, "rev-parse", "HEAD") != entry["base"]:
                raise RuntimeError(f"{name}: HEAD does not match locked base {entry['base']}")
            link = output(ROOT, "ls-files", "--stage", "--", name).split()
            if link[:2] != ["160000", entry["base"]]:
                raise RuntimeError(f"{name}: root gitlink does not match locked base")
            url = output(ROOT, "config", "-f", str(ROOT / ".gitmodules"),
                         "submodule." + name + ".url")
            if url != entry["url"]:
                raise RuntimeError(f"{name}: .gitmodules URL does not match manifest")
            base_tree = output(repo, "rev-parse", entry["base"] + "^{tree}")
            if output(repo, "write-tree") != base_tree:
                raise RuntimeError(f"{name}: real index must be clean and match base")
            env = os.environ.copy()
            env["GIT_INDEX_FILE"] = str(Path(temp) / (name + ".index"))
            git(repo, "read-tree", base_tree, env=env)
            git(repo, "apply", "--cached", "--check", "--binary", str(patch), env=env)
            git(repo, "apply", "--cached", "--binary", str(patch), env=env)
            if output(repo, "write-tree", env=env) != entry["result_tree"]:
                raise RuntimeError(f"{name}: patch result tree mismatch")
            added = git(repo, "diff-tree", "--no-commit-id", "--name-only", "-r",
                        "--diff-filter=A", "-z", base_tree, entry["result_tree"]).stdout
            added = [p.decode() for p in added.split(b"\0") if p]
            if matches(repo, base_tree, env):
                git(repo, "apply", "--check", "--binary", str(patch))
                plans.append((repo, entry, env, added, False))
            elif matches(repo, entry["result_tree"], env):
                plans.append((repo, entry, env, added, True))
            else:
                raise RuntimeError(f"{name}: dirty working tree (including untracked/ignored files) or partial patch")

        # No worktree changes occur until every module passes preflight.
        changed = []
        try:
            for repo, entry, env, added, applied in plans:
                if not applied:
                    # Recheck immediately before writing, then retain rollback information.
                    if not matches(repo, entry["base"], env):
                        raise RuntimeError(f"{repo.name}: changed after preflight")
                    changed.append((repo, entry, env, added))
                    git(repo, "apply", "--binary", str(ROOT / entry["patch"]))
                if not matches(repo, entry["result_tree"], env):
                    raise RuntimeError(f"{repo.name}: working tree verification failed")
                print(f"{repo.name}: {'already applied' if applied else 'applied'}; verified {entry['result_tree']}")
        except BaseException:
            errors = []
            for repo, entry, env, added in reversed(changed):
                try:
                    rollback(repo, entry["base"], added)
                    if not matches(repo, entry["base"], env):
                        raise RuntimeError("rollback tree verification failed")
                except BaseException as error:
                    errors.append(f"{repo.name}: {error}")
            if errors:
                raise RuntimeError("Rollback needs attention: " + "; ".join(errors))
            raise


if __name__ == "__main__":
    try:
        main()
    except (Exception, KeyboardInterrupt) as error:
        print(f"Submodule patches stopped: {error}", file=sys.stderr)
        sys.exit(1)
