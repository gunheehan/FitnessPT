
window.GoogleAuth = {
    // Google 버튼 렌더링
    renderButton: function(dotnetHelper, clientId, containerId) {
        console.log('Rendering Google button...');

        const checkGoogleLoaded = setInterval(() => {
            if (typeof google !== 'undefined' && google.accounts) {
                clearInterval(checkGoogleLoaded);

                console.log('Google API loaded');

                // Google 초기화
                google.accounts.id.initialize({
                    client_id: clientId,
                    callback: async (response) => {
                        console.log('Google login callback triggered');
                        await dotnetHelper.invokeMethodAsync('HandleGoogleLogin', response.credential);
                    }
                });

                // 버튼 렌더링
                const container = document.getElementById(containerId);
                if (container) {
                    google.accounts.id.renderButton(
                        container,
                        {
                            theme: 'outline',
                            size: 'large',
                            text: 'signin_with',
                            shape: 'rectangular',
                            logo_alignment: 'left',
                            width: 150
                        }
                    );
                    console.log('Google button rendered');
                } else {
                    console.error('Container not found:', containerId);
                }
            }
        }, 100);
    },

    // Google 로그아웃
    logout: function() {
        if (typeof google !== 'undefined' && google.accounts) {
            google.accounts.id.disableAutoSelect();
            console.log('Google logout');
        }
    }
};