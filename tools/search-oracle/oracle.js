// Runs the SolidWorks Day post's own search code (extracted verbatim from
// design/reference/IDEA100_D04_SolidWorks-Day.html) over design/kb-v2.json and
// prints the top five titles for every pinned query. It is the reference the
// C# port in IdeaUnstuck.Core/SearchEngine.cs is checked against.
const fs = require('fs'), path = require('path');
const root = path.join(__dirname, '..', '..');
const html = fs.readFileSync(path.join(root, 'design/reference/IDEA100_D04_SolidWorks-Day.html'), 'utf8');
const js = html.slice(html.indexOf('/* search: any word counts'), html.indexOf('  var shown = [], act = 0;'));
const D = JSON.parse(fs.readFileSync(path.join(root, 'design/kb-v2.json'), 'utf8'));
const E = D.entries;
const run = new Function('D', 'E', js + '\nreturn { score: score, toks: toks, norm: norm };');
const S = run(D, E);
function search(v) {
  const qt = S.toks(v), qn = ' ' + S.norm(v).replace(/\s+/g, ' ') + ' ';
  if (!qt.length) return [];
  return E.map(x => [S.score(x, qt, qn), x]).filter(p => p[0] >= 2).sort((a, b) => b[0] - a[0]).slice(0, 5);
}
const pins = JSON.parse(fs.readFileSync(path.join(__dirname, 'pins.json'), 'utf8'));
let fail = 0;
const extra = process.argv.slice(2);
for (const [q, want] of pins.concat(extra.map(q => [q, null]))) {
  const r = search(q);
  const top = r.length ? r[0][1].title : '(nothing)';
  const ok = want === null ? (r.length === 0 ? 'NONE' : 'INFO') : top === want ? 'PASS' : 'FAIL';
  if (ok === 'FAIL') fail++;
  console.log(ok + ' ' + JSON.stringify(q) + ' -> ' + r.map(p => p[1].id + '=' + p[0].toFixed(2)).join(' | '));
}
console.log('oracle pins failed=' + fail);
if (process.env.WRITE_EXPECTED) {
  // Corpus for the C# port: the pins, every title, every student phrasing,
  // and a few typo, synonym, contraction and nonsense queries.
  const qs = pins.map(p => p[0]);
  E.forEach(x => { qs.push(x.title); x.q.split(';').forEach(p => qs.push(p.trim())); });
  qs.push('zzqx blorp', '', '   ', 'IT WON\'T LET ME EXTRUD!', 'opne contoru', 'skech', 'dimenshun', 'the the the', 'grey grey', 'cant cant extrude', 'what does mid plane mean', 'fillett', 'extrusions', 'boxes');
  const seen = new Set(), out = [];
  for (const q of qs) { if (seen.has(q)) continue; seen.add(q); out.push({ q: q, top: search(q).map(p => [p[1].id, Math.round(p[0] * 1e6) / 1e6]) }); }
  fs.writeFileSync(path.join(__dirname, 'expected.json'), JSON.stringify(out) + '\n');
  console.log('expected queries=' + out.length);
}
process.exit(fail ? 1 : 0);
