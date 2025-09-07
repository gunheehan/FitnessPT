// wwwroot/js/site.js

window.siteHelpers = {
    // 모달 관련 함수들
    showModal: (modalId) => {
        try {
            const modal = document.getElementById(modalId);
            if (modal) {
                // 기존 인스턴스가 있다면 제거
                const existingModal = bootstrap.Modal.getInstance(modal);
                if (existingModal) {
                    existingModal.dispose();
                }

                const bsModal = new bootstrap.Modal(modal);
                bsModal.show();
                return true;
            }
            return false;
        } catch (error) {
            console.error('Error showing modal:', error);
            return false;
        }
    },

    hideModal: (modalId) => {
        try {
            const modal = document.getElementById(modalId);
            if (modal) {
                const bsModal = bootstrap.Modal.getInstance(modal);
                if (bsModal) {
                    bsModal.hide();
                    return true;
                }
            }
            return false;
        } catch (error) {
            console.error('Error hiding modal:', error);
            return false;
        }
    },

    // 드롭다운 관련 함수들
    initializeDropdowns: () => {
        try {
            // 기존 드롭다운 인스턴스들 정리
            const existingDropdowns = document.querySelectorAll('[data-bs-toggle="dropdown"]');
            existingDropdowns.forEach(element => {
                const existingInstance = bootstrap.Dropdown.getInstance(element);
                if (existingInstance) {
                    existingInstance.dispose();
                }
            });

            // 새로운 드롭다운 초기화
            const dropdownElementList = document.querySelectorAll('[data-bs-toggle="dropdown"]');
            dropdownElementList.forEach(dropdownToggleEl => {
                if (dropdownToggleEl && !bootstrap.Dropdown.getInstance(dropdownToggleEl)) {
                    new bootstrap.Dropdown(dropdownToggleEl);
                }
            });

            console.log(`Initialized ${dropdownElementList.length} dropdowns`);
            return true;
        } catch (error) {
            console.error('Error initializing dropdowns:', error);
            return false;
        }
    },

    // 툴팁 초기화
    initializeTooltips: () => {
        try {
            // 기존 툴팁 인스턴스들 정리
            const existingTooltips = document.querySelectorAll('[data-bs-toggle="tooltip"]');
            existingTooltips.forEach(element => {
                const existingInstance = bootstrap.Tooltip.getInstance(element);
                if (existingInstance) {
                    existingInstance.dispose();
                }
            });

            // 새로운 툴팁 초기화
            const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
            tooltipTriggerList.forEach(tooltipTriggerEl => {
                if (tooltipTriggerEl && !bootstrap.Tooltip.getInstance(tooltipTriggerEl)) {
                    new bootstrap.Tooltip(tooltipTriggerEl);
                }
            });

            return true;
        } catch (error) {
            console.error('Error initializing tooltips:', error);
            return false;
        }
    },

    // 모든 부트스트랩 컴포넌트 초기화
    initializeAllComponents: () => {
        try {
            window.siteHelpers.initializeDropdowns();
            window.siteHelpers.initializeTooltips();
            return true;
        } catch (error) {
            console.error('Error initializing components:', error);
            return false;
        }
    },

    // DOM 정리 함수
    cleanup: () => {
        try {
            // 모든 부트스트랩 인스턴스 정리
            const modals = document.querySelectorAll('.modal');
            modals.forEach(modal => {
                const instance = bootstrap.Modal.getInstance(modal);
                if (instance) instance.dispose();
            });

            const dropdowns = document.querySelectorAll('[data-bs-toggle="dropdown"]');
            dropdowns.forEach(dropdown => {
                const instance = bootstrap.Dropdown.getInstance(dropdown);
                if (instance) instance.dispose();
            });

            const tooltips = document.querySelectorAll('[data-bs-toggle="tooltip"]');
            tooltips.forEach(tooltip => {
                const instance = bootstrap.Tooltip.getInstance(tooltip);
                if (instance) instance.dispose();
            });

            return true;
        } catch (error) {
            console.error('Error during cleanup:', error);
            return false;
        }
    }
};

// 하위 호환성을 위한 전역 함수들
window.showModal = window.siteHelpers.showModal;
window.hideModal = window.siteHelpers.hideModal;
window.initializeDropdowns = window.siteHelpers.initializeDropdowns;

// DOM 로드 후 초기화
document.addEventListener('DOMContentLoaded', function () {
    console.log('DOM loaded, initializing components...');
    window.siteHelpers.initializeAllComponents();
});

// Blazor 관련 이벤트 핸들러들
window.blazorReconnected = () => {
    console.log('Blazor reconnected, reinitializing components...');
    setTimeout(() => {
        window.siteHelpers.initializeAllComponents();
    }, 100);
};

// Blazor가 페이지를 업데이트할 때마다 호출
window.blazorUpdated = () => {
    console.log('Blazor updated, reinitializing components...');
    setTimeout(() => {
        window.siteHelpers.initializeAllComponents();
    }, 50);
};

// MutationObserver를 사용하여 DOM 변경 감지
let observer;
const initializeObserver = () => {
    if (observer) {
        observer.disconnect();
    }

    observer = new MutationObserver((mutations) => {
        let shouldReinitialize = false;

        mutations.forEach((mutation) => {
            // 새로운 노드가 추가되었는지 확인
            mutation.addedNodes.forEach((node) => {
                if (node.nodeType === Node.ELEMENT_NODE) {
                    const element = node;
                    // 드롭다운이나 모달이 포함된 요소가 추가되었는지 확인
                    if (element.querySelector &&
                        (element.querySelector('[data-bs-toggle="dropdown"]') ||
                            element.querySelector('[data-bs-toggle="tooltip"]') ||
                            element.classList.contains('dropdown') ||
                            element.classList.contains('modal'))) {
                        shouldReinitialize = true;
                    }
                }
            });
        });

        if (shouldReinitialize) {
            setTimeout(() => {
                window.siteHelpers.initializeAllComponents();
            }, 50);
        }
    });

    // body 요소의 변경사항 관찰
    observer.observe(document.body, {
        childList: true,
        subtree: true
    });
};

// 페이지 로드 완료 후 MutationObserver 시작
window.addEventListener('load', () => {
    initializeObserver();
});

// 페이지 언로드 시 정리
window.addEventListener('beforeunload', () => {
    window.siteHelpers.cleanup();
    if (observer) {
        observer.disconnect();
    }
});