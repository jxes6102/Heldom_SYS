// Dcamera.js (更新後，添加按鈕禁用)
let currentIndex = 0;
const itemsPerPage = 4;

// 初始化頁面
document.addEventListener("DOMContentLoaded", function () {
    if (!cctvData || cctvData.length === 0) {
        console.error("cctvData not loaded");
        return;
    }

    // 初始化大畫面
    document.getElementById("mainVideo").src = cctvData[0].src;

    // 動態生成 4 個 camera-item
    const cameraList = document.querySelector(".camera-list");
    for (let i = 0; i < itemsPerPage; i++) {
        const cameraItem = document.createElement("div");
        cameraItem.className = "camera-item";
        cameraItem.id = `camera${i}`;
        cameraItem.innerHTML = `
            <img class="camera-video" id="cameraVideo${i}" src="" alt="小畫面">
            <span class="camera-name"></span>
        `;
        cameraList.insertBefore(cameraItem, document.getElementById("nextButton"));
    }

    // 初始化小畫面
    updateCameraItems();

    // 設置第一個為選中狀態
    document.getElementById("cameraVideo0").classList.add("selected");

    // 綁定點擊事件 (小畫面更新大畫面)
    document.querySelectorAll(".camera-item").forEach(item => {
        item.addEventListener("click", function (event) {
            event.preventDefault();
            const index = parseInt(this.getAttribute("data-index")) + currentIndex;
            console.log(`Clicked camera item with index: ${index}`);
            document.getElementById("mainVideo").src = cctvData[index].src;

            document.querySelectorAll(".camera-video").forEach(video => {
                video.classList.remove("selected");
            });

            const clickedVideo = this.querySelector(".camera-video");
            clickedVideo.classList.add("selected");

            console.log(`Initial index from URL: ${index}`);
        });
    });

    // 綁定 mainVideo 點擊事件 (觸發跳轉)
    const mainVideo = document.getElementById("mainVideo");
    mainVideo.addEventListener("click", function () {
        const currentSrc = this.src;
        const index = cctvData.findIndex(item => item.src === currentSrc);
        console.log(`Clicked mainVideo with index: ${index}`);
        if (index !== -1) {
            const url = `/Dashboard/Cctv?index=${index}`;
            console.log(`Navigating to: ${url}`);
            window.location.href = url;
        } else {
            console.error("No matching index found for current video");
        }
    });

    // 左右按鈕事件
    const prevButton = document.getElementById("prevButton");
    const nextButton = document.getElementById("nextButton");

    function updateButtonState() {
        if (currentIndex <= 0) {
            prevButton.setAttribute("disabled", "true");
        } else {
            prevButton.removeAttribute("disabled");
        }
        if (currentIndex + itemsPerPage >= cctvData.length) {
            nextButton.setAttribute("disabled", "true");
        } else {
            nextButton.removeAttribute("disabled");
        }
        console.log(`Button state updated: prev=${prevButton.hasAttribute('disabled')}, next=${nextButton.hasAttribute('disabled')}, currentIndex=${currentIndex}, cctvData.length=${cctvData.length}`);
    }

    prevButton.addEventListener("click", function () {
        console.log(`prevButton clicked, currentIndex: ${currentIndex}`);
        if (currentIndex > 0) {
            currentIndex -= itemsPerPage;
            console.log(`New currentIndex: ${currentIndex}`);
            updateCameraItems();
            updateSelectedVideo();
            updateButtonState();
        }
    });

    nextButton.addEventListener("click", function () {
        console.log(`nextButton clicked, currentIndex: ${currentIndex}`);
        if (currentIndex + itemsPerPage < cctvData.length) {
            currentIndex += itemsPerPage;
            console.log(`New currentIndex: ${currentIndex}`);
            updateCameraItems();
            updateSelectedVideo();
            updateButtonState();
        }
    });

    // 初始更新
    updateButtonState();
    updateCameraItems();
    updateSelectedVideo();
});

// 更新小畫面內容
function updateCameraItems() {
    console.log(`Updating camera items with currentIndex: ${currentIndex}`);
    for (let i = 0; i < itemsPerPage; i++) {
        const dataIndex = currentIndex + i;
        const cameraItem = document.getElementById(`camera${i}`);
        const cameraVideo = document.getElementById(`cameraVideo${i}`);
        const cameraName = cameraItem.querySelector(".camera-name");

        if (dataIndex < cctvData.length && cameraItem && cameraVideo && cameraName) {
            console.log(`Updating camera${i}: src=${cctvData[dataIndex].src}, name=${cctvData[dataIndex].name}`);
            cameraVideo.src = cctvData[dataIndex].src || "";
            cameraName.textContent = cctvData[dataIndex].name || "無名稱";
            cameraItem.setAttribute("data-index", i);
        } else {
            console.log(`Clearing camera${i}: dataIndex=${dataIndex} out of range or element missing`);
            cameraVideo.src = "";
            cameraName.textContent = "";
            cameraItem.setAttribute("data-index", "");
        }
    }
}

// 更新選中視頻的狀態
function updateSelectedVideo() {
    const mainVideoSrc = document.getElementById("mainVideo").src;
    document.querySelectorAll(".camera-video").forEach(video => {
        if (video.src === mainVideoSrc) {
            video.classList.add("selected");
        } else {
            video.classList.remove("selected");
        }
    });
}