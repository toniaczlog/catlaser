
(function(){
"use strict";
// Polish fallback lives in index.html, so the page remains usable without JavaScript.
const english={"skip":"Skip to content","navFeatures":"Features","navHow":"How it works","navDownload":"Download ↗","kicker":"TINY APP. BIG FUN.","heroTitle":"Your cursor.<br><em>Their new</em><br>obsession<span class=\"dot\">.</span>","heroIntro":"Turn your everyday mouse pointer into a glowing laser. Move it around the screen, try different effects, or let the light lead the chase.","download":"Download for free","free":"Free and open source","live":"INTERACTIVE DEMO","hint":"MOVE YOUR MOUSE HERE ↗","demoBottom":"THE EYES FOLLOW THE LASER. MOVE YOUR CURSOR.","effectsLabel":"TRY AN EFFECT","demoNote":"An interactive browser preview. The full app runs on your Windows desktop.","featuresKicker":"WAY MORE THAN A RED DOT","featuresTitle":"Tiny app.<br><em>Serious fun.</em>","featuresIntro":"Light effects, automatic cat chases, and complete control over the look. No ads, accounts, or subscriptions.","f1Title":"8 laser effects","f1Body":"Glows, comets, sparks, plasma, prism colors, and shockwaves. Pick your style.","f2Title":"5 cat play modes","f2Body":"The light runs, bounces, hides, and suddenly changes direction. With breaks and time limits.","f3Title":"Fully customizable","f3Body":"Color, speed, size, brightness, trails, and FPS. Save your favorite presets.","howKicker":"JUST THREE STEPS","howTitle":"Download.<br>Launch.<br><em>Chase!</em>","howBody":"No server setup. Just Windows, a mouse, and a curious cat.","s1Title":"Download the ZIP","s1Body":"Grab the free release on GitHub and extract the archive.","s2Title":"Run CatLaser.exe","s2Body":"No installer or separate .NET runtime. You'll also find it in the Windows system tray.","s3Title":"Pick a play mode","s3Body":"Move the laser yourself or start an automatic chase.","faqKicker":"GOOD TO KNOW","faqTitle":"Questions & answers.","q1":"Is CatLaser free?","a1":"Yes. Free, open source (MIT), with no ads or subscriptions.","q2":"Do I need to install it?","a2":"No. Download the ZIP, extract it, and run CatLaser.exe on Windows 10/11 x64.","q3":"Why might Windows SmartScreen warn me?","a3":"The executable is not digitally signed yet. Source code is public on GitHub and the app is built with GitHub Actions.","q4":"Is it safe for cats?","a4":"This is only an effect on a monitor, not a physical laser beam. Supervise your cat to prevent screen scratching, keep sessions short, and end play with a physical toy they can actually catch.","q5":"How do I turn off the laser?","a5":"Press Ctrl + Alt + L or use the Windows system tray. Exiting restores your cursors. If CatLaser crashes, restart it to restore the cursor scheme.","endingKicker":"YOUR CAT HAS PLANS TONIGHT","endingTitle":"Ready to<br><em>chase?</em>","prerelease":"Preview version · unsigned EXE","footer":"Made for cats, curious humans, and fans of glowing things."};
const polish={};
document.querySelectorAll("[data-i18n]").forEach(el=>polish[el.dataset.i18n]=el.textContent);
document.querySelectorAll("[data-i18n-html]").forEach(el=>polish[el.dataset.i18nHtml]=el.innerHTML);
const langs={pl:polish,en:english};
const buttons=Array.from(document.querySelectorAll("[data-lang]"));
let saved=null;try{saved=localStorage.getItem("catlaser-language")}catch(e){}
const preferred=((navigator.languages&&navigator.languages[0])||navigator.language||"en");
function switchLanguage(lang,save){
 if(!langs[lang])return;
 const t=langs[lang];document.documentElement.lang=lang;
 document.title=lang==="pl"?"CatLaser — Twój kursor. Jego obsesja.":"CatLaser — Your cursor. Their obsession.";
 const description=document.querySelector('meta[name="description"]');
 if(description)description.content=lang==="pl"?"Darmowy laserowy kursor Windows. Tryby zabawy dla kota i efekty świetlne.":"Free Windows laser pointer with glowing effects and cat chase modes.";
 const ogTitle=document.querySelector('meta[property="og:title"]'),ogDescription=document.querySelector('meta[property="og:description"]');
 if(ogTitle)ogTitle.content=document.title;
 if(ogDescription)ogDescription.content=description.content;
 document.querySelectorAll("[data-i18n]").forEach(el=>{if(t[el.dataset.i18n]!==undefined)el.textContent=t[el.dataset.i18n]});
 // All HTML translations are authored locally (never retrieved from third parties).
 document.querySelectorAll("[data-i18n-html]").forEach(el=>{if(t[el.dataset.i18nHtml]!==undefined)el.innerHTML=t[el.dataset.i18nHtml]});
 buttons.forEach(b=>b.setAttribute("aria-pressed",String(b.dataset.lang===lang)));
 document.getElementById("demo").setAttribute("aria-label",lang==="pl"?"Podgląd lasera. Porusz myszą lub dotknij.":"Laser preview. Move your mouse or tap here.");
 if(save)try{localStorage.setItem("catlaser-language",lang)}catch(e){}
}
buttons.forEach(b=>b.addEventListener("click",()=>switchLanguage(b.dataset.lang,true)));
switchLanguage(saved==="pl"||saved==="en"?saved:(/^pl(?:-|$)/i.test(preferred)?"pl":"en"),false);
const arena=document.getElementById("demo"),canvas=document.getElementById("laser-preview");
const watcher=document.getElementById("watcher");
if(!canvas||!arena)return;
const context=canvas.getContext("2d",{alpha:true});if(!context)return;
const still=window.matchMedia&&window.matchMedia("(prefers-reduced-motion: reduce)").matches;
let w=1,h=1,dpr=1,px=0,py=0,tx=0,ty=0,interacted=false,visible=true,frame=0,last=0,effect="real";
const trail=[];
let lastActivity=performance.now();
let gazeX=0,gazeY=0;
function recordActivity(){
 lastActivity=performance.now();
 if(watcher){watcher.classList.add("is-awake");watcher.classList.remove("is-idle")}
}
function updateEyes(time){
 if(!watcher)return;
 // Iris movement uses the SAME frame loop as the laser, with no extra animation timer.
 const targetX=Math.max(-10,Math.min(10,(px-w*.5)/Math.max(90,w*.34)*10));
 const targetY=Math.max(-7,Math.min(7,(py-h*.53)/Math.max(75,h*.38)*7));
 gazeX+=(targetX-gazeX)*.16;
 gazeY+=(targetY-gazeY)*.16;
 watcher.style.setProperty("--look-x",gazeX.toFixed(2)+"px");
 watcher.style.setProperty("--look-y",gazeY.toFixed(2)+"px");
 const inactive=interacted&&time-lastActivity>6500;
 watcher.classList.toggle("is-idle",inactive);
 watcher.classList.toggle("is-awake",interacted&&!inactive);
 const blinking=!still&&!inactive&&interacted&&(time%5300>4870&&time%5300<5020);
 watcher.classList.toggle("is-blinking",blinking);
}

function draw(time){
 context.setTransform(dpr,0,0,dpr,0,0);context.clearRect(0,0,w,h);
 updateEyes(time);
 if(effect!=="real"){
  for(let i=1;i<trail.length;i++){
   const a=trail[i-1],b=trail[i],alpha=i/trail.length;
   context.beginPath();context.moveTo(a.x,a.y);context.lineTo(b.x,b.y);
   context.strokeStyle="rgba(255,61,85,"+(alpha*.8)+")";
   context.lineWidth=effect==="comet"?1+10*alpha*alpha:1.2*alpha+0.6;
   context.lineCap="round";context.stroke();
  }
 }
 const halo=context.createRadialGradient(px,py,0,px,py,47);
 halo.addColorStop(0,"rgba(255,255,250,1)");
 halo.addColorStop(.09,"rgba(255,62,88,.95)");
 halo.addColorStop(.22,"rgba(255,30,66,.48)");
 halo.addColorStop(.6,"rgba(255,20,50,.12)");
 halo.addColorStop(1,"rgba(255,20,50,0)");
 context.fillStyle=halo;context.beginPath();context.arc(px,py,47,0,Math.PI*2);context.fill();
 if(effect==="electric"){
  for(let a=0;a<4;a++){
   const angle=a*Math.PI/2+time*.0007;
   context.beginPath();context.moveTo(px,py);
   for(let s=1;s<=5;s++){
    const r=s*7,jitter=Math.sin(time*.023+a*12+s*17)*7;
    context.lineTo(px+Math.cos(angle)*r-Math.sin(angle)*jitter,py+Math.sin(angle)*r+Math.cos(angle)*jitter);
   }
   context.lineWidth=1.3;context.strokeStyle="rgba(255,165,183,.8)";context.stroke();
  }
 }
 context.beginPath();context.arc(px,py,3.3,0,Math.PI*2);context.fillStyle="#fffefa";context.fill();
}
function resize(){
 const rect=arena.getBoundingClientRect();
 if(!rect.width||!rect.height)return;
 w=rect.width;h=rect.height;dpr=Math.min(devicePixelRatio||1,2);
 canvas.width=Math.round(w*dpr);canvas.height=Math.round(h*dpr);
 if(!interacted){px=tx=w*.35;py=ty=h*.39}
 draw(performance.now());
}
function move(x,y){
 const rect=arena.getBoundingClientRect();
 tx=Math.max(12,Math.min(w-12,x-rect.left));
 ty=Math.max(12,Math.min(h-12,y-rect.top));
 interacted=true;recordActivity();
 if(still){px=tx;py=ty;trail.length=0;draw(performance.now())}
}
arena.addEventListener("pointermove",e=>move(e.clientX,e.clientY));
arena.addEventListener("pointerdown",e=>move(e.clientX,e.clientY));
arena.addEventListener("keydown",e=>{
 if(e.key==="Enter"||e.key===" "){
  e.preventDefault();tx=20+Math.random()*Math.max(1,w-40);ty=20+Math.random()*Math.max(1,h-40);interacted=true;recordActivity();
  if(still){px=tx;py=ty;draw(performance.now())}
 }else if(e.key.startsWith("Arrow")){
  e.preventDefault();tx=Math.max(12,Math.min(w-12,tx+(e.key==="ArrowLeft"?-24:e.key==="ArrowRight"?24:0)));
  ty=Math.max(12,Math.min(h-12,ty+(e.key==="ArrowUp"?-24:e.key==="ArrowDown"?24:0)));interacted=true;recordActivity();
  if(still){px=tx;py=ty;draw(performance.now())}
 }
});
document.querySelectorAll("[data-fx]").forEach(b=>b.addEventListener("click",()=>{
 effect=b.dataset.fx;
 document.querySelectorAll("[data-fx]").forEach(s=>{const active=s===b;s.classList.toggle("selected",active);s.setAttribute("aria-pressed",String(active))});
 draw(performance.now());
}));
function tick(time){
 frame=0;if(!visible||document.hidden||still)return;
 if(time-last>16){
  last=time;
  if(!interacted){tx=w*(.37+Math.sin(time*.00038)*.1);ty=h*(.39+Math.cos(time*.00031)*.1)}
  px+=(tx-px)*.21;py+=(ty-py)*.21;
  if(!trail.length||Math.hypot(px-trail[trail.length-1].x,py-trail[trail.length-1].y)>1){
   trail.push({x:px,y:py});if(trail.length>25)trail.shift();
  }
  draw(time);
 }
 frame=requestAnimationFrame(tick);
}
function setVisibility(value){
 visible=value;
 if(!visible&&frame){cancelAnimationFrame(frame);frame=0}
 else if(visible&&!document.hidden&&!still&&!frame)frame=requestAnimationFrame(tick);
}
if(window.IntersectionObserver)new IntersectionObserver(list=>setVisibility(list.some(e=>e.isIntersecting)),{threshold:.03}).observe(arena);
document.addEventListener("visibilitychange",()=>{if(document.hidden&&frame){cancelAnimationFrame(frame);frame=0}else setVisibility(visible)});
if(window.ResizeObserver)new ResizeObserver(resize).observe(arena);else window.addEventListener("resize",resize);
resize();if(!still)setVisibility(true);
})();
