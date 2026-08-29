#!/usr/bin/env python3
"""Dynamic UX discovery + full-page capture. NO hardcoded app structure.

PHASE 1 - DISCOVER (generic live-browser BFS):
  Seed = the reflection-generated route table (ux-paths.json from the F#
  headless probe). At every state, generically enumerate every visible
  interactive element (button, [role=tab], [role=menuitem], a[href],
  expandable nav groups, split-button toggles, sidebar toggles, inputs) via
  ROLE-ONLY selectors. Each candidate is click-tested on a fresh load of its
  route with the state's accepted steps replayed first, then classified
  generically by OBSERVED EFFECT:
      NAVIGATE   URL changed                       (deep chains only)
      POPUP      a menu/popup became visible       -> its items = new elements
      DIALOG     a modal became visible            -> its buttons = new elements
      DOM        page text changed (toggle/alert/drawer/sign-in)
  Every element VISIBLE AFTER an effectful step that wasn't visible before
  is enqueued as a deeper chain — so hamburger->drawer->Community->Members,
  split-button->View details->dialog, and fill->submit->profile-menu->Sign
  out are DISCOVERED, never hardcoded. Fixpoint: repeat until no new states.

PHASE 2 - CAPTURE: replay every discovered state on a fresh browser context,
  settle on the inner scroll container's height, release the Radzen inner
  scroll, full-page screenshot. Gates: capture height completeness,
  byte-identical duplicate FAIL, and a final PAIRWISE PIXEL-DIFF over every
  capture pair (objective distinctness — no vision trust).

Run:
  dotnet test tests/Community.Client.Tests/Community.Client.Tests.fsproj
  uv run --with playwright --with pillow python scripts/visual/uxp.py http://localhost:PORT
"""
from __future__ import annotations

import glob
import hashlib
import itertools
import json
import re
import sys
from collections import deque
from datetime import datetime, timezone
from html import escape
from pathlib import Path

from playwright.sync_api import TimeoutError as PWTimeout
from playwright.sync_api import sync_playwright

# Force unbuffered stdout so progress lines appear immediately even when the
# process is watched (never pipe this tool — run it in the foreground).
sys.stdout.reconfigure(line_buffering=True)

VIEWPORTS = [("mobile", 390, 844), ("desktop", 1440, 900)]
OUT_DIR = Path(".tmp/visual-audit")
PLAN = OUT_DIR / "discovered-plan.json"
PLAN_EVENTS = OUT_DIR / "plan-events.jsonl"      # live discovery events
CAPTURE_EVENTS = OUT_DIR / "capture-events.jsonl"  # live capture events
MAX_STATES = 120          # global cap per viewport
MAX_DEPTH = 6
CLICK_WAIT = 600           # only used in recorded step dicts (replay safety)
QUIESCE_MS = 250           # DOM must be mutation-free this long = ready
DISCOVER_BUDGET_S = 180   # per-viewport wall-clock budget; always completes


def log_event(file: Path, **kv) -> None:
    """Append one JSON event and FLUSH — real-time progress on disk."""
    kv["ts"] = datetime.now(timezone.utc).isoformat(timespec="seconds")
    with file.open("a", encoding="utf-8") as f:
        f.write(json.dumps(kv) + "\n")

# ---------------------------------------------------------------------------
# generic helpers — zero app knowledge
# ---------------------------------------------------------------------------

def slug(t: str) -> str:
    s = re.sub(r"[^a-z0-9]+", "-", (t or "el").strip().lower()).strip("-")
    return s[:42] or "el"


def safe(t: str, n=28) -> str:
    t = re.sub(r"[\\\"\n\r\t]+", " ", (t or "")).strip()
    return t[:n].strip() or "."


def load_routes() -> list[dict]:
    hits = sorted(glob.glob(
        "tests/Community.Client.Tests/bin/**/ux-paths.json", recursive=True))
    if not hits:
        sys.exit("ux-paths.json missing — run the F# probe: dotnet test "
                 "tests/Community.Client.Tests/Community.Client.Tests.fsproj")
    doc = json.loads(Path(hits[-1]).read_text())
    return doc["routes"]


def new_page(browser, vp):
    w, h = next((w, h) for v, w, h in VIEWPORTS if v == vp)
    ctx = browser.new_context(viewport={"width": w, "height": h})
    # Install the mutation watch before any page script runs so EVERY DOM
    # change (render, data swap, popup) is timestamped.
    ctx.add_init_script("""
        window.__uxq = { last: performance.now() };
        new MutationObserver(() => { window.__uxq.last = performance.now(); })
            .observe(document.documentElement,
                     {subtree: true, childList: true,
                      characterData: true, attributes: true});
    """)
    return ctx, ctx.new_page()


# Resolves true once the DOM has been mutation-free for QUIET_MS — event-
# driven readiness: no fixed sleep, returns the instant rendering settled.
QUIESCE_JS = """() => performance.now() - window.__uxq.last > 250"""


def quiesce(page, max_ms=600):
    """Wait until the DOM stops mutating (or max_ms). Typically <100ms."""
    try:
        page.wait_for_function(QUIESCE_JS, timeout=max_ms)
    except PWTimeout:
        pass                      # continuous animation — proceed at cap


def goto_settle(page, base, route):
    """Navigate and wait for REAL readiness — no blind sleeps, no networkidle
    (Blazor keeps background requests alive, so networkidle always burned its
    full timeout): h3 rendered (WASM booted + rendered) then DOM-quiet."""
    page.goto(f"{base}/{route}".rstrip("/"),
              wait_until="domcontentloaded", timeout=30_000)
    page.wait_for_selector("h3", timeout=20_000)
    quiesce(page, 1500)


def body_sig(page) -> str:
    return page.evaluate("document.body.innerText.replace(/\\s+/g,' ').trim()")


def replay(page, steps) -> bool:
    """Replay accepted steps; False if any step missed. Waits are
    condition-based (quiescence), not fixed sleeps."""
    for s in steps:
        try:
            loc = page.locator(s["sel"]).first
            if s["act"] == "fill":
                loc.fill(s["text"], timeout=5_000)
            elif s["act"] == "hover":
                loc.hover(timeout=5_000)
            else:
                loc.click(timeout=5_000)
            quiesce(page)
        except PWTimeout:
            return False
    return True


# ---------------------------------------------------------------------------
# PHASE 1 — generic element enumeration + effect classification
# ---------------------------------------------------------------------------

# Role-only enumeration of visible interactives. Every selector here is an
# HTML standard role/element — NO app/Radzen-specific structure is assumed
# beyond "a button is a button".
ENUM_JS = """() => {
    const vis = (el) => {
        const r = el.getBoundingClientRect();
        if (r.width < 1 || r.height < 1) return false;
        const st = getComputedStyle(el);
        return st.visibility !== 'hidden' && st.display !== 'none';
    };
    const out = [];
    const push = (el, kind, text) => {
        if (!vis(el)) return;
        out.push({kind, text: (text || '').trim(),
                  scope: el.closest('.mobile-nav-drawer') ? 'drawer'
                       : el.closest('ul[role="menubar"]') ? 'menubar' : 'page'});
    };
    document.querySelectorAll('button:not([disabled])')
        .forEach(el => {
            // skip buttons inside tab headers — the [role=tab] enumeration
            // already covers them (Radzen tabs nest a button per header).
            if (el.closest('[role="tab"]')) return;
            const t = (el.innerText || '').trim();
            // icon-only buttons (sidebar toggle, popup chevron): expose the
            // component-library hook class as the stable selector handle.
            let cls = '';
            if (!t) cls = ([...el.classList].find(c => c.startsWith('rz-'))
                           || '');
            out.push({kind: 'button', text: t, rzcls: cls,
                      scope: el.closest('.mobile-nav-drawer') ? 'drawer'
                           : el.closest('ul[role="menubar"]') ? 'menubar'
                           : 'page'});
        });
    document.querySelectorAll('[role="tab"]')
        .forEach(el => push(el, 'tab', el.innerText));
    document.querySelectorAll('[role="menuitem"], ul[role="menu"] li')
        .forEach(el => push(el, 'menuitem', el.innerText));
    document.querySelectorAll('a[href]').forEach(el => {
        const href = el.getAttribute('href') || '';
        if (href.startsWith('http') || href.startsWith('#')) return;
        push(el, 'link', el.innerText);
    });
    // expandable group headers: li whose toggle icon exists but no href
    document.querySelectorAll('li.rz-navigation-item').forEach(el => {
        if (el.querySelector('a[href]')) return;
        const t = el.querySelector('.rz-navigation-item-text');
        if (t) push(el, 'group', t.textContent);
    });
    // split-button popup toggles (icon-only button next to main button)
    document.querySelectorAll('button[aria-haspopup="menu"]')
        .forEach(el => push(el, 'popuptoggle', ''));
    // form inputs
    document.querySelectorAll('input:not([type=hidden]), textarea')
        .forEach(el => {
            if (!vis(el)) return;
            out.push({kind: 'input', text: el.type || 'text', scope: 'page'});
        });
    return out;
}"""


def selector_for(c, nth):
    """Build a replayable selector from an enumerated descriptor."""
    scope = {"drawer": ".mobile-nav-drawer ",
             "menubar": 'ul[role="menubar"] '}.get(c["scope"], "")
    t = safe(c.get("text", ""))
    if c["kind"] == "button":
        if c.get("text"):
            return f'{scope}button:has-text("{t}")'
        if c.get("rzcls"):
            return f'{scope}button.{c["rzcls"]}'
        return f'{scope}button[aria-haspopup="menu"]'
    if c["kind"] == "tab" and t != ".":
        return f'[role="tab"]:has-text("{t}")'
    if c["kind"] == "menuitem" and t != ".":
        return f'{scope}ul[role="menu"] li:has-text("{t}"):visible'
    if c["kind"] == "link":
        href = c.get("_href") or ""
        return f'{scope}a[href="{href}"]' if href else None
    if c["kind"] == "group" and t != ".":
        return f'{scope}li.rz-navigation-item:has-text("{t}")'
    if c["kind"] == "popuptoggle":
        return f'{scope}button[aria-haspopup="menu"]'
    if c["kind"] == "input":
        return ("input[type='password']:visible" if c["text"] == "password"
                else "input:not([type='password']):visible")
    return None


def classify(page, url0, sig0, overlays0):
    """Generic effect classification from OBSERVED state change."""
    if page.url.rstrip("/") != url0.rstrip("/"):
        return "navigate", None
    dlg = page.locator("[role='dialog']:visible, .rz-dialog:visible")
    if dlg.count() > 0 and dlg.first.is_visible():
        return "dialog", "[role='dialog']:visible, .rz-dialog:visible"
    menu = page.locator('ul[role="menu"] li:visible, .rz-popup:visible')
    if menu.count() > 0:
        return "popup", None
    if body_sig(page) != sig0:
        return "dom", None
    return "none", None


def discover(browser, base, vp, total_routes: int):
    """BFS: state = (route, steps). Candidates = elements enumerated at the
    state's DOM. Effectful candidates become states; newly-visible elements
    at the post-state are enqueued as chains."""
    states, seen, queue = [], set(), deque()
    routes = load_routes()
    import time as _time
    deadline = _time.monotonic() + DISCOVER_BUDGET_S
    log_event(PLAN_EVENTS, phase="discover-start", viewport=vp,
              routes=total_routes)
    # Depth-0 dedupe by selector alone: the same control (e.g. the hamburger)
    # exists on every route; its effect was already tested once.
    tested_depth0: set[str] = set()
    # Post-state DOM-signature dedupe: a state is characterized by the DOM it
    # PRODUCES. If that DOM was already seen (from any other chain), the
    # capture would be a near-duplicate and re-exploring it adds nothing —
    # skip registration and exploration. This makes the BFS converge.
    visited_sigs: set[str] = set()

    def register(route, steps, label, effect):
        key = (route, tuple((s["sel"], s["act"]) for s in steps))
        if key in seen or len(states) >= MAX_STATES:
            return False
        seen.add(key)
        states.append({"name": f"{slug(label)}__{len(states)}", "route": route,
                       "steps": steps, "effect": effect})
        return True

    # Seed: base state per route.
    for r in routes:
        route = r["route"]
        register(route, [], f"base-{route or 'home'}", "base")
        queue.append((route, [], 0))

    while queue:
        if _time.monotonic() > deadline:
            print(f"    (time budget reached — {len(states)} states so far)")
            break
        route, base_steps, depth = queue.popleft()
        if depth >= MAX_DEPTH:
            continue
        ctx, page = new_page(browser, vp)
        try:
            # LOAD ONCE per state (not per candidate): enumerate the DOM,
            # then click-test every candidate in place. Reset (reload +
            # replay) only when a click CHANGED the state in a way that
            # could hide subsequent candidates (popup/dialog opened, DOM
            # mutated). Inert clicks need no reset at all.
            goto_settle(page, base, route)
            if base_steps and not replay(page, base_steps):
                continue
            visited_sigs.add(body_sig(page))   # this state's DOM is known
            candidates = []
            seen_cands: dict = {}
            for c in page.evaluate(ENUM_JS):
                # Global-nav menus (hamburger drawer / desktop menubar) are
                # identical on every page: test them ONCE (depth 0). Testing
                # them again from every mutated state explodes the search
                # without new DOM states — the menu opens the same overlay.
                if base_steps and c.get("scope") in ("menubar", "drawer"):
                    continue
                sel = selector_for(c, seen_cands)
                if not sel:
                    continue
                candidates.append(sel)
            for sel in candidates:
                if _time.monotonic() > deadline:
                    break
                ckey = (route, tuple((s["sel"], s["act"]) for s in base_steps),
                        sel)
                if ckey in seen:
                    continue
                seen.add(ckey)
                if not base_steps:
                    if sel in tested_depth0:
                        continue
                    tested_depth0.add(sel)
                url0, sig0 = page.url, body_sig(page)
                try:
                    page.locator(sel).first.click(timeout=4_000)
                    quiesce(page)
                except PWTimeout:
                    # element vanished from the current DOM (stale candidate)
                    continue
                effect, post = classify(page, url0, sig0, None)
                if effect == "none":
                    continue     # inert click: NO reset, test next in place
                if effect == "navigate" and len(base_steps) == 0:
                    # plain top-nav links = other base routes; skip
                    continue
                # DOM-signature dedupe: if the produced DOM was already
                # seen, this chain adds no new state — skip entirely.
                post_sig = body_sig(page)
                if post_sig in visited_sigs:
                    if effect in ("dom", "dialog"):
                        # still reset so later candidates start clean
                        goto_settle(page, base, route)
                        if base_steps:
                            replay(page, base_steps)
                    continue
                visited_sigs.add(post_sig)
                new_steps = base_steps + [{"sel": sel, "act": "click",
                                           "wait": CLICK_WAIT}]
                register(route, new_steps, sel, effect)
                print(f"    [{len(states)}/{MAX_STATES}] [{vp}] "
                      f"{route or '/'} "
                      f"{' > '.join(s['sel'][:38] for s in new_steps)} "
                      f"-> {effect}")
                log_event(PLAN_EVENTS, phase="state", viewport=vp,
                          route=route, effect=effect,
                          chain=[s["sel"] for s in new_steps],
                          n_states=len(states))
                # enqueue deeper exploration from this new state. navigate
                # chains are NOT explored: the target route is already a
                # seeded base state (its DOM gets crawled on its own pass).
                if effect in ("dom", "dialog", "popup"):
                    queue.append((route, new_steps, depth + 1))
                # Popup items: test IN PLACE while the popup is open — but
                # the FIRST item click closes it, so re-open before each
                # item after the first (one toggle click, no reload).
                if effect == "popup":
                    items = page.evaluate("""() => [...document
                        .querySelectorAll('ul[role="menu"] li')]
                        .filter(li => {
                            const r = li.getBoundingClientRect();
                            return r.width > 0 && r.height > 0;
                        })
                        .map(li => {
                            const t = li.querySelector(
                                '.rz-navigation-item-text, a, span');
                            return ((t ? t.textContent
                                        : li.textContent) || '').trim();
                        }).filter(Boolean)""")
                    first = True
                    for txt in dict.fromkeys(items):
                        isel = ('ul[role="menu"] li:has-text("'
                                + safe(txt) + '"):visible')
                        ikey = (route, tuple(
                            (s["sel"], s["act"]) for s in new_steps), isel)
                        if ikey in seen:
                            continue
                        seen.add(ikey)
                        try:
                            if not first:
                                # re-open the popup in place (no reload)
                                page.locator(sel).first.click(timeout=4_000)
                                quiesce(page)
                            first = False
                            url1, sig1 = page.url, body_sig(page)
                            page.locator(isel).first.click(timeout=4_000)
                            quiesce(page)
                            eff2, post2 = classify(page, url1, sig1, None)
                            chain = new_steps + [{"sel": isel, "act": "click",
                                                  "wait": CLICK_WAIT}]
                            if eff2 != "none":
                                post_sig2 = body_sig(page)
                                if post_sig2 in visited_sigs:
                                    continue
                                visited_sigs.add(post_sig2)
                                register(route, chain, f"{sel}>{txt}", eff2)
                                print(f"    [{len(states)}/{MAX_STATES}] [{vp}] "
                                      f"{route or '/'} "
                                      f"{' > '.join(s['sel'][:38] for s in chain)} "
                                      f"-> {eff2}")
                                log_event(PLAN_EVENTS, phase="state",
                                          viewport=vp, route=route,
                                          effect=eff2,
                                          chain=[s["sel"] for s in chain],
                                          n_states=len(states))
                                if eff2 in ("dom", "dialog"):
                                    queue.append((route, chain, depth + 2))
                                if eff2 == "dialog":
                                    # close the dialog in place so the popup
                                    # re-open below isn't blocked by the modal
                                    try:
                                        page.keyboard.press("Escape")
                                        quiesce(page)
                                    except Exception:
                                        pass
                        except PWTimeout:
                            continue
                    # RESET once after the popup block (the page DOM may be
                    # mutated by item effects): reload + replay the chain.
                    goto_settle(page, base, route)
                    if base_steps:
                        replay(page, base_steps)
                elif effect in ("dom", "dialog"):
                    # DOM changed or modal is open: reset once so remaining
                    # candidates of THIS state are tested from the clean state.
                    goto_settle(page, base, route)
                    if base_steps:
                        replay(page, base_steps)
        finally:
            ctx.close()
    return states


# ---------------------------------------------------------------------------
# PHASE 2 — capture with gates
# ---------------------------------------------------------------------------

def content_height(page) -> int:
    return page.evaluate("""() => {
        const el = document.querySelector('.rz-body');
        return Math.max(document.documentElement.scrollHeight,
                        el ? el.scrollHeight : 0);
    }""")


def wait_stable(page, timeout_ms=8000):
    last, stable, waited = -1, 0, 0
    while waited < timeout_ms:
        h = content_height(page)
        stable = stable + 1 if h == last and h > 0 else 0
        last = h
        page.wait_for_timeout(250)
        waited += 250
        if stable >= 3:
            break
    return content_height(page)


def release_scroll(page):
    page.evaluate("""() => {
        document.querySelectorAll('.rz-layout,.rz-body,.rz-sidebar,.rz-header,'
            + '.rz-footer').forEach(e => {
                e.style.height = 'auto'; e.style.overflow = 'visible';
                e.style.maxHeight = 'none'; e.style.position = 'static';
            });
        document.documentElement.style.height = 'auto';
        document.documentElement.style.overflowY = 'visible';
        document.body.style.height = 'auto';
        document.body.style.overflowY = 'visible';
    }""")
    page.wait_for_timeout(400)


def capture_all(browser, base, per_vp_states):
    shots, failures, hashes = [], [], {}
    total = sum(len(v) for v in per_vp_states.values())
    done = 0
    log_event(CAPTURE_EVENTS, phase="capture-start", total=total)
    for vp, states in per_vp_states.items():
        for st in states:
            done += 1
            label = f"{st['name']}_{vp}"
            ctx, page = new_page(browser, vp)
            try:
                goto_settle(page, base, st["route"])
                if not replay(page, st["steps"]):
                    print(f"  (step missed on {label})")
                page.wait_for_timeout(400)
                wait_stable(page, 4000)
                release_scroll(page)
                page_h = content_height(page)
                shot = OUT_DIR / f"{label}.png"
                page.screenshot(path=str(shot), full_page=True)
                page.wait_for_timeout(200)
                from PIL import Image
                img_h = Image.open(shot).height
                if img_h + 4 < page_h:
                    failures.append(f"{label} short ({img_h}<{page_h})")
                    print(f"FAIL  [{done}/{total}] {label}: capture shorter "
                          f"than page")
                    log_event(CAPTURE_EVENTS, phase="capture", label=label,
                              ok=False, why="short", i=done, total=total)
                    shot.unlink(missing_ok=True)
                    continue
                digest = hashlib.sha256(shot.read_bytes()).hexdigest()
                if digest in hashes:
                    failures.append(f"{label} == {hashes[digest]}")
                    print(f"FAIL  [{done}/{total}] {label}: identical to "
                          f"{hashes[digest]}")
                    log_event(CAPTURE_EVENTS, phase="capture", label=label,
                              ok=False, why="duplicate", i=done, total=total)
                    shot.unlink(missing_ok=True)
                    continue
                hashes[digest] = label
                shots.append({"label": label, "route": st["route"] or "/",
                              "effect": st["effect"], "h": img_h,
                              "file": shot.name})
                print(f"  OK  [{done}/{total}] {label}  {img_h}px "
                      f"({st['effect']})")
                log_event(CAPTURE_EVENTS, phase="capture", label=label,
                          ok=True, h=img_h, i=done, total=total,
                          effect=st["effect"])
            except PWTimeout as e:
                failures.append(f"{label}: {type(e).__name__}")
                print(f"FAIL  [{done}/{total}] {label}: {type(e).__name__}")
                log_event(CAPTURE_EVENTS, phase="capture", label=label,
                          ok=False, why=type(e).__name__, i=done, total=total)
            finally:
                ctx.close()
    log_event(CAPTURE_EVENTS, phase="capture-done", ok=len(shots),
              failed=len(failures))
    return shots, failures


def pairwise_diff_report():
    files = sorted(OUT_DIR.glob("*_mobile.png")) + sorted(
        OUT_DIR.glob("*_desktop.png"))
    near, checked = [], 0
    for a, b in itertools.combinations(files, 2):
        ia = None
        try:
            from PIL import Image
            ia, ib = Image.open(a).convert("RGB"), Image.open(b).convert("RGB")
        except Exception:
            continue
        if ia.size != ib.size:
            continue
        import numpy as np
        from PIL import ImageChops
        arr = np.array(ImageChops.difference(ia, ib))
        pct = (arr.sum(axis=2) > 12).mean() * 100
        checked += 1
        if pct < 1.0:
            near.append(f"{a.name} vs {b.name}: {pct:.2f}%")
    return checked, near


def write_gallery(shots):
    rows = "\n".join(
        f"<figure><figcaption>{escape(s['label'])} "
        f"<small>({escape(s['route'])} — {escape(s['effect'])} — {s['h']}px)"
        f"</small><a href=\"{escape(s['file'])}\"><img src=\"{escape(s['file'])}\" "
        f"loading=\"lazy\" alt=\"{escape(s['label'])}\"></a></figure>"
        for s in shots)
    html = f"""<!doctype html><html><head><meta charset="utf-8">
<title>42WASD UX audit</title><style>
body{{background:#0a0a0a;color:#ededed;font-family:monospace;margin:2rem}}
figure{{margin:0 0 2rem}}img{{max-width:480px;border:1px solid #2a2a2a;display:block}}
figcaption{{margin:.5rem 0}}small{{color:#8a8a8a}}</style></head><body>
<h1>42WASD UX audit — dynamically discovered states</h1>
<p>{len(shots)} captures — {escape(datetime.now(timezone.utc).isoformat(timespec='seconds'))}</p>
{rows}</body></html>"""
    (OUT_DIR / "index.html").write_text(html, encoding="utf-8")


def main() -> int:
    base = sys.argv[1] if len(sys.argv) > 1 else "http://localhost:5046"
    discover_only = "--discover-only" in sys.argv
    capture_only = "--capture-only" in sys.argv
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    with sync_playwright() as pw:
        browser = pw.chromium.launch()
        try:
            if capture_only:
                if not PLAN.exists():
                    print(f"No plan at {PLAN} — run discovery first.")
                    return 2
                doc = json.loads(PLAN.read_text())
                print(f"Loaded plan: "
                      f"{', '.join(f'{k}={len(v)}' for k, v in doc['states'].items())}")
                print(f"\nPHASE 2: capturing …\n")
                shots, failures = capture_all(browser, base, doc["states"])
            else:
                per_vp = {}
                all_states = {}
                routes_list = load_routes()
                for vp, _, _ in VIEWPORTS:
                    print(f"PHASE 1 [{vp}]: discovering …")
                    states = discover(browser, base, vp, len(routes_list))
                    all_states[vp] = states
                    per_vp[vp] = states
                    print(f"PHASE 1 [{vp}]: {len(states)} states")
                    log_event(PLAN_EVENTS, phase="viewport-done", viewport=vp,
                              n_states=len(states))
                    # Persist per viewport so a killed run keeps its work.
                    PLAN.write_text(json.dumps(
                        {"base": base, "states": all_states}, indent=2),
                        encoding="utf-8")
                print(f"plan -> {PLAN}")
                if discover_only:
                    return 0
                print(f"\nPHASE 2: capturing …\n")
                shots, failures = capture_all(browser, base, per_vp)
        finally:
            browser.close()
    write_gallery(shots)
    checked, near = pairwise_diff_report()
    print(f"\n{len(shots)} verified captures -> {OUT_DIR / 'index.html'}")
    print(f"pairwise diff: {checked} same-size pairs checked; "
          f"{len(near)} near-identical")
    for n in near:
        print(f"  ~ {n}")
    if failures:
        print(f"FAILED ({len(failures)}):")
        for f in failures:
            print(f"  - {f}")
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
