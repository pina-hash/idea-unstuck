"""Draws the IDEA Unstuck icon: a magnifier with a small wrench inside the lens,
IDEA green (#78b870, the --green token in idea-app) on the IDEA dark plate
(#1a2a1a). Original artwork; nothing from SOLIDWORKS.

Run:  python3 tools/make-icon.py        (needs Pillow)
Writes assets/icon/:
  unstuck-<N>.png   N = 16 20 24 32 40 48 64 96 128 256, transparent corners
  unstuck.ico       16 20 24 32 40 48 64 128 256 in one file (shortcuts, Apps & features)
  taskpane-16x18.bmp  24-bit, background RGB(192,192,192) for CreateTaskpaneView2
  addin-16.bmp        24-bit, same key color, for the Add-Ins list "Icon Path"
Small sizes are drawn with thicker strokes, not shrunk, so 16 px still reads.
"""
import math, os
from PIL import Image, ImageDraw

GREEN = (0x78, 0xB8, 0x70, 255)
PLATE = (0x1A, 0x2A, 0x1A, 255)
EDGE = (0x2F, 0x46, 0x2F, 255)
HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, '..', 'assets', 'icon')

def draw(size, key=None):
    S = 1024
    small = size <= 24
    img = Image.new('RGBA', (S, S), key + (255,) if key else (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    m = 0 if key else 24
    d.rounded_rectangle([m, m, S - m, S - m], radius=180 if not key else 120, fill=PLATE, outline=EDGE, width=0 if small else 16)
    # Lens ring
    cx, cy, r = 430, 430, 250 if not small else 265
    w = 70 if not small else 115
    d.ellipse([cx - r, cy - r, cx + r, cy + r], outline=GREEN, width=w)
    # Handle, down and to the right
    a = math.radians(45)
    x0, y0 = cx + (r - 10) * math.cos(a), cy + (r - 10) * math.sin(a)
    x1, y1 = cx + (r + 330) * math.cos(a), cy + (r + 330) * math.sin(a)
    d.line([x0, y0, x1, y1], fill=GREEN, width=130 if not small else 170)
    d.ellipse([x1 - 65, y1 - 65, x1 + 65, y1 + 65], fill=GREEN)
    # Wrench inside the lens, short and chunky so it never touches the ring
    # (a bar across the whole lens would read as a "no" sign).
    ang = math.radians(-50)
    ux, uy = math.cos(ang), math.sin(ang)          # toward the head (up and right)
    bw = 58 if not small else 95
    hr = 78 if not small else 105
    hx, hy = cx + 55 * ux, cy + 55 * uy              # head center
    tx, ty = cx - 110 * ux, cy - 110 * uy            # tail end
    d.line([tx, ty, hx, hy], fill=GREEN, width=bw)
    d.ellipse([tx - bw / 2, ty - bw / 2, tx + bw / 2, ty + bw / 2], fill=GREEN)
    d.ellipse([hx - hr, hy - hr, hx + hr, hy + hr], fill=GREEN)
    # Open jaw: a slot cut from the head outward, plus a round nut seat
    jw = 30 if not small else 40
    d.line([hx, hy, hx + (hr + 20) * ux, hy + (hr + 20) * uy], fill=PLATE, width=jw * 2)
    d.ellipse([hx - jw - 4, hy - jw - 4, hx + jw + 4, hy + jw + 4], fill=PLATE)
    return img.resize((size, size), Image.LANCZOS)

def main():
    os.makedirs(OUT, exist_ok=True)
    sizes = [16, 20, 24, 32, 40, 48, 64, 96, 128, 256]
    imgs = {}
    for s in sizes:
        imgs[s] = draw(s)
        imgs[s].save(os.path.join(OUT, 'unstuck-%d.png' % s))
    ico = [16, 20, 24, 32, 40, 48, 64, 128, 256]
    imgs[256].save(os.path.join(OUT, 'unstuck.ico'), sizes=[(s, s) for s in ico], append_images=[imgs[s] for s in ico if s != 256])
    key = (192, 192, 192)
    tp = Image.new('RGB', (16, 18), key)
    tp.paste(draw(16, key).convert('RGB'), (0, 1))
    tp.save(os.path.join(OUT, 'taskpane-16x18.bmp'))
    draw(16, key).convert('RGB').save(os.path.join(OUT, 'addin-16.bmp'))
    sheet = Image.new('RGBA', (sum(sizes) + 10 * len(sizes), 256), (240, 240, 240, 255))
    x = 0
    for s in sizes:
        sheet.alpha_composite(imgs[s], (x, 0)); x += s + 10
    sheet.save(os.path.join(OUT, 'preview-sheet.png'))
    print('wrote', OUT)

if __name__ == '__main__':
    main()
