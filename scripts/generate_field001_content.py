"""Compatibility entry point. Prefer python scripts/content/generate.py [--check]."""
import sys
from content.generate import main

if __name__ == "__main__":
    sys.exit(main())
