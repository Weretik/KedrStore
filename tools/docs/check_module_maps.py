"""Checks that the module and feature maps describe the code as it is, so an agent finds modules and features fast.

Every module in src/Modules has a section in the module map with its real projects, a line in the system map and a
section in the feature map; every host in src/Bootstrapper is in the system map; every feature folder under
<Module>.Api/Endpoints is named in a feature card (by its path, a use case or a file in it, or
drawn under `Endpoints/` in a code tree); every feature card is linked from the feature map.
"""
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
MODULES = ROOT / "src" / "Modules"
HOSTS = ROOT / "src" / "Bootstrapper"
MODULE_MAP = ROOT / "docs" / "sdd" / "architecture" / "overview" / "modules.md"
SYSTEM_MAP = ROOT / "docs" / "sdd" / "architecture" / "overview" / "system-map.md"
FEATURES = ROOT / "docs" / "sdd" / "features"
FEATURE_MAP = FEATURES / "README.md"


def sections(text: str) -> dict[str, str]:
    parts = re.split(r"^## +(.+?) *$", text, flags=re.M)
    return {parts[i].strip(): parts[i + 1] for i in range(1, len(parts), 2)}


def tree_endpoint_folders(text: str) -> set[str]:
    """Folder names drawn under an `Endpoints/` line of a code tree in a feature card."""
    found: set[str] = set()
    parent_indent = None
    for line in text.splitlines():
        indent = len(line) - len(line.lstrip(" │├└─"))
        if parent_indent is not None and indent > parent_indent:
            head = line.lstrip(" │├└─").split("#")[0]
            found.update(m for m in re.findall(r"(\w+)/", head))
            continue
        parent_indent = indent if re.search(r"Endpoints/\s*(#|$)", line) else None
    for group in re.findall(r"Endpoints/\{([^}]*)\}", text):
        found.update(x.strip() for x in group.split(","))
    return found


def main() -> int:
    errors: list[str] = []
    module_map = sections(MODULE_MAP.read_text(encoding="utf-8"))
    system_map = SYSTEM_MAP.read_text(encoding="utf-8")
    feature_map = FEATURE_MAP.read_text(encoding="utf-8")
    feature_sections = sections(feature_map)
    cards = [p for p in FEATURES.rglob("*.md") if p != FEATURE_MAP]
    card_text = "\n".join(p.read_text(encoding="utf-8") for p in cards)
    drawn = tree_endpoint_folders(card_text)

    for module in sorted(p for p in MODULES.iterdir() if p.is_dir()):
        name = module.name
        project_dirs = sorted({p.parent for p in module.glob("*/*.csproj")} | {p.parent for p in module.glob("*/*/*.csproj")})
        projects = sorted(p.name for p in project_dirs)
        section = module_map.get(name)
        if section is None:
            errors.append(f"{MODULE_MAP.relative_to(ROOT)}: no '## {name}' section")
        else:
            listed = re.search(r"^Layers: (.+?)\.?$", section, flags=re.M)
            listed_projects = sorted(x.strip() for x in listed.group(1).split(",")) if listed else []
            if listed_projects != projects:
                errors.append(f"{MODULE_MAP.relative_to(ROOT)}: '## {name}' must say 'Layers: {', '.join(projects)}.'")
        if not re.search(rf"^- {name} owns ", system_map, flags=re.M):
            errors.append(f"{SYSTEM_MAP.relative_to(ROOT)}: no '- {name} owns ...' line under Business modules")
        if name not in feature_sections:
            errors.append(f"{FEATURE_MAP.relative_to(ROOT)}: no '## {name}' section")

        for api in (d / "Endpoints" for d in project_dirs if d.name.endswith(".Api") and (d / "Endpoints").is_dir()):
            for feature in sorted(p for p in api.iterdir() if p.is_dir()):
                names = [f"Endpoints/{feature.name}"] + [p.stem for p in feature.iterdir()]
                if feature.name not in drawn and not any(re.search(rf"{re.escape(name)}\b", card_text) for name in names):
                    errors.append(f"{feature.relative_to(ROOT)}: no feature card names this endpoint folder")

    for host in sorted(p for p in HOSTS.iterdir() if (p / f"{p.name}.csproj").exists()):
        if f"src/Bootstrapper/{host.name}" not in system_map:
            errors.append(f"{SYSTEM_MAP.relative_to(ROOT)}: host src/Bootstrapper/{host.name} is not described")

    for card in sorted(cards):
        link = card.relative_to(FEATURES).as_posix()
        if f"]({link})" not in feature_map:
            errors.append(f"{FEATURE_MAP.relative_to(ROOT)}: feature card {link} is not linked")

    print("\n".join(errors) or "Module and feature maps match the code.")
    return 1 if errors else 0


if __name__ == "__main__":
    sys.exit(main())
