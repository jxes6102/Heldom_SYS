// wwwroot/js/waterRipple.js

export function initRippleAnimation(bellContainerId) {
    const canvas = document.getElementById('rippleCanvas');
    const ctx = canvas.getContext('2d');

    // 讓 Canvas 充滿 bell-container
    const container = document.getElementById(bellContainerId);
    canvas.width = container.offsetWidth;
    canvas.height = container.offsetWidth;

    const bellX = canvas.width / 2;
    const bellY = canvas.height / 2;
    //const maxRadius = Math.min(canvas.width, canvas.height) / 2;
    let ripples = [];
    let animationFrameId; // 儲存 requestAnimationFrame ID

    function createRipple() {
        ripples.push({
            x: bellX,
            y: bellY,
            radius: 0,
            opacity: 1
        });
    }

    function drawRipples() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        ripples.forEach((ripple, index) => {
            ctx.beginPath();
            ctx.arc(ripple.x, ripple.y, ripple.radius, 0, Math.PI * 2);
            ctx.strokeStyle = `rgba(255, 165, 0, ${ripple.opacity})`; // 橘色水波
            ctx.lineWidth = 1;
            ctx.stroke();

            ripple.radius += 1;  // 增加水波半徑
            ripple.opacity -= 0.02;  // 逐漸減少透明度

            // 當透明度小於等於 0 時，從水波陣列中移除
            if (ripple.opacity <= 0) {
                ripples.splice(index, 1);
            }
        });

        // 不斷更新動畫，並保存下一幀的 ID
        animationFrameId = requestAnimationFrame(drawRipples);
    }

    // 每 1.5 秒自動產生一個水波
    setInterval(createRipple, 1500);

    function startAnimation() {
        drawRipples();
    }

    function stopAnimation() {
        cancelAnimationFrame(animationFrameId); // 停止動畫
    }

    // 當頁面可見時開始動畫，不可見時停止動畫
    document.addEventListener("visibilitychange", function () {
        if (document.hidden) {
            stopAnimation(); // 停止動畫
        } else {
            startAnimation(); // 開始動畫
        }
    });

    startAnimation();  // 頁面初始載入時開始動畫
}
