/**
 * chat.js — Blazor ↔ WebSocket 채팅 브릿지
 */
window.ChatWs = (() => {
    let ws = null;
    let dotnetRef = null;
    let wsUrl = null;
    let reconnectTimer = null;
    let reconnectDelay = 1000;     // 초기 재연결 대기 1초
    const maxReconnectDelay = 30000; // 최대 30초
    let manualClose = false;        // 사용자가 명시적으로 닫았는지 여부

    function connect(dotnetObjRef, url) {
        // 이전 연결이 살아있으면 먼저 정리
        if (ws && ws.readyState !== WebSocket.CLOSED) {
            manualClose = true;
            ws.close();
        }

        dotnetRef = dotnetObjRef;
        wsUrl = url;
        manualClose = false;
        _open();
    }

    function _open() {
        ws = new WebSocket(wsUrl);

        ws.onopen = () => {
            reconnectDelay = 1000; // 성공 시 재연결 딜레이 초기화
            console.log('[Chat] Connected');
        };

        ws.onmessage = (event) => {
            if (dotnetRef) {
                dotnetRef.invokeMethodAsync('OnMessageReceived', event.data);
            }
        };

        ws.onerror = (err) => {
            console.error('[Chat] Error', err);
        };

        ws.onclose = (event) => {
            console.log('[Chat] Disconnected', event.code, event.reason);

            if (dotnetRef) {
                dotnetRef.invokeMethodAsync('OnMessageReceived',
                    JSON.stringify({ type: 'disconnected' }));
            }

            // 사용자가 명시적으로 닫지 않은 경우 자동 재연결 (지수 백오프)
            if (!manualClose) {
                reconnectTimer = setTimeout(() => {
                    console.log(`[Chat] Reconnecting in ${reconnectDelay}ms...`);
                    _open();
                    reconnectDelay = Math.min(reconnectDelay * 2, maxReconnectDelay);
                }, reconnectDelay);
            }
        };
    }

    function send(message) {
        if (ws && ws.readyState === WebSocket.OPEN) {
            ws.send(message);
        }
    }

    function disconnect() {
        manualClose = true;
        clearTimeout(reconnectTimer);

        if (ws) {
            ws.close();
            ws = null;
        }
        dotnetRef = null;
    }

    return { connect, send, disconnect };
})();
