// IcyUI docs: English at the site root, Russian under /ru/ with the same page paths.
// The EN/RU link is computed per page from docfx:rel (the relative path to this build's root), so it also works when
// the site is served under a path prefix (github.io/IcyUI/, docfx serve).

function otherLanguageUrl() {
  const relMeta = document.querySelector('meta[name="docfx:rel"]');
  if (!relMeta) return null;
  // docfx:rel is empty on pages at the build root; '' would resolve to the page itself, so use './'.
  const buildRoot = new URL(relMeta.content || './', window.location.href);
  const isRussian = document.documentElement.lang === 'ru';
  const siteRoot = isRussian ? new URL('../', buildRoot) : buildRoot;
  const pagePath = window.location.pathname.substring(buildRoot.pathname.length);

  if (isRussian) return new URL(pagePath, siteRoot).href;
  // The API reference exists only in English: send its pages to the Russian home.
  if (pagePath.startsWith('api/')) return new URL('ru/', siteRoot).href;
  return new URL('ru/' + pagePath, siteRoot).href;
}

const isRussian = document.documentElement.lang === 'ru';
const languageLink = otherLanguageUrl();

export default {
  lunrLanguages: ['en', 'ru'],
  iconLinks: [
    ...(languageLink ? [{ icon: 'translate', href: languageLink, title: isRussian ? 'English' : 'Русский' }] : []),
    { icon: 'github', href: 'https://github.com/IOExcept10n/IcyUI', title: 'GitHub' }
  ]
}
