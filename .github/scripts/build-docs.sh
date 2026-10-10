#!/usr/bin/env bash
# Builds the documentation site into docfx/_site: API metadata, the English site, then the Russian site under /ru/.
# Every DocFX warning (broken link, missing xref, missing snippet) fails the build. Used locally and by docs.yml.
# Usage: bash .github/scripts/build-docs.sh   (from the repository root)
set -euo pipefail

dotnet tool restore
rm -rf docfx/_site docfx/obj
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
# root, so a Russian page at /ru/... links /api/..., not /ru/api/....
# Keeps the "### YamlMime:XRefMap" header line first.
awk 'NR == 1 && /^### YamlMime/ { print; print "baseUrl: /"; next } NR == 1 { print "baseUrl: /" } !/^baseUrl:/ { print }' docfx/_site/xrefmap.yml > docfx/obj/xrefmap.en.yml
dotnet docfx build docfx/docfx.ru.json --warningsAsErrors
