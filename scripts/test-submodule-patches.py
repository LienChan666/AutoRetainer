#!/usr/bin/env python3
"""Integration checks in an isolated local clone; never builds the plugin."""
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tempfile

SOURCE = Path(__file__).resolve().parents[1]


def run(*args, cwd=None, env=None, ok=True):
    p = subprocess.run(args, cwd=cwd, env=env, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    if ok and p.returncode:
        raise AssertionError(p.stdout.decode() + p.stderr.decode())
    return p


def git(repo, *args):
    return run("git", "-C", str(repo), *args).stdout.decode().strip()


with tempfile.TemporaryDirectory(prefix="autoretainer-patch-test-") as temp:
    root = Path(temp) / "checkout"
    run("git", "clone", "--quiet", "--no-hardlinks", "--no-checkout", str(SOURCE), str(root))
    git(root, "checkout", "--quiet", "HEAD")
    lock = root / "patches/submodules/manifest.json"
    original_lock = lock.read_bytes()
    entries = json.loads(original_lock)["submodules"]
    for entry in entries:
        repo = root / entry["path"]
        git(repo, "init", "--quiet")
        # Fetch only official base history, never the retained localization fork commit.
        git(repo, "fetch", "--quiet", "--depth=2", str(SOURCE / entry["path"]), entry["base"])
        git(repo, "checkout", "--quiet", "--detach", entry["base"])
        source_present = run("git", "-C", str(repo), "cat-file", "-e",
                             entry["source_commit"] + "^{commit}", ok=False)
        assert source_present.returncode != 0, "Fixture unexpectedly includes localization fork commit"
    print("PASS official-base-only fixture; localization fork commits are absent")
    original_patches = {entry["patch"]: (root / entry["patch"]).read_bytes() for entry in entries}

    def reset():
        lock.write_bytes(original_lock)
        for name, content in original_patches.items():
            (root / name).write_bytes(content)
        for entry in entries:
            repo = root / entry["path"]
            git(repo, "reset", "--hard", entry["base"])
            git(repo, "clean", "-ffdqx")
        git(root, "add", "ECommons", "AutoRetainerAPI")

    def snapshot():
        result = []
        for entry in entries:
            repo = root / entry["path"]
            files = []
            for path in sorted(repo.rglob("*")):
                relative = path.relative_to(repo)
                if ".git" in relative.parts or path.is_dir():
                    continue
                content = os.readlink(path).encode() if path.is_symlink() else path.read_bytes()
                files.append((str(relative), hashlib.sha256(content).hexdigest(), path.stat().st_mode))
            result.append((git(repo, "rev-parse", "HEAD"), git(repo, "write-tree"), files))
        return result

    def apply(ok=True, env=None):
        return run(sys.executable, str(root / "scripts/apply-submodule-patches.py"),
                   cwd=root, env=env, ok=ok)

    def reject(label, expected, env=None):
        before = snapshot()
        p = apply(ok=False, env=env)
        assert p.returncode != 0, label
        assert expected in p.stderr.decode(), (label, p.stderr.decode())
        assert snapshot() == before, label + ": modified worktrees/indexes/HEADs"
        print("PASS " + label)

    apply()
    before = snapshot()
    p = apply()
    assert p.stdout.decode().count("already applied") == 2
    assert snapshot() == before
    for entry in entries:
        repo = root / entry["path"]
        env = os.environ.copy()
        env["GIT_INDEX_FILE"] = str(Path(temp) / (entry["path"] + ".verify-index"))
        run("git", "-C", str(repo), "read-tree", entry["base"], env=env)
        run("git", "-C", str(repo), "add", "-A", "--", ".", env=env)
        tree = run("git", "-C", str(repo), "write-tree", env=env).stdout.decode().strip()
        assert tree == entry["result_tree"]
        assert git(repo, "rev-parse", "HEAD") == entry["base"]
        assert git(repo, "write-tree") == git(repo, "rev-parse", entry["base"] + "^{tree}")
        print(f"PASS exact original localized tree {entry['path']}: {tree}")
    print("PASS clean application and idempotent repeat")

    reset()
    data = json.loads(original_lock)
    data["submodules"][1]["sha256"] = "0" * 64
    lock.write_text(json.dumps(data))
    reject("second-module checksum rejects all before changes", "checksum mismatch")

    reset()
    git(root / "AutoRetainerAPI", "checkout", "--quiet", "--detach", entries[1]["base"] + "^")
    reject("wrong HEAD", "HEAD does not match locked base")

    reset()
    data = json.loads(original_lock)
    data["submodules"][1]["result_tree"] = git(root / "AutoRetainerAPI", "rev-parse", entries[1]["base"] + "^{tree}")
    lock.write_text(json.dumps(data))
    reject("wrong result tree", "patch result tree mismatch")

    reset()
    git(root, "update-index", "--cacheinfo", "160000," + entries[1]["source_commit"] + ",AutoRetainerAPI")
    reject("wrong root gitlink", "root gitlink does not match locked base")

    reset()
    tracked = root / "AutoRetainerAPI/AutoRetainerAPI/Configuration/OfflineCharacterData.cs"
    tracked.write_bytes(tracked.read_bytes() + b"\n// test dirt\n")
    reject("tracked dirt in second module", "dirty working tree")
    git(root / "AutoRetainerAPI", "add", ".")
    reject("staged dirt", "real index must be clean")

    reset()
    (root / "AutoRetainerAPI/untracked-test").write_text("keep")
    reject("untracked file", "dirty working tree")

    reset()
    ignored = root / "AutoRetainerAPI/AutoRetainerAPI/obj/test-cache"
    ignored.parent.mkdir(parents=True)
    ignored.write_text("keep")
    reject("ignored build output", "dirty working tree")

    reset()
    apply()
    tracked.write_bytes(tracked.read_bytes() + b"\n// partial state\n")
    reject("altered already-applied tree", "dirty working tree")

    reset()
    # Force a worktree-application error in module two after module one succeeds.
    wrapper = Path(temp) / "bin"
    wrapper.mkdir()
    real_git = shutil.which("git")
    (wrapper / "git").write_text(
        "#!/usr/bin/env python3\nimport os,sys\n"
        "a=sys.argv[1:]\n"
        "if 'apply' in a and '--cached' not in a and '--check' not in a and a[1].endswith('/AutoRetainerAPI'):\n"
        " print('injected application failure',file=sys.stderr);sys.exit(1)\n"
        f"os.execv({real_git!r},[{real_git!r}]+a)\n")
    (wrapper / "git").chmod(0o755)
    env = os.environ.copy()
    env["PATH"] = str(wrapper) + os.pathsep + env["PATH"]
    reject("cross-module application failure rolls back", "injected application failure", env)

    reset()
    # Preserve an already-applied module if the other module fails.
    git(root / "ECommons", "apply", "--binary", str(root / entries[0]["patch"]))
    reject("rollback preserves previously applied module", "injected application failure", env)
    print("All integration checks passed; no plugin build was run.")
