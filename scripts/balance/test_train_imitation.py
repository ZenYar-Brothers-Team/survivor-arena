import unittest
from pathlib import Path
from unittest import mock

import numpy as np

import train_imitation as imitation


class ImitationTrainingTests(unittest.TestCase):
    def test_direction_index_maps_idle_and_cardinal_action(self):
        self.assertEqual(0, imitation.direction_index([0, 0]))
        self.assertEqual(1, imitation.direction_index([1, 0]))
        self.assertEqual(2, imitation.direction_index([0, 1]))
        self.assertEqual(7, imitation.direction_index([-0.71, -0.71]))

    def test_score_exposes_changed_direction_failure(self):
        target = np.asarray([1, 2, 3])
        previous = np.asarray([1, 1, 3])
        score = imitation.score(target, previous, previous)
        self.assertEqual(1, score["changedSamples"])
        self.assertEqual(0, score["changedAccuracy"])
        self.assertAlmostEqual(2 / 3, score["accuracy"])

    def test_rejects_duplicate_runs_before_fitting(self):
        data = ({"runId": "same"}, np.ones((1, 54), dtype=np.float32),
                np.asarray([1]), np.asarray([1]))
        with mock.patch.object(imitation, "load_run", return_value=data), \
                mock.patch.object(imitation, "fit") as fit:
            with self.assertRaisesRegex(ValueError, "distinct run"):
                imitation.train([Path("a"), Path("b"), Path("c")], Path("unused"))
            fit.assert_not_called()


if __name__ == "__main__":
    unittest.main()
