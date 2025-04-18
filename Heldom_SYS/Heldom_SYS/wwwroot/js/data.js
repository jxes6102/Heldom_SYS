// data.js
let cctvData = [];

function generateCctvData() {
    const cctvSources = [
        { url: "https://tcnvr7.taichung.gov.tw/7ebde69e", road1: "公益", road2: "大墩" },
        { url: "https://tcnvr7.taichung.gov.tw/ab97ca85", road1: "敦化", road2: "凱旋" },
        { url: "https://tcnvr4.taichung.gov.tw/56c91b90", road1: "中科", road2: "河南" },
        { url: "https://tcnvr3.taichung.gov.tw/f13df0d7", road1: "公益", road2: "忠明南" },
        { url: "https://tcnvr3.taichung.gov.tw/f13df0d7", road1: "公益", road2: "忠明南" },
        { url: "https://tcnvr3.taichung.gov.tw/f13df0d7", road1: "公益", road2: "忠明南" }
    ];

    cctvData = [];
    for (let i = 0; i < 40; i++) {
        const pairIndex = i % cctvSources.length; // 使用陣列長度動態計算
        const source = cctvSources[pairIndex];
        const src = source.url;
        const name = `${source.road1}路/${source.road2}路`;
        cctvData.push({ name, src });
    }
}

// 執行生成並調試
generateCctvData();

