window.Login = {
    initialize: function(dotnetHelper, clientId){
        console.log('🚀 Login page initialized');

        // Google 버튼 렌더링
        if (window.GoogleAuth) {
            window.GoogleAuth.renderButton(
                dotnetHelper,
                clientId,
                'googleButtonContainer'
            );
        } else {
            console.error('GoogleAuth not loaded');
        }
    },

    showLoading: function(show){
        const loginCard = document.querySelector('.login-card');
        if (loginCard) {
            if (show) {
                loginCard.classList.add('loading');
            } else {loginCard.classList.remove('loading');
            }
        }
    },
    
    showError: function(message){
        console.error(message);
    }
}