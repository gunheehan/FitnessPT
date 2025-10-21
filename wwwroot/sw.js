const CACHE_NAME = 'fitnesspt-v1.0.0';
const urlsToCache = [
    '/',
    '/css/site.css',
    '/js/site.js',
    '/icons/icon-192x192.png',
    '/icons/icon-512x512.png',
    // 필요한 정적 파일들 추가
];

// 서비스 워커 설치
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => {
                console.log('캐시 열림');
                return cache.addAll(urlsToCache);
            })
    );
});

// 캐시된 리소스 제공
self.addEventListener('fetch', event => {
    // Blazor SignalR 연결은 캐시하지 않음
    if (event.request.url.includes('/_blazor') ||
        event.request.url.includes('/negotiate')) {
        return;
    }

    event.respondWith(
        caches.match(event.request)
            .then(response => {
                // 캐시에 있으면 캐시에서 반환
                if (response) {
                    return response;
                }

                // 네트워크 요청
                return fetch(event.request)
                    .then(response => {
                        // 유효한 응답인지 확인
                        if (!response || response.status !== 200 || response.type !== 'basic') {
                            return response;
                        }

                        // 응답 복사 후 캐시에 저장
                        const responseToCache = response.clone();
                        caches.open(CACHE_NAME)
                            .then(cache => {
                                cache.put(event.request, responseToCache);
                            });

                        return response;
                    })
                    .catch(() => {
                        // 오프라인일 때 기본 페이지 반환
                        return caches.match('/');
                    });
            })
    );
});

// 캐시 업데이트
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(cacheNames => {
            return Promise.all(
                cacheNames.map(cacheName => {
                    if (cacheName !== CACHE_NAME) {
                        console.log('오래된 캐시 삭제:', cacheName);
                        return caches.delete(cacheName);
                    }
                })
            );
        })
    );
});