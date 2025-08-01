"use strict";
/**
 * Copyright(c) Live2D Inc. All rights reserved.
 *
 * Use of this source code is governed by the Live2D Open Software license
 * that can be found at https://www.live2d.com/eula/live2d-open-software-license-agreement_en.html.
 */
Object.defineProperty(exports, "__esModule", { value: true });
exports.LAppGlManager = void 0;
/**
 * Cubism SDKのサンプルで使用するWebGLを管理するクラス
 */
class LAppGlManager {
    constructor() {
        this._gl = null;
        this._gl = null;
    }
    initialize(canvas) {
        // glコンテキストを初期化
        this._gl = canvas.getContext('webgl2');
        if (!this._gl) {
            // gl初期化失敗
            alert('Cannot initialize WebGL. This browser does not support.');
            this._gl = null;
            // document.body.innerHTML =
            //   'This browser does not support the <code>&lt;canvas&gt;</code> element.';
            return false;
        }
        return true;
    }
    /**
     * 解放する。
     */
    release() { }
    getGl() {
        return this._gl;
    }
}
exports.LAppGlManager = LAppGlManager;
