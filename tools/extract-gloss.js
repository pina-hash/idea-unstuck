// Copies the SOLIDWORKS word list (the "gloss" hover definitions) out of the
// SolidWorks Day post into IdeaUnstuck.Core/gloss.json, so the add-in's hover help uses
// the same words as the post. Run: node tools/extract-gloss.js
const fs = require('fs'), path = require('path');
const root = path.join(__dirname, '..');
const h = fs.readFileSync(path.join(root, 'design/reference/IDEA100_D04_SolidWorks-Day.html'), 'utf8');
const i = h.indexOf('var D = '), j = h.indexOf(';\n', i);
const D = JSON.parse(h.slice(i + 8, j));
fs.writeFileSync(path.join(root, 'IdeaUnstuck.Core', 'gloss.json'), JSON.stringify(D.gloss, null, 1) + '\n');
console.log('gloss terms=' + Object.keys(D.gloss).length);
