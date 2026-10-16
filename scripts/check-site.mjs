import assert from 'node:assert/strict';
import { readFileSync, existsSync } from 'node:fs';

const read = file => readFileSync(new URL('../docs/' + file, import.meta.url), 'utf8');
const html = read('index.html');
const css = read('styles.css');
const js = read('main.js');
assert.match(html, /<html lang="pl">/, 'Polish fallback');
assert.match(html, /data-lang="en"/, 'English selector');
assert.match(html, /name="viewport"/, 'Responsive viewport');
assert.match(html, /CatLaser-0\.2\.1-win-x64\.zip/, 'Download URL');
assert.match(css, /prefers-reduced-motion/, 'Reduced motion support');
assert.match(js, /IntersectionObserver/, 'Pause offscreen rendering');
assert.match(js, /catlaser-language/, 'Persist locale preference');
assert.match(js, /function switchLanguage/, 'Live locale switch');
assert.ok(existsSync(new URL('../docs/.nojekyll', import.meta.url)), '.nojekyll marker');
const match = js.match(/const english=(\{[^\n]+\});/);
assert.ok(match, 'English dictionary');
const dictionary = JSON.parse(match[1]);
const used = [...html.matchAll(/data-i18n(?:-html)?="([^"]+)"/g)].map(x => x[1]);
for (const key of used) assert.ok(Object.hasOwn(dictionary, key), 'Missing translation: ' + key);
for (const key of [...html.matchAll(/href="#([^"]+)"/g)].map(x => x[1]))
  assert.ok(html.includes('id="' + key + '"'), 'Missing anchor: ' + key);
for (const file of ['styles.css','main.js','favicon.svg']) read(file);
console.log('Site OK. ' + Object.keys(dictionary).length + ' English keys, ' + used.length + ' translated nodes.');
