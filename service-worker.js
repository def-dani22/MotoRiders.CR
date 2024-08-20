//asignar un nombre y versión al cache
const CACHE_NAME = 'xamplepwa',
    urlsToCache = [
        '/',
        '/Views/Cuenta/InicioSesion.cshtml',
        '/Content/Script.js',
        '/Content/Site.css',
        '/Content/Styles.css',
        '/Views/Auditoria/index.cshtml',
        '/Views/Carrito/index.cshtml',
        '/Views/CarritoCompra/index.cshtml',
        '/Views/Contacto/index.cshtml',
        '/Views/Cotizacion/Create.cshtml',
        '/Views/Cotizacion/index.cshtml',
        '/Views/CotizacionPersonalizada/index.cshtml',
        '/Views/Cuadraciclo/index.cshtml',
        '/Views/Cuenta/Confirmacion.cshtml',
        '/Views/Cuenta/MiPerfil.cshtml',
        '/Views/Cuenta/OlvideMiContrasena.cshtml',
        '/Views/Cuenta/RecuperarContrasena.cshtml',
        '/Views/Cuenta/Registro.cshtml',
        '/Views/Cuenta/ResponderPreguntaSeguridad.cshtml',
        '/Views/Cuenta/VerificarCorreo.cshtml',
        '/Views/Cuenta/VerificarToken.cshtml',
        '/Views/Cuenta/VerificarToken2FA.cshtml',
        '/Views/Home/index.cshtml',
        '/Views/HomeAnalistaDatos/index.cshtml',
        '/Views/HomeAnalistaDatos/MiPerfil.cshtml',
        '/Views/IndicadoresEconomicosBCCR/index.cshtml',
        '/Views/MotosElectricas/index.cshtml',
        '/Views/Producto/index.cshtml',
        '/Views/Prueba/Confirmacion.cshtml',
        '/Views/Prueba/Nueva.cshtml',
        '/Views/PruebaManejo/Confirmacion.cshtml',
        '/Views/PruebaManejo/index.cshtml',
        '/Views/Reporte/AccesoDenegado.cshtml',
        '/Views/Reporte/index.cshtml',
        '/Views/ReporteCompras/index.cshtml',
        '/Views/ReporteCotización/index.cshtml',
        '/Views/ReportePruebaManejo/index.cshtml',
        '/Views/Repuestos/index.cshtml',
        '/Views/Shared/_Layout.cshtml',
        '/Views/Shared/_Onboarding.cshtml',
        '/Views/Talleres/index.cshtml',
        '/Views/_ViewStart.cshtml',
        '/Content/Imagenes/Ob2.PNG',
        '/Content/Imagenes/Ob3.PNG',
        '/Content/Imagenes/Onboarding1.png',
        '/Content/Imagenes/c1.png',
        '/Content/Imagenes/ima-512x512.png',
        '/Content/Imagenes/ima-72x72.png',
    ]
//durante la fase de instalación, generalmente se almacena en caché los activos estáticos
self.addEventListener('install', e => {
    e.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => {
                return cache.addAll(urlsToCache)
                    .then(() => self.skipWaiting())
            })
            .catch(err => console.log('Falló registro de cache', err))
    )
})
//una vez que se instala el SW, se activa y busca los recursos para hacer que funcione sin conexión
self.addEventListener('activate', e => {
    const cacheWhitelist = [CACHE_NAME]
    e.waitUntil(
        caches.keys()
            .then(cacheNames => {
                return Promise.all(
                    cacheNames.map(cacheName => {
                        //Eliminamos lo que ya no se necesita en cache
                        if (cacheWhitelist.indexOf(cacheName) === -1) {
                            return caches.delete(cacheName)
                        }
                    })
                )
            })
            // Le indica al SW activar el cache actual
            .then(() => self.clients.claim())
    )
})
//cuando el navegador recupera una url
self.addEventListener('fetch', e => {
    //Responder ya sea con el objeto en caché o continuar y buscar la url real
    e.respondWith(
        caches.match(e.request)
            .then(res => {
                if (res) {
                    //recuperar del cache
                    return res
                }
                //recuperar de la petición a la url
                return fetch(e.request)
            })
    )
})