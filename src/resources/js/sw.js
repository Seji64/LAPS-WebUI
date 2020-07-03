const staticCacheName = 'LAPSWebUI';
const assets = [
    '/',
    '/laps',
    '/resources/js/helper.js',
    '/resources/images/*.png',
    '/resources/bootstrap/css/bootstrap.min.css',
    '/resources/bootstrap/js/popper.min.js',
    '/resources/bootstrap/js/bootstrap.min.js',
    '/resources/clipboardjs/js/clipboard.min.js',
    '/resources/fontawesome/css/all.min.css',
    '/resources/fontawesome/webfonts/*.(svg|ttf|woff|woff2)',
];// install event
self.addEventListener('install', evt => {
    evt.waitUntil(
        caches.open(staticCacheName).then((cache) => {
            console.log('caching shell assets');
            cache.addAll(assets);
        })
    );
});// activate event
self.addEventListener('activate', evt => {
    evt.waitUntil(
        caches.keys().then(keys => {
            return Promise.all(keys
                .filter(key => key !== staticCacheName)
                .map(key => caches.delete(key))
            );
        })
    );
});// fetch event
self.addEventListener('fetch', evt => {
    evt.respondWith(
        caches.match(evt.request).then(cacheRes => {
            return cacheRes || fetch(evt.request);
        })
    );
});