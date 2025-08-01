"use strict";
/**
 * Copyright(c) Live2D Inc. All rights reserved.
 *
 * Use of this source code is governed by the Live2D Open Software license
 * that can be found at https://www.live2d.com/eula/live2d-open-software-license-agreement_en.html.
 */
Object.defineProperty(exports, "__esModule", { value: true });
exports.LAppPal = void 0;
/**
 * プラットフォーム依存機能を抽象化する Cubism Platform Abstraction Layer.
 *
 * ファイル読み込みや時刻取得等のプラットフォームに依存する関数をまとめる。
 */
class LAppPal {
    /**
     * ファイルをバイトデータとして読みこむ
     *
     * @param filePath 読み込み対象ファイルのパス
     * @return
     * {
     *      buffer,   読み込んだバイトデータ
     *      size        ファイルサイズ
     * }
     */
    static loadFileAsBytes(filePath, callback) {
        fetch(filePath)
            .then(response => response.arrayBuffer())
            .then(arrayBuffer => callback(arrayBuffer, arrayBuffer.byteLength));
    }
    /**
     * デルタ時間（前回フレームとの差分）を取得する
     * @return デルタ時間[ms]
     */
    static getDeltaTime() {
        return this.deltaTime;
    }
    static updateTime() {
        this.currentFrame = Date.now();
        this.deltaTime = (this.currentFrame - this.lastFrame) / 1000;
        this.lastFrame = this.currentFrame;
    }
    /**
     * メッセージを出力する
     * @param message 文字列
     */
    static printMessage(message) {
        console.log(message);
    }
}
exports.LAppPal = LAppPal;
LAppPal.lastUpdate = Date.now();
LAppPal.currentFrame = 0.0;
LAppPal.lastFrame = 0.0;
LAppPal.deltaTime = 0.0;
