"use strict";
/**
 * Copyright(c) Live2D Inc. All rights reserved.
 *
 * Use of this source code is governed by the Live2D Open Software license
 * that can be found at https://www.live2d.com/eula/live2d-open-software-license-agreement_en.html.
 */
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
exports.LAppDelegate = exports.s_instance = void 0;
const csmvector_1 = require("@framework/type/csmvector");
const live2dcubismframework_1 = require("@framework/live2dcubismframework");
const LAppDefine = __importStar(require("./lappdefine"));
const lapppal_1 = require("./lapppal");
const lappsubdelegate_1 = require("./lappsubdelegate");
const cubismdebug_1 = require("@framework/utils/cubismdebug");
exports.s_instance = null;
/**
 * アプリケーションクラス。
 * Cubism SDKの管理を行う。
 */
class LAppDelegate {
    /**
     * クラスのインスタンス（シングルトン）を返す。
     * インスタンスが生成されていない場合は内部でインスタンスを生成する。
     *
     * @return クラスのインスタンス
     */
    static getInstance() {
        if (exports.s_instance == null) {
            exports.s_instance = new LAppDelegate();
        }
        return exports.s_instance;
    }
    /**
     * クラスのインスタンス（シングルトン）を解放する。
     */
    static releaseInstance() {
        if (exports.s_instance != null) {
            exports.s_instance.release();
        }
        exports.s_instance = null;
    }
    /**
     * ポインタがアクティブになるときに呼ばれる。
     */
    onPointerBegan(e) {
        for (let ite = this._subdelegates.begin(); ite.notEqual(this._subdelegates.end()); ite.preIncrement()) {
            ite.ptr().onPointBegan(e.pageX, e.pageY);
        }
    }
    /**
     * ポインタが動いたら呼ばれる。
     */
    onPointerMoved(e) {
        for (let ite = this._subdelegates.begin(); ite.notEqual(this._subdelegates.end()); ite.preIncrement()) {
            ite.ptr().onPointMoved(e.pageX, e.pageY);
        }
    }
    /**
     * ポインタがアクティブでなくなったときに呼ばれる。
     */
    onPointerEnded(e) {
        for (let ite = this._subdelegates.begin(); ite.notEqual(this._subdelegates.end()); ite.preIncrement()) {
            ite.ptr().onPointEnded(e.pageX, e.pageY);
        }
    }
    /**
     * ポインタがキャンセルされると呼ばれる。
     */
    onPointerCancel(e) {
        for (let ite = this._subdelegates.begin(); ite.notEqual(this._subdelegates.end()); ite.preIncrement()) {
            ite.ptr().onTouchCancel(e.pageX, e.pageY);
        }
    }
    /**
     * Resize canvas and re-initialize view.
     */
    onResize() {
        for (let i = 0; i < this._subdelegates.getSize(); i++) {
            this._subdelegates.at(i).onResize();
        }
    }
    /**
     * 実行処理。
     */
    run() {
        // メインループ
        const loop = () => {
            // インスタンスの有無の確認
            if (exports.s_instance == null) {
                return;
            }
            // 時間更新
            lapppal_1.LAppPal.updateTime();
            for (let i = 0; i < this._subdelegates.getSize(); i++) {
                this._subdelegates.at(i).update();
            }
            // ループのために再帰呼び出し
            requestAnimationFrame(loop);
        };
        loop();
    }
    /**
     * 解放する。
     */
    release() {
        this.releaseEventListener();
        this.releaseSubdelegates();
        // Cubism SDKの解放
        live2dcubismframework_1.CubismFramework.dispose();
        this._cubismOption = null;
    }
    /**
     * イベントリスナーを解除する。
     */
    releaseEventListener() {
        document.removeEventListener('pointerup', this.pointBeganEventListener);
        this.pointBeganEventListener = null;
        document.removeEventListener('pointermove', this.pointMovedEventListener);
        this.pointMovedEventListener = null;
        document.removeEventListener('pointerdown', this.pointEndedEventListener);
        this.pointEndedEventListener = null;
        document.removeEventListener('pointerdown', this.pointCancelEventListener);
        this.pointCancelEventListener = null;
    }
    /**
     * Subdelegate を解放する
     */
    releaseSubdelegates() {
        for (let ite = this._subdelegates.begin(); ite.notEqual(this._subdelegates.end()); ite.preIncrement()) {
            ite.ptr().release();
        }
        this._subdelegates.clear();
        this._subdelegates = null;
    }
    /**
     * APPに必要な物を初期化する。
     */
    initialize() {
        // Cubism SDKの初期化
        this.initializeCubism();
        this.initializeSubdelegates();
        this.initializeEventListener();
        return true;
    }
    /**
     * イベントリスナーを設定する。
     */
    initializeEventListener() {
        this.pointBeganEventListener = this.onPointerBegan.bind(this);
        this.pointMovedEventListener = this.onPointerMoved.bind(this);
        this.pointEndedEventListener = this.onPointerEnded.bind(this);
        this.pointCancelEventListener = this.onPointerCancel.bind(this);
        // ポインタ関連コールバック関数登録
        document.addEventListener('pointerdown', this.pointBeganEventListener, {
            passive: true
        });
        document.addEventListener('pointermove', this.pointMovedEventListener, {
            passive: true
        });
        document.addEventListener('pointerup', this.pointEndedEventListener, {
            passive: true
        });
        document.addEventListener('pointercancel', this.pointCancelEventListener, {
            passive: true
        });
    }
    /**
     * Cubism SDKの初期化
     */
    initializeCubism() {
        lapppal_1.LAppPal.updateTime();
        // setup cubism
        this._cubismOption.logFunction = lapppal_1.LAppPal.printMessage;
        this._cubismOption.loggingLevel = LAppDefine.CubismLoggingLevel;
        live2dcubismframework_1.CubismFramework.startUp(this._cubismOption);
        // initialize cubism
        live2dcubismframework_1.CubismFramework.initialize();
    }
    /**
     * Canvasを生成配置、Subdelegateを初期化する
     */
    initializeSubdelegates() {
        let width = 100;
        let height = 100;
        if (LAppDefine.CanvasNum > 3) {
            const widthunit = Math.ceil(Math.sqrt(LAppDefine.CanvasNum));
            const heightUnit = Math.ceil(LAppDefine.CanvasNum / widthunit);
            width = 100.0 / widthunit;
            height = 100.0 / heightUnit;
        }
        else {
            width = 100.0 / LAppDefine.CanvasNum;
        }
        this._canvases.prepareCapacity(LAppDefine.CanvasNum);
        this._subdelegates.prepareCapacity(LAppDefine.CanvasNum);
        for (let i = 0; i < LAppDefine.CanvasNum; i++) {
            const canvas = document.createElement('canvas');
            this._canvases.pushBack(canvas);
            canvas.style.width = `${width}vw`;
            canvas.style.height = `${height}vh`;
            // キャンバスを DOM に追加
            document.body.appendChild(canvas);
        }
        for (let i = 0; i < this._canvases.getSize(); i++) {
            const subdelegate = new lappsubdelegate_1.LAppSubdelegate();
            subdelegate.initialize(this._canvases.at(i));
            this._subdelegates.pushBack(subdelegate);
        }
        for (let i = 0; i < LAppDefine.CanvasNum; i++) {
            if (this._subdelegates.at(i).isContextLost()) {
                (0, cubismdebug_1.CubismLogError)(`The context for Canvas at index ${i} was lost, possibly because the acquisition limit for WebGLRenderingContext was reached.`);
            }
        }
    }
    /**
     * Privateなコンストラクタ
     */
    constructor() {
        this._cubismOption = new live2dcubismframework_1.Option();
        this._subdelegates = new csmvector_1.csmVector();
        this._canvases = new csmvector_1.csmVector();
    }
}
exports.LAppDelegate = LAppDelegate;
