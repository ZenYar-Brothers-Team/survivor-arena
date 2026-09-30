import unittest

import numpy as np

import probe_temporal_imitation as temporal


class TemporalProbeTests(unittest.TestCase):
    def test_uses_only_observations_old_enough_for_each_lag(self):
        history_times = [0.1, 0.5, 1.0]
        history_vectors = [np.asarray([value, value, value, value], dtype=np.float32)
                           for value in (1, 2, 3)]
        result = temporal.temporal_vector(np.asarray([9, 9], dtype=np.float32), 1.3,
                                          (0.4, 1.2), history_times, history_vectors)
        np.testing.assert_array_equal(result, [9, 9, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1])

    def test_missing_history_is_zero_and_marked_missing(self):
        result = temporal.temporal_vector(np.asarray([9, 9], dtype=np.float32), 0.2,
                                          (0.4,), [0.1], [np.asarray([1, 1, 1, 1], dtype=np.float32)])
        np.testing.assert_array_equal(result, [9, 9, 0, 0, 0, 0, 0])


if __name__ == "__main__":
    unittest.main()
