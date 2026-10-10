#!/usr/bin/env bash
# Reports Russian doc pages whose English source changed since they were translated. Never fails the build: an English
# fix shouldn't block a deploy, but the warnings list what to re-translate. A Russian page records its source in front
# matter as `source-hash: <first 12 hex of sha256 of the English file with CRs removed>` (compute it with
#   tr -d '\r' < docfx/en/<page>.md | sha256sum | cut -c1-12); a stub records `translation: stub`.
# Usage: check-translations.sh [docs-root]   (default: docfx; expects <root>/en and <root>/ru)
set -uo pipefail

root="${1:-docfx}"
fresh=0 stale=0 stubs=0 orphans=0

front_matter_value() { # file key -> value of `key:` inside the leading --- block, or empty
  awk -v key="$2" '
    NR == 1 { if ($0 != "---") exit; next }
    $0 == "---" { exit }
    { split($0, kv, ":"); if (kv[1] == key) { sub(/^[^:]*:[[:space:]]*/, ""); sub(/\r$/, ""); print; exit } }
  ' "$1"
}

while IFS= read -r -d '' ru; do
  rel="${ru#"$root/ru/"}"
  [[ "$rel" == "_glossary.md" ]] && continue
  en="$root/en/$rel"
  if [[ "$(front_matter_value "$ru" translation)" == "stub" ]]; then
    stubs=$((stubs + 1))
    continue
  fi
  if [[ ! -f "$en" ]]; then
    echo "::warning file=$ru::Russian page has no English source at $en (renamed or removed?)."
    orphans=$((orphans + 1))
    continue
  fi
  # Hash with CRs stripped, so the value is the same on CRLF (autocrlf) and LF checkouts; hashing stdin also avoids
  # sha256sum escaping its output with a leading "\" when the path contains a backslash.
  current=$(tr -d '\r' < "$en" | sha256sum | cut -c1-12)
  recorded=$(front_matter_value "$ru" source-hash)
  if [[ "$recorded" == "$current" ]]; then
    fresh=$((fresh + 1))
  else
    echo "::warning file=$ru::Translation is stale: $en changed (recorded ${recorded:-none}, current $current). Update the text, then set source-hash: $current"
    stale=$((stale + 1))
  fi
done < <(find "$root/ru" -name '*.md' -print0 | sort -z)

echo "Translations: $fresh fresh, $stale stale, $stubs stubs, $orphans orphans"
exit 0
