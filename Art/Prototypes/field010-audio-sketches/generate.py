"""Original procedural listening sketches; no sampled audio and no game integration."""
import hashlib
import json
import math
import random
import struct
import wave
from pathlib import Path

RATE = 44100
ROOT = Path(__file__).resolve().parents[3]
OUT = ROOT / 'TestResults/field010-audio-sketches'
OUT.mkdir(parents=True, exist_ok=True)


def render(kind, seconds, seed):
    rng = random.Random(seed)
    samples = []
    low = 0.0
    fast = 0.0
    for i in range(int(seconds * RATE)):
        t = i / RATE
        noise = rng.uniform(-1, 1)
        low += .035 * (noise - low)
        fast += .35 * (noise - fast)
        if kind == 'lightning':
            # Close dry arc, several irregular cracks, then a restrained low thunder tail.
            arc = (noise - fast) * math.exp(-t / .028) * .8
            cracks = sum((noise - low) * .48 * math.exp(-(t - at) / .014)
                         for at in (.045, .085, .13) if t >= at)
            rumble = low * 5.5 * (1 - math.exp(-t / .012)) * math.exp(-t / .24)
            thump = math.sin(2 * math.pi * (68 * t + 30 * .04 * (1 - math.exp(-t / .04)))) * .25 * math.exp(-t / .10)
            value = arc + cracks + rumble + thump
        else:
            # Air descends for 160 ms, followed by an airy impact and inharmonic luminous resonance.
            impact = .16
            if t < impact:
                phase = 2 * math.pi * (1250 * t - 1900 * t * t)
                value = (.12 * math.sin(phase) + .3 * fast) * math.sin(math.pi * t / impact) ** 1.5
            else:
                u = t - impact
                air = fast * .8 * math.exp(-u / .055)
                body = math.sin(2 * math.pi * 105 * u) * .28 * math.exp(-u / .09)
                shimmer = sum(math.sin(2 * math.pi * hz * u + n * .43) * gain * math.exp(-u / decay)
                              for n, (hz, gain, decay) in enumerate(((420, .14, .24), (672, .11, .31),
                                  (1008, .065, .27), (1480, .035, .18))))
                value = air + body + shimmer * (1 - math.exp(-u / .003))
        # Tiny start ramp, gentle tail to exact silence; no discontinuity at file edges.
        value *= min(1, t / .0007) * min(1, max(0, (seconds - t) / .07))
        samples.append(value)
    stereo = []
    for i, value in enumerate(samples):
        # A few quiet reflections suggest a hall without obscuring the attack.
        left = value
        right = value
        for delay, gain in ((.031, .10), (.061, .065), (.109, .035)):
            j = i - int(delay * RATE)
            if j >= 0:
                left += samples[j] * gain
                right += samples[max(0, j - 97)] * gain
        stereo.append((left, right))
    peak = max(abs(x) for pair in stereo for x in pair)
    gain = .68 / peak
    pcm = b''.join(struct.pack('<hh', round(left * gain * 32767), round(right * gain * 32767))
                   for left, right in stereo)
    path = OUT / (kind + '-v1.wav')
    with wave.open(str(path), 'wb') as output:
        output.setnchannels(2)
        output.setsampwidth(2)
        output.setframerate(RATE)
        output.writeframes(pcm)
    return {'file': path.name, 'seconds': seconds, 'sampleRate': RATE, 'channels': 2,
            'peakDbfs': round(20 * math.log10(.68), 2), 'seed': seed,
            'sha256': hashlib.sha256(path.read_bytes()).hexdigest()}


records = [render('lightning', 1.05, 10031), render('light-column', 1.2, 10032)]
(OUT / 'provenance.json').write_text(json.dumps({'status': 'listening sketch only',
    'source': 'Original procedural synthesis by Codex for the user; no third-party samples',
    'processing': '44.1 kHz stereo signed PCM16, short reflections, peak -3.35 dBFS, tapered edges',
    'generator': 'Art/Prototypes/field010-audio-sketches/generate.py', 'clips': records}, indent=2), encoding='utf8')
(OUT / 'index.html').write_text('''<!doctype html><html lang="ru"><meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1"><title>Звуковые эскизы</title>
<style>body{background:#24202b;color:#e5dce8;font:18px/1.6 system-ui;max-width:740px;margin:40px auto;padding:20px}
section{background:#342e3d;padding:20px;border-radius:14px;margin:24px 0}audio{width:100%}a{color:#cbb0dd}</style>
<h1>Молния и столб света</h1><p>Два отдельных звуковых эскиза. Можно послушать независимо; в игру пока не подключены.</p>
<section><h2>Удар молнии</h2><p>Резкий электрический треск, несколько коротких разрядов и низкий грохот.</p>
<audio controls preload="metadata" src="lightning-v1.wav"></audio><a href="lightning-v1.wav" download>Скачать WAV</a></section>
<section><h2>Падение столба света</h2><p>Короткий воздушный спуск, мягкий удар и звонкое магическое затухание.</p>
<audio controls preload="metadata" src="light-column-v1.wav"></audio><a href="light-column-v1.wav" download>Скачать WAV</a></section>
<script>document.querySelectorAll('audio').forEach(a=>{a.volume=.65;a.addEventListener('play',()=>document.querySelectorAll('audio').forEach(b=>{if(b!==a)b.pause()}))})</script>
</html>''', encoding='utf8')
print(json.dumps(records))
