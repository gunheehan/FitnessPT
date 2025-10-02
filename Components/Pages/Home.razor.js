window.Home = {
    initialize: function() {
        console.log('🏠 Home page initialized');

        // 애니메이션 추가
        this.animateCards();
    },

    animateCards: function() {
        const cards = document.querySelectorAll('.action-card, .feature-card');
        cards.forEach((card, index) => {
            setTimeout(() => {
                card.style.opacity = '0';
                card.style.transform = 'translateY(20px)';

                setTimeout(() => {
                    card.style.transition = 'all 0.5s ease';
                    card.style.opacity = '1';
                    card.style.transform = 'translateY(0)';
                }, 50);
            }, index * 100);
        });
    },

    // 통계 카운터 애니메이션
    animateCounter: function(elementId, targetValue, duration) {
        const element = document.getElementById(elementId);
        if (!element) return;

        let startValue = 0;
        const increment = targetValue / (duration / 16); // 60fps

        const counter = setInterval(() => {
            startValue += increment;
            if (startValue >= targetValue) {
                element.textContent = targetValue;
                clearInterval(counter);
            } else {
                element.textContent = Math.floor(startValue);
            }
        }, 16);
    }
};