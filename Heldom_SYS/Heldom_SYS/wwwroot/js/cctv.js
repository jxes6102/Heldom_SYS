// cctv.js
let cctvTotalPage = Math.ceil(cctvData.length / 8);
let cctvCurrentPage = 0;

// 確保 cctvData 已加載
function initializeCctv() {
    if (!cctvData || cctvData.length === 0) {
        console.error("cctvData not loaded. Retrying in 1 second...");
        setTimeout(initializeCctv, 1000);
        return;
    }

    cctvTotalPage = Math.ceil(cctvData.length / 8); // 確保重新計算

    // 頁面加載時初始化
    $(function () {
        // 從 URL 獲取初始索引
        const urlParams = new URLSearchParams(window.location.search);
        const initialIndex = parseInt(urlParams.get("index")) || 0;
        cctvCurrentPage = Math.floor(initialIndex / 8);

        // 設置大畫面初始影像
        document.getElementById("largeImg").src = cctvData[initialIndex].src;

        // 小cctv生成
        for (let i = 0; i < (cctvData.length <= 8 ? cctvData.length : 8); i++) {
            $(".cctv-middle-right").append(`
                <div class="cctv-small">
                    <p class="db-title-s bold txt-center cctv-txt">${cctvData[i].name}</p>
                    <img class="video-s border-radius" id="smallImg${i}" style="-webkit-user-select: none;" src="${cctvData[i].src}">
                </div>
            `);

            // 點擊影像選擇
            document.getElementById(`smallImg${i}`).addEventListener("click", function () {
                document.getElementById("largeImg").src = this.src;
            });
        }

        $(".cctv-end").append(`
            <h4><i class="fas fa-chevron-left txt-center" id="cctvLast"></i></h4>
        `);
        for (let i = 0; i < cctvTotalPage; i++) {
            $(".cctv-end").append(`
                <div class="page_circle obj-center" id="page_circle${i + 1}">
                    <p class="bold">${i + 1}</p>
                </div>
            `);
        }
        $(".cctv-end").append(`
            <h4><i class="fas fa-chevron-right txt-center" id="cctvNext"></i></h4>
        `);

        // 頁次點擊功能
        document.getElementById("cctvLast").addEventListener("click", function () {
            if (cctvCurrentPage > 0) {
                cctvCurrentPage = cctvCurrentPage - 1;

                loadCCTV(cctvCurrentPage);
            }
        });

        document.getElementById("cctvNext").addEventListener("click", function () {
            //console.log(`cctvNext clicked, currentPage: ${cctvCurrentPage}`);
            if (cctvCurrentPage < cctvTotalPage - 1) {
                cctvCurrentPage = cctvCurrentPage + 1;

                loadCCTV(cctvCurrentPage);
            }
        });

        $(".page_circle").each(function (index) {
            $(this).on('click', function () {

                cctvCurrentPage = index;
                loadCCTV(cctvCurrentPage);
            });
        });

        // 初始化頁面圓圈為選中狀態並加載初始頁面
        document.getElementById(`page_circle${cctvCurrentPage + 1}`).style.backgroundColor = '#D9D9D9';
        loadCCTV(cctvCurrentPage);
    });
}

// 啟動初始化
initializeCctv();

function loadCCTV(_cctvCurrentPage) {
    for (let i = 0; i < 8; i++) {
        let dataIndex = _cctvCurrentPage * 8 + i;
        if (dataIndex < cctvData.length) {
            const smallImg = document.getElementById(`smallImg${i}`);
            const txt = smallImg.parentElement.querySelector('.cctv-txt');
            if (smallImg && txt) {
                smallImg.src = cctvData[dataIndex].src || "";
                txt.textContent = cctvData[dataIndex].name || "無名稱";
            }
        }
    }

    for (let i = 1; i <= cctvTotalPage; i++) {
        document.getElementById(`page_circle${i}`).style.backgroundColor = 'transparent';
    }
    document.getElementById(`page_circle${cctvCurrentPage + 1}`).style.backgroundColor = '#D9D9D9';
}