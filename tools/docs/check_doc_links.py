"""Checks that relative Markdown links in AGENTS.md and docs/sdd, docs/product point to tracked files."""
import re
import subprocess
import sys
from pathlib import PurePosixPath
from urllib.parse import unquote

SCOPES = ("AGENTS.md", "docs/README.md", "docs/sdd/", "docs/product/")
LINK = re.compile(r"\]\(([^)#\s]+)(?:#[^)]*)?\)")
FENCE = re.compile(r"```.*?```", re.S)

tracked = set(subprocess.run(["git", "ls-files"], capture_output=True, text=True, check=True).stdout.splitlines())
dirs = {str(parent) for path in tracked for parent in PurePosixPath(path).parents}
errors = []

for doc in sorted(p for p in tracked if p.endswith(".md") and p.startswith(SCOPES)):
    text = FENCE.sub("", open(doc, encoding="utf-8").read())
    for match in LINK.finditer(text):
        target = unquote(match.group(1))
        if re.match(r"[a-z]+:", target):
            continue
        parts = []
        for part in (PurePosixPath(doc).parent / target).parts:
            if part == "..":
                if parts:
                    parts.pop()
            elif part != ".":
                parts.append(part)
        resolved = "/".join(parts)
        if resolved not in tracked and resolved not in dirs:
            errors.append(f"{doc}: broken link {match.group(1)}")

print("\n".join(errors) or "Documentation links are valid.")
sys.exit(1 if errors else 0)
