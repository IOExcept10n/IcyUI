#!/usr/bin/env bash
# Builds the documentation site into docfx/_site: API metadata, the English site, then the Russian site under /ru/.
# Every DocFX warning (broken link, missing xref, missing snippet) fails the build. Used locally and by docs.yml.
# Usage: bash .github/scripts/build-docs.sh   (from the repository root)
set -euo pipefail

dotnet tool restore
rm -rf docfx/_site docfx/obj

# DocFX silently includes the whole file when a [!code-...[](file#region)] names a region that doesn't exist, so check
# every include first: the file must exist and, when a region is named, contain `#region <name>`.
bad_includes=0
while IFS=: read -r page _ include; do
  target="${include#*](}"
  target="${target%)]}"
  file="${target%%#*}"
  region=""
  [[ "$target" == *"#"* ]] && region="${target#*#}"
  path="$(dirname "$page")/$file"
  if [[ ! -f "$path" ]]; then
    echo "::error file=$page::Code include points at a missing file: $file"
    bad_includes=$((bad_includes + 1))
  elif [[ -n "$region" && ! "$region" =~ ^L[0-9] ]] && ! grep -qE "#region[[:space:]]+${region}[[:space:]]*$" <(tr -d '\r' < "$path"); then
    echo "::error file=$page::Code include names a region that doesn't exist: $file#$region"
    bad_includes=$((bad_includes + 1))
  fi
done < <(grep -rnoE '\[!code-[a-z]+\[[^]]*\]\([^)]+\)\]' docfx/en docfx/ru --include='*.md')
if ((bad_includes > 0)); then
  exit 1
fi
# Metadata warnings fail the build too, except those inside source-generated files (*.g.cs): their XML comments come
# from generators such as CommunityToolkit.Mvvm's [ObservableObject] and can't be fixed here.
mkdir -p docfx/obj
dotnet docfx metadata docfx/docfx.en.json 2>&1 | tee docfx/obj/metadata.log
if grep -i 'warning' docfx/obj/metadata.log | grep -v '\.g\.cs' | grep -iv '^[[:space:]]*[0-9]* warning(s)' | grep -iv 'succeeded with warning'; then
  echo "::error::DocFX metadata reported the warnings above."
  exit 1
fi
dotnet docfx build docfx/docfx.en.json --warningsAsErrors

# The Russian build links API uids to the English pages. Re-base the English xrefmap's relative hrefs to the site
# root (absolute: DocFX rejects a relative baseUrl), so a Russian page links https://icyui.dev/api/..., not /ru/api/....
# Keeps the "### YamlMime:XRefMap" header line first.
awk 'NR == 1 && /^### YamlMime/ { print; print "baseUrl: https://icyui.dev/"; next } NR == 1 { print "baseUrl: https://icyui.dev/" } !/^baseUrl:/ { print }' docfx/_site/xrefmap.yml > docfx/obj/xrefmap.en.yml
dotnet docfx build docfx/docfx.ru.json --warningsAsErrors
