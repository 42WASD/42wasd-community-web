// 42WASD favicon active/inactive swap.
//
// While the page is first loading (SSR skeleton / WASM boot), the browser tab
// shows the GREY inactive icon (favicon-inactive.svg). Once the Blazor app has
// hydrated and real content is in the DOM, we swap it to the pink ACTIVE icon
// (favicon.svg).
//
// IMPORTANT: Chromium/Edge will NOT re-read the favicon if we just change
// `href` on an existing <link> element — they only pick it up when a brand-new
// <link> is inserted into <head>. So every swap appends NEW elements (and
// removes the old ones) rather than mutating existing links. This is what
// makes the pink icon actually appear.
//
// We swap BOTH formats (SVG + ICO). Chromium-family browsers cache favicons
// aggressively and prefer a multi-size .ico when present, so replacing both
// keeps the effect consistent everywhere. The `?v=` query busts the cache.
(function () {
  'use strict';

  var V = '5'; // bump this whenever the favicon artwork changes

  var ACTIVE_SVG = '/favicon.svg?v=' + V;
  var ACTIVE_ICO = '/favicon.ico?v=' + V;
  var INACTIVE_SVG = '/favicon-inactive.svg?v=' + V;
  var INACTIVE_ICO = '/favicon-inactive.ico?v=' + V;

  var MARKER = 'data-favicon-ready'; // attribute we set on <html> once done

  function setFavicon(svgHref, icoHref) {
    // Remove every icon link we previously added so the browser re-reads.
    document.querySelectorAll('link[data-favicon]').forEach(function (l) {
      l.remove();
    });
    var svg = document.createElement('link');
    svg.rel = 'icon';
    svg.type = 'image/svg+xml';
    svg.href = svgHref;
    svg.setAttribute('data-favicon', '');
    document.head.appendChild(svg);

    var ico = document.createElement('link');
    ico.rel = 'alternate icon';
    ico.type = 'image/x-icon';
    ico.href = icoHref;
    ico.setAttribute('data-favicon', '');
    document.head.appendChild(ico);
  }

  function isReady() {
    // "Ready" = the app replaced the SSR skeleton (no skeleton placeholders).
    return document.querySelectorAll('.rz-skeleton').length === 0;
  }

  function activate() {
    setFavicon(ACTIVE_SVG, ACTIVE_ICO);
    document.documentElement.setAttribute(MARKER, '');
    window.clearInterval(interval);
  }

  function tick() {
    if (isReady()) {
      // One frame so the final render settles before swapping.
      window.setTimeout(activate, 60);
    }
  }

  // Start grey immediately (in case SSR painted the grey icon already).
  setFavicon(INACTIVE_SVG, INACTIVE_ICO);
  var interval = window.setInterval(tick, 150);
  // Safety: never run forever.
  window.setTimeout(function () {
    if (!document.documentElement.hasAttribute(MARKER)) activate();
  }, 30000);
})();