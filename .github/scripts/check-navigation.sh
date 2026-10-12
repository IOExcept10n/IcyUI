#!/usr/bin/env bash
# Local check that the built site's navigation works: serves docfx/_site over HTTP, renders pages in headless Edge
# and asserts the rendered DOM has the navbar sections, the page's section sidebar and the EN/RU link. Not run in CI
# (it needs a browser). Usage: check-navigation.sh   (after build-docs.sh)
set -euo pipefail

port=8089
sections=(concepts controls guides project)
edge="/c/Program Files (x86)/Microsoft/Edge/Application/msedge.exe"
[[ -x "$edge" ]] || edge="/c/Program Files/Microsoft/Edge/Application/msedge.exe"
[[ -x "$edge" ]] || { echo "Microsoft Edge not found"; exit 2; }

dotnet docfx serve docfx/_site --port "$port" > /dev/null 2>&1 &
server=$!
profile=$(mktemp -d)
trap 'kill "$server" 2>/dev/null || true; rm -rf "$profile"' EXIT
for _ in $(seq 1 30); do curl -s -o /dev/null "http://localhost:$port/" && break; sleep 1; done

failures=0
render() {
  "$edge" --headless=new --disable-gpu --user-data-dir="$profile" --virtual-time-budget=8000 --dump-dom "$1" 2>/dev/null
}
expect() { # page-url, description, regex
  if ! grep -qE "$3" <<< "$dom"; then echo "FAIL $1: no $2"; failures=$((failures + 1)); fi
}

for lang in "" "ru/"; do
  for section in "${sections[@]}"; do
    url="http://localhost:$port/${lang}${section}/index.html"
    dom=$(render "$url")
    for other in "${sections[@]}"; do
      expect "$url" "navbar link to ${lang}${other}/" "class=\"nav-link[^>]*href=\"[^\"]*/${lang}${other}/"
    done
    # The sidebar is the <nav id="toc"> element; it must list the section's own Overview page. Both the navbar and
    # the sidebar render absolute hrefs, so the check looks inside that element only.
    sidebar=$(tr '\n' ' ' <<< "$dom" | grep -oE 'id="toc".*' | sed 's#</nav>.*##' || true)
    if ! grep -qE "href=\"[^\"]*/${lang}${section}/index\.html\"" <<< "$sidebar"; then
      echo "FAIL $url: no sidebar entry for ${lang}${section}/index.html"; failures=$((failures + 1))
    fi
    expect "$url" "language link" "title=\"(English|Русский)\""
  done
done

if ((failures > 0)); then echo "$failures navigation check(s) failed"; exit 1; fi
echo "Navigation: all pages render the navbar, their sidebar and the language link"
