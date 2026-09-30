import unittest
from pathlib import Path
from unittest import mock

import numpy as np

import probe_two_stage_imitation as probe


class TwoStageProbeTests(unittest.TestCase):
    def test_joint_decision_keeps_weak_turn_and_excludes_previous_direction(self):
        change_probability = np.asarray([0.4, 0.9])
        # The direction model puts most probability on the previous action for
        # the second row; a genuine turn must choose another direction.
        direction_probability = np.asarray([[0.1, 0.8, 0.1], [0.1, 0.8, 0.1]])
        actions, changed = probe.select_actions(change_probability, direction_probability,
                                                np.asarray([0, 1, 2]), np.asarray([1, 1]))
        np.testing.assert_array_equal(actions, [1, 0])
        np.testing.assert_array_equal(changed, [False, True])

    def test_rejects_administrative_abort_before_training(self):
        def aborted(path):
            return ({"runId": path.stem, "outcome": "Aborted", "reason": "experimentWallBudget"},
                    np.ones((1, 54), dtype=np.float32), np.asarray([1]), np.asarray([0]))
        with mock.patch.object(probe.base, "load_run", side_effect=aborted), \
                mock.patch.object(probe, "new_model") as model:
            with self.assertRaisesRegex(ValueError, "naturally completed"):
                probe.evaluate([Path("a"), Path("b"), Path("c")])
            model.assert_not_called()


if __name__ == "__main__":
    unittest.main()
