"""Prepare approved generated pickup masters as normalized runtime sprites."""

from __future__ import annotations

import argparse
from pathlib import Path

from PIL import Image


def prepare(source: Path, output: Path, target: int = 256, visible: int = 192) -> tuple[int, int, int, int]:
    image = Image.open(source).convert("RGBA")
    alpha = image.getchannel("A")
    mask = alpha.point(lambda value: 255 if value >= 10 else 0)
    bounds = mask.getbbox()
    if bounds is None:
        raise ValueError(f"{source} has no visible pixels")

    cropped = image.crop(bounds)
    scale = min(visible / cropped.width, visible / cropped.height)
    size = (max(1, round(cropped.width * scale)), max(1, round(cropped.height * scale)))
    cropped = cropped.resize(size, Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", (target, target), (0, 0, 0, 0))
    canvas.alpha_composite(cropped, ((target - size[0]) // 2, (target - size[1]) // 2))
    output.parent.mkdir(parents=True, exist_ok=True)
    canvas.save(output, optimize=True)
    return bounds


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    print(prepare(args.source, args.output))


if __name__ == "__main__":
    main()
