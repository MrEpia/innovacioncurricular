/* ==========================================================================
   Service worker de la PWA (v4)

   Blazor Server pinta cada pantalla en el SERVIDOR a través de una conexión
   viva: sin red no hay datos, y eso no se puede «cachear». Lo que sí hace la
   PWA:
     · se instala como aplicación (manifest.webmanifest);
     · guarda los archivos estáticos (estilos, logos, íconos) para que el
       marco cargue al instante;
     · sin conexión, muestra offline.html en vez del error del navegador.

   NUNCA guarda respuestas de la API, de /_blazor, ni de /ingresar o /sesion:
   son datos de la sesión de una persona.
   ========================================================================== */
const VERSION = 'umv-v4-1';
const ESTATICOS = [
  '/offline.html',
  '/app.css',
  '/marca.css',
  '/favicon.png',
  '/marca/logo-horizontal.png',
  '/marca/logo-vertical.png',
  '/iconos/icono-192.png',
  '/iconos/icono-512.png'
];

self.addEventListener('install', (evento) => {
  evento.waitUntil(caches.open(VERSION).then((cache) => cache.addAll(ESTATICOS)));
  self.skipWaiting();
});

self.addEventListener('activate', (evento) => {
  evento.waitUntil(
    caches.keys().then((llaves) => Promise.all(llaves.filter((l) => l !== VERSION).map((l) => caches.delete(l))))
  );
  self.clients.claim();
});

self.addEventListener('fetch', (evento) => {
  const pedido = evento.request;
  const url = new URL(pedido.url);

  if (pedido.method !== 'GET' || url.origin !== self.location.origin) return;
  if (url.pathname.startsWith('/_blazor') || url.pathname.startsWith('/_framework') ||
      url.pathname.startsWith('/ingresar') || url.pathname.startsWith('/sesion')) return;

  // Páginas: siempre a la red; si no hay red, la página «sin conexión».
  if (pedido.mode === 'navigate') {
    evento.respondWith(fetch(pedido).catch(() => caches.match('/offline.html')));
    return;
  }

  // Estáticos de la marca: primero la caché, y si no está, la red.
  if (/\.(css|png|svg|ico|woff2?)$/.test(url.pathname)) {
    evento.respondWith(caches.match(pedido).then((guardado) => guardado || fetch(pedido)));
  }
});
