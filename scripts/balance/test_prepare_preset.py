import copy
import json
import unittest
from pathlib import Path

from prepare_preset import build_preset


class PreparePresetTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        catalog_path = Path(__file__).resolve().parents[2] / "Assets/Resources/Content/Meta/MetaEconomy.json"
        cls.catalog = json.loads(catalog_path.read_text(encoding="utf-8"))

    def base(self):
        return {
            "currency": 1000,
            "firstRun": True,
            "upgradesDisabled": False,
            "upgrades": {"META-003:CHAR-001": 2},
            "unlocked": [],
            "clearedFields": [],
        }

    def test_spending_and_initial_unlocked(self):
        result = build_preset(self.base(), self.catalog)
        self.assertEqual(300, result["upgradeSpending"]["META-003:CHAR-001"])
        self.assertIn("CHAR-001", result["unlocked"])
        self.assertIn("FIELD-001", result["unlocked"])
        self.assertEqual({}, result["runs"])

    def test_rejects_unknown_owner_level_and_catalog_ids(self):
        for change in (
            lambda d: d["upgrades"].update({"META-003:CHAR-002": 1}),
            lambda d: d["upgrades"].update({"META-003:CHAR-001": 99}),
            lambda d: d["unlocked"].append("FIELD-999"),
            lambda d: d["clearedFields"].append("FIELD-002"),
        ):
            with self.subTest(change=change):
                data = copy.deepcopy(self.base())
                change(data)
                with self.assertRaises(ValueError):
                    build_preset(data, self.catalog)

    def test_rejects_missing_and_extra_properties(self):
        data = self.base()
        data["unexpected"] = 1
        with self.assertRaises(ValueError):
            build_preset(data, self.catalog)
        del data["unexpected"]
        del data["currency"]
        with self.assertRaises(ValueError):
            build_preset(data, self.catalog)


if __name__ == "__main__":
    unittest.main()
