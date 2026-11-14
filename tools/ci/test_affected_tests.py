"""Tests for affected_tests.py: python3 -m unittest discover -s tools/ci"""
import unittest

from affected_tests import select

# Sales references Catalog.Contracts; Catalog references nothing of other modules.
REFS = {"Sales": {"Catalog"}, "Catalog": set(), "Content": set(), "Identity": set()}


class SelectTests(unittest.TestCase):
    def test_module_change_runs_that_module_and_the_modules_that_reference_it(self):
        self.assertEqual(select(["src/Modules/Catalog/Catalog.Domain/Products/Product.cs"], REFS), {"Catalog", "Sales"})

    def test_change_in_a_module_nobody_references_runs_only_that_module(self):
        self.assertEqual(select(["src/Modules/Content/Content.Api/Features/Home/HomeBlocksController.cs"], REFS), {"Content"})

    def test_module_test_change_runs_that_module(self):
        self.assertEqual(select(["tests/UnitTests/Identity/UserTests.cs"], REFS), {"Identity"})

    def test_shared_code_hosts_and_build_files_run_everything(self):
        for path in ["src/BuildingBlocks/BuildingBlocks.Domain/Result.cs",
                     "src/Bootstrapper/Host.Api/Program.cs",
                     "Directory.Packages.props",
                     "tests/IntegrationTests/TestSupport/Database/PostgresFixture.cs",
                     "tests/UnitTests/UnitTests.csproj",
                     ".github/workflows/deploy-cloudrun.yml"]:
            with self.subTest(path=path):
                self.assertIsNone(select([path], REFS))

    def test_documentation_only_change_runs_no_module_tests(self):
        self.assertEqual(select(["docs/sdd/features/README.md", "AGENTS.md", "tools/docs/check_doc_links.py"], REFS),
                         set())


if __name__ == "__main__":
    unittest.main()
