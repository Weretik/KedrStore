"""Chooses which module tests a pull request runs.

usage: affected_tests.py <base ref>   (writes scope= and filters to $GITHUB_OUTPUT, or prints them)

A change in src/Modules/<Module>/ runs the tests of that module and of every module whose projects reference it.
A change in tests/<Project>/<Module>/ runs that module's tests. Shared code (BuildingBlocks), hosts
(Bootstrapper), build files, test support and CI files run everything. Documentation runs no module tests.
Architecture tests and the platform integration tests always run; a push to master always runs everything.
"""
import os
import re
import subprocess
import sys

DOC_PREFIXES = ("docs/", "tools/docs/")
TEST_PROJECTS = ("UnitTests", "IntegrationTests")


def module_of_src(path):
    parts = path.split("/")
    if len(parts) > 3 and parts[0] == "src" and parts[1] == "Modules":
        return parts[2]
    return None


def select(changed, references):
    """Returns the modules whose tests must run, or None when everything must run.

    references maps a module to the modules its projects reference.
    """
    modules = set()
    for path in changed:
        if path.startswith(DOC_PREFIXES) or path.endswith(".md"):
            continue
        module = module_of_src(path)
        if module in references:
            modules.add(module)
            continue
        parts = path.split("/")
        if len(parts) > 3 and parts[0] == "tests" and parts[1] in TEST_PROJECTS and parts[2] in references:
            modules.add(parts[2])
            continue
        return None

    affected = set(modules)
    grew = True
    while grew:
        dependents = {m for m, refs in references.items() if refs & affected} - affected
        affected |= dependents
        grew = bool(dependents)
    return affected


def module_references(root="src/Modules"):
    refs = {}
    for module in sorted(os.listdir(root)):
        folder = os.path.join(root, module)
        if not os.path.isdir(folder):
            continue
        refs[module] = set()
        for dirpath, dirs, files in os.walk(folder):
            dirs[:] = [d for d in dirs if d not in ("bin", "obj")]
            for name in files:
                if name.endswith(".csproj"):
                    text = open(os.path.join(dirpath, name), encoding="utf-8-sig").read()
                    for ref in re.findall(r'ProjectReference\s+Include="([^"]+)"', text):
                        target = module_of_src(os.path.relpath(
                            os.path.normpath(os.path.join(dirpath, ref.replace("\\", "/"))), "."))
                        if target and target != module:
                            refs[module].add(target)
    return refs


def main():
    base = sys.argv[1]
    changed = subprocess.run(["git", "diff", "--name-only", f"{base}...HEAD"], check=True,
                             capture_output=True, text=True).stdout.split()
    modules = select(changed, module_references())
    if modules is None:
        lines = ["scope=all", "unit_filter=", "integration_filter="]
    else:
        unit = "|".join(f"FullyQualifiedName~UnitTests.{m}." for m in sorted(modules))
        integration = "|".join(f"FullyQualifiedName~IntegrationTests.{m}."
                               for m in sorted(modules | {"Platform"}))
        lines = [f"scope={'modules' if modules else 'none'}", f"modules={' '.join(sorted(modules))}",
                 f"unit_filter={unit}", f"integration_filter={integration}"]
    output = os.environ.get("GITHUB_OUTPUT")
    if output:
        with open(output, "a", encoding="utf-8") as handle:
            handle.write("\n".join(lines) + "\n")
    print("\n".join(lines))


if __name__ == "__main__":
    main()
