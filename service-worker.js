const CACHE_NAME = 'motoRiders-cache-v1';
const URLsToCache = [
    '/',                        // Página principal (index.html)
    '/Content/Styles.css',       // Hoja de estilo
    '/Content/Script.js',        // Script de JavaScript
    '/Content/Imagenes/icon-192x192.png',  // Icono 192x192
    '/Content/Imagenes/icon-512x512.png'   // Icono 512x512
];

self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then((cache) => {
                return cache.addAll(URLsToCache);
            })
    );
});

self.addEventListener('fetch', (event) => {
    event.respondWith(
        caches.match(event.request)
            .then((response) => {
                return response || fetch(event.request);
            })
    );
});

self.addEventListener('sync', (event) => {
    if (event.tag === 'sync-data') {
        event.waitUntil(
            fetch('/api/sync-data')
                .then((response) => response.json())
                .then((data) => {
                    // Procesar los datos sincronizados
                })
        );
    }
});
