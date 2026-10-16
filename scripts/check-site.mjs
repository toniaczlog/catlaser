import assert from 'node:assert/strict';
import { readFileSync, existsSync } from 'node:fs';

const read = file => readFileSync(new URL('../docs/' + file, import.meta.url), 'utf8');
const html = read('index.html');
const pl = read('pl/index.html');
const en = read('en/index.html');
const css = read('styles.css');
const js = read('main.js');

assert.match(html, /<html lang="pl">/, 'Polish fallback');
assert.match(html, /data-lang="en"/, 'English selector');
assert.match(pl, /<html lang="pl">/, 'Polish SEO route');
assert.match(en, /<html lang="en">/, 'English SEO route');
assert.match(pl, /rel="canonical"/, 'Polish canonical');
assert.match(en, /rel="canonical"/, 'English canonical');
assert.match(html, /name="viewport"/, 'Responsive viewport');
for (const localized of [html, pl, en]) {
  assert.match(localized, /settings-0\.3\.png/, 'Actual WPF settings screenshot on localized landing');
  assert.doesNotMatch(localized, /coming soon|w przygotowaniu/i, 'No screenshot placeholder');
}
assert.match(html, /CatLaser-0\.3-win-x64\.zip/, 'Download URL');
assert.match(css, /prefers-reduced-motion/, 'Reduced motion support');
assert.match(html, /id="watcher"/, 'Eye tracker is present');
assert.match(html, /class="watch-eyes"/, 'Minimalist pair of eyes is present');
assert.doesNotMatch(html, /class="cat"/, 'Legacy illustrated cat is removed');
assert.match(js, /function updateEyes/, 'Eyes track the laser');
assert.match(js, /recordActivity\(\)/, 'Eyes wake on user interaction');
assert.match(js, /is-blinking/, 'Blink state is animated');
assert.match(css, /\.eye-gaze/, 'Eye movement CSS is present');
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
assert.match(html,/data-motion="ricochet"/,'Preview of Ricochet');
assert.match(html,/id="demo-glow"/,'Glow slider');
assert.match(html,/id="demo-trail"/,'Trail slider');
assert.match(html,/id="demo-color"/,'Color selector');
assert.match(js,/renderPreviews/,'Mode preview renderer');
assert.match(html,/class="mobile-download"/,'Sticky download');
assert.match(html,/id="privacy"/,'Privacy panel');
assert.match(html,/id="updates"/,'Changelog');
assert.match(html,/id="press"/,'Press kit');
assert.match(html,/screensPending/,'Real screenshots explicitly pending');
for (const file of ['styles.css','main.js','favicon.svg','robots.txt','sitemap.xml',
    'press/mark.svg','press/wordmark.svg','press/shorts-card.svg','press/social-banner.svg'])
    read(file);
console.log('Site OK. ' + Object.keys(dictionary).length + ' English keys, ' + used.length + ' translated nodes.');
