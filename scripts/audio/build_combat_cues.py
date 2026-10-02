"""Build the combat/zone SFX of the DECISION-0154 sound pass from the already downloaded Kenney CC0 packs.

Inputs live in the ignored TestResults/audio-source (see fetch_kenney.py there) and the OGG writer in
TestResults/audio-tools (soundfile). Each output is a mono mix of one or more pack clips (pitch by resampling, per-layer
gain/delay), trimmed, 5 ms faded and peak-normalised like the earlier clips. Re-running replaces the same names in
docs/audio/SOURCES.json and rewrites the clips; existing .meta files (GUIDs) are kept, new ones are created.

    python scripts/audio/build_combat_cues.py
"""

import hashlib
import json
import sys
import uuid
import zipfile
from io import BytesIO
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "TestResults/audio-tools"))
import numpy as np  # noqa: E402
import soundfile as sf  # noqa: E402

SOURCE_DIR = ROOT / "TestResults/audio-source"
OUT_DIR = ROOT / "Assets/Resources/Audio/Sfx"
SOURCES = ROOT / "docs/audio/SOURCES.json"
RATE = 44100
PEAK = 0.70

PACKS = {
    "interface": ("interface-sounds", "https://kenney.nl/assets/interface-sounds"),
    "rpg": ("rpg-audio", "https://kenney.nl/assets/rpg-audio"),
    "impact": ("impact-sounds", "https://kenney.nl/assets/impact-sounds"),
    "digital": ("digital-audio", "https://kenney.nl/assets/digital-audio"),
}

META_TEMPLATE = """fileFormatVersion: 2
guid: {guid}
AudioImporter:
  externalObjects: {{}}
  serializedVersion: 8
  defaultSettings:
    serializedVersion: 2
    loadType: 0
    sampleRateSetting: 0
    sampleRateOverride: 44100
    compressionFormat: 1
    quality: 0.75
    conversionMode: 0
    preloadAudioData: 0
  platformSettingOverrides: {{}}
  forceToMono: 0
  normalize: 1
  loadInBackground: 0
  ambisonic: 0
  3D: 1
  userData:
  assetBundleName:
  assetBundleVariant:
"""


def L(pack, member, gain=1.0, pitch=1.0, delay=0.0, cut=None):
    """One layer: pack key, member name (no folder/extension), gain, pitch ratio, delay seconds, optional max seconds."""
    return {"pack": pack, "member": member, "gain": gain, "pitch": pitch, "delay": delay, "cut": cut}


# name -> (layers, loudest-layer-first). Families are described in ProductionAudio.json (cue ids).
RECIPES = {
    # Player attack families (skillCues).
    "rough_throw": [L("impact", "impactGeneric_light_000", 1.0, 1.25), L("rpg", "cloth1", 0.6, 1.1)],
    "blade_cast_3": [L("rpg", "drawKnife2", 1.0, 1.15, cut=0.35)],
    "wind_cast": [L("rpg", "cloth2", 1.0, 0.8), L("digital", "phaserUp1", 0.3, 1.4)],
    "wind_cast_alt": [L("rpg", "cloth4", 1.0, 0.7), L("digital", "phaserUp3", 0.25, 1.5)],
    "lightning_cast": [L("digital", "zap1", 1.0, 1.0, cut=0.45)],
    "lightning_cast_alt": [L("digital", "zap2", 1.0, 1.0, cut=0.45)],
    "fire_cast": [L("digital", "phaseJump1", 1.0, 0.8, cut=0.45), L("rpg", "cloth3", 0.4, 0.6)],
    "fire_cast_alt": [L("digital", "phaseJump3", 1.0, 0.75, cut=0.45), L("rpg", "cloth3", 0.4, 0.6)],
    "ice_cast": [L("impact", "impactGlass_light_001", 1.0, 1.1), L("interface", "glass_003", 0.5, 1.2, delay=0.02)],
    "ice_cast_alt": [L("impact", "impactGlass_light_002", 1.0, 1.2), L("interface", "glass_005", 0.5, 1.1, delay=0.02)],
    # Enemy actions.
    "enemy_windup": [L("digital", "phaserUp2", 1.0, 1.15, cut=0.5)],
    "enemy_shot": [L("interface", "pluck_002", 1.0, 0.75), L("rpg", "cloth1", 0.5, 1.3)],
    "enemy_shot_alt": [L("impact", "impactTin_medium_000", 0.8, 1.5), L("rpg", "cloth2", 0.6, 1.2)],
    "enemy_dash_windup": [L("digital", "phaserUp4", 1.0, 0.9, cut=0.6)],
    "enemy_dash": [L("rpg", "clothBelt", 1.0, 0.7, cut=0.6), L("impact", "impactSoft_heavy_000", 0.5, 1.0, cut=0.6)],
    # Boss specials.
    "boss_zone": [L("digital", "phaserDown2", 1.0, 0.8, cut=0.7), L("impact", "impactPlate_heavy_000", 0.7, 0.8, delay=0.05)],
    "boss_beam": [L("digital", "laser3", 1.0, 0.85, cut=0.7)],
    "boss_summon": [L("digital", "lowThreeTone", 1.0, 1.0)],
    "boss_teleport_windup": [L("digital", "phaserUp7", 1.0, 0.8, cut=0.8)],
    "boss_slam": [L("impact", "impactMetal_heavy_000", 1.0, 0.8), L("impact", "impactPlate_heavy_001", 0.8, 0.7, delay=0.02)],
    # Altars, shrines and academy seals.
    "altar_on": [L("digital", "powerUp3", 1.0, 1.0, cut=0.6)],
    "altar_cursed": [L("digital", "zapThreeToneDown", 1.0, 0.85, cut=0.7)],
    "zone_seal": [L("interface", "glass_002", 1.0, 0.9), L("digital", "twoTone1", 0.45, 1.0, delay=0.03)],
    "shrine_reward": [L("impact", "impactBell_heavy_003", 1.0, 1.4), L("digital", "powerUp7", 0.6, 1.0, delay=0.04)],
    "zone_strike": [L("impact", "impactMetal_heavy_002", 1.0, 0.8, cut=0.7), L("digital", "zap2", 0.6, 0.8, cut=0.7)],
    "zone_burst": [L("digital", "phaseJump2", 1.0, 0.9, cut=0.5), L("impact", "impactSoft_heavy_001", 0.5, 1.0)],
    "zone_portal": [L("digital", "highUp", 1.0, 1.0, cut=0.7)],
}


def sha256(data):
    return hashlib.sha256(data).hexdigest()


def read_layer(layer, archives):
    slug, _ = PACKS[layer["pack"]]
    member = f"Audio/{layer['member']}.ogg"
    raw = archives[layer["pack"]].read(member)
    samples, rate = sf.read(BytesIO(raw), dtype="float32", always_2d=True)
    mono = samples.mean(axis=1)
    # Pitch by resampling: ratio > 1 is higher and shorter; the target rate converts every pack to RATE.
    step = layer["pitch"] * rate / RATE
    positions = np.arange(0, len(mono) - 1, step)
    mono = np.interp(positions, np.arange(len(mono)), mono).astype("float32")
    if layer["cut"]:
        mono = mono[: int(layer["cut"] * RATE)]
        fade = min(len(mono) // 4, int(0.04 * RATE))
        if fade:
            mono[-fade:] *= np.linspace(1, 0, fade, dtype="float32")
    return mono * layer["gain"], f"{slug}.zip:{member}", sha256(raw)


def build(name, layers, archives):
    parts = [read_layer(layer, archives) for layer in layers]
    length = max(int(layer["delay"] * RATE) + len(part[0]) for layer, part in zip(layers, parts))
    mix = np.zeros(length, dtype="float32")
    for layer, (data, _, _) in zip(layers, parts):
        offset = int(layer["delay"] * RATE)
        mix[offset:offset + len(data)] += data
    envelope = np.abs(mix)
    active = np.flatnonzero(envelope > max(0.001, float(envelope.max()) * 0.002))
    margin = int(RATE * 0.012)
    mix = mix[max(0, active[0] - margin): min(len(mix), active[-1] + margin + 1)]
    fade = min(int(RATE * 0.005), len(mix) // 3)
    mix[:fade] *= np.linspace(0, 1, fade, dtype="float32")
    mix[-fade:] *= np.linspace(1, 0, fade, dtype="float32")
    mix *= min(4.0, PEAK / float(np.abs(mix).max()))
    target = OUT_DIR / f"{name}.ogg"
    with sf.SoundFile(target, mode="w", samplerate=RATE, channels=1, format="OGG", subtype="VORBIS") as dest:
        dest.write(np.ascontiguousarray(mix))
    meta = target.with_name(target.name + ".meta")
    if not meta.exists():
        meta.write_text(META_TEMPLATE.format(guid=uuid.uuid4().hex), encoding="utf-8", newline="\r\n")
    changes = []
    for layer in layers:
        text = f"{layer['member']} gain {layer['gain']:g}"
        if layer["pitch"] != 1.0:
            text += f" pitch x{layer['pitch']:g}"
        if layer["delay"]:
            text += f" delay {layer['delay'] * 1000:g} ms"
        if layer["cut"]:
            text += f" cut {layer['cut']:g} s"
        changes.append(text)
    packs = []
    for layer in layers:
        page = PACKS[layer["pack"]][1]
        if page not in packs:
            packs.append(page)
    return {
        "name": name,
        "file": target.relative_to(ROOT).as_posix(),
        "sha256": sha256(target.read_bytes()),
        "source": " + ".join(part[1] for part in parts),
        "sourceSha256": sha256("".join(part[2] for part in parts).encode("ascii")),
        "author": "Kenney",
        "license": "CC0 1.0",
        "page": " + ".join(packs),
        "seconds": round(len(mix) / RATE, 3),
        "sourceSeconds": round(max(len(part[0]) for part in parts) / RATE, 3),
        "sampleRate": RATE,
        "channels": 1,
        "processing": "mono mix of layers (" + "; ".join(changes) + "), trim threshold, 5 ms fade, peak target 0.70",
    }


def main():
    archives = {key: zipfile.ZipFile(SOURCE_DIR / f"{slug}.zip") for key, (slug, _) in PACKS.items()}
    manifest = json.loads(SOURCES.read_text(encoding="utf-8"))
    records = [item for item in manifest["assets"] if item["name"] not in RECIPES]
    for name, layers in RECIPES.items():
        record = build(name, layers, archives)
        records.append(record)
        print(f"{name}: {record['seconds']} s")
    manifest["assets"] = records
    SOURCES.write_text(json.dumps(manifest, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


if __name__ == "__main__":
    main()
