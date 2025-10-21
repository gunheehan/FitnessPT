const CACHE_NAME = 'fitnesspt-ssr-v1.0.0';

// SSR 앱용 캐시 전략
const STATIC_CACHE = [
    '/',
    '/about',
    '/exercises',
    '/css/site.css',
    '/js/site.js',
    '/icons/icon-192x192.png',
];

// 캐시 우선 전략 (정적 콘텐츠)
const CACHE_FIRST = [
    /\.css$/,
    /\.js$/,
    /\.png$/,
    /\.jpg$/,
    /\.gif$/,
    /\.svg$/,
];

// 네트워크 우선 전략 (동적 콘텐츠)
const NETWORK_FIRST = [
    /\/api\//,
    /\/_blazor\//,
    /\/chat/,
];

self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => cache.addAll(STATIC_CACHE))
    );
});

self.addEventListener('fetch', event => {
    const url = new URL(event.request.url);

    // Blazor SignalR 연결은 캐시하지 않음
    if (url.pathname.includes('/_blazor') ||
        url.pathname.includes('/negotiate')) {
        return;
    }

    // 정적 리소스: 캐시 우선
    if (CACHE_FIRST.some(pattern => pattern.test(url.pathname))) {
        event.respondWith(cacheFirst(event.request));
        return;
    }

    // 동적 콘텐츠: 네트워크 우선
    if (NETWORK_FIRST.some(pattern => pattern.test(url.pathname))) {
        event.respondWith(networkFirst(event.request));
        return;
    }

    // 기본: 네트워크 우선, 오프라인 시 캐시
    event.respondWith(networkFirst(event.request));
});

async function cacheFirst(request) {
    const cached = await caches.match(request);
    if (cached) return cached;

    try {
        const response = await fetch(request);
        const cache = await caches.open(CACHE_NAME);
        cache.put(request, response.clone());
        return response;
    } catch {
        return new Response('오프라인 상태입니다', { status: 503 });
    }
}

async function networkFirst(request) {
    try {
        const response = await fetch(request);
        const cache = await caches.open(CACHE_NAME);
        cache.put(request, response.clone());
        return response;
    } catch {
        const cached = await caches.match(request);
        if (cached) return cached;

        // 오프라인 페이지 반환
        return caches.match('/offline.html') ||
            new Response('오프라인 상태입니다', { status: 503 });
    }
}