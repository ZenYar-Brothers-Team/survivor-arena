from pathlib import Path
from PIL import Image, ImageOps


ROOT = Path(__file__).resolve().parents[1]


def source(name: str) -> Image.Image:
    return Image.open(ROOT / "Art" / "Source" / "Fields" / "field-001" / name / "selected-master.png")


def fit_transparent(image: Image.Image, size: tuple[int, int], padding: int) -> Image.Image:
    image = image.convert("RGBA")
    alpha = image.getchannel("A")
    bounds = alpha.point(lambda value: 255 if value >= 10 else 0).getbbox()
    if bounds is None:
        raise ValueError("Generated prop has no visible alpha.")
    cropped = image.crop(bounds)
    cropped.thumbnail((size[0] - padding * 2, size[1] - padding * 2), Image.Resampling.LANCZOS)
    result = Image.new("RGBA", size)
    result.alpha_composite(cropped, ((size[0] - cropped.width) // 2, (size[1] - cropped.height) // 2))
    return result


def main() -> None:
    runtime = ROOT / "Assets" / "Resources" / "Art" / "Sprites" / "Fields" / "field-001"
    runtime.mkdir(parents=True, exist_ok=True)

    ground = source("ground").convert("RGB").resize((256, 256), Image.Resampling.LANCZOS)
    tile = Image.new("RGB", (512, 512))
    tile.paste(ground, (0, 0))
    tile.paste(ImageOps.mirror(ground), (256, 0))
    tile.paste(ImageOps.flip(ground), (0, 256))
    tile.paste(ImageOps.flip(ImageOps.mirror(ground)), (256, 256))
    tile.save(runtime / "field-001-ground-tile.png", optimize=True)

    specs = {
        "fence": ((512, 256), 18),
        "stump": ((256, 256), 16),
        "bush": ((256, 256), 20),
        "grass": ((256, 256), 24),
    }
    for name, (size, padding) in specs.items():
        image = source(name)
        fit_transparent(image, size, padding).save(runtime / f"field-001-{name}-prop.png", optimize=True)


if __name__ == "__main__":
    main()
