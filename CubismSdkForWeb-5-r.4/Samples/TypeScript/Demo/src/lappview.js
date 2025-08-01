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
exports.LAppView = void 0;
const cubismmatrix44_1 = require("@framework/math/cubismmatrix44");
const cubismviewmatrix_1 = require("@framework/math/cubismviewmatrix");
const LAppDefine = __importStar(require("./lappdefine"));
const lapppal_1 = require("./lapppal");
const lappsprite_1 = require("./lappsprite");
const touchmanager_1 = require("./touchmanager");
/**
 * 描画クラス。
 */
class LAppView {
    /**
     * コンストラクタ
     */
    constructor() {
        this._programId = null;
        this._back = null;
        this._gear = null;
        // タッチ関係のイベント管理
        this._touchManager = new touchmanager_1.TouchManager();
        // デバイス座標からスクリーン座標に変換するための
        this._deviceToScreen = new cubismmatrix44_1.CubismMatrix44();
        // 画面の表示の拡大縮小や移動の変換を行う行列
        this._viewMatrix = new cubismviewmatrix_1.CubismViewMatrix();
    }
    /**
     * 初期化する。
     */
    initialize(subdelegate) {
        this._subdelegate = subdelegate;
        const { width, height } = subdelegate.getCanvas();
        const ratio = width / height;
        const left = -ratio;
        const right = ratio;
        const bottom = LAppDefine.ViewLogicalLeft;
        const top = LAppDefine.ViewLogicalRight;
        this._viewMatrix.setScreenRect(left, right, bottom, top); // デバイスに対応する画面の範囲。 Xの左端、Xの右端、Yの下端、Yの上端
        this._viewMatrix.scale(LAppDefine.ViewScale, LAppDefine.ViewScale);
        this._deviceToScreen.loadIdentity();
        if (width > height) {
            const screenW = Math.abs(right - left);
            this._deviceToScreen.scaleRelative(screenW / width, -screenW / width);
        }
        else {
            const screenH = Math.abs(top - bottom);
            this._deviceToScreen.scaleRelative(screenH / height, -screenH / height);
        }
        this._deviceToScreen.translateRelative(-width * 0.5, -height * 0.5);
        // 表示範囲の設定
        this._viewMatrix.setMaxScale(LAppDefine.ViewMaxScale); // 限界拡張率
        this._viewMatrix.setMinScale(LAppDefine.ViewMinScale); // 限界縮小率
        // 表示できる最大範囲
        this._viewMatrix.setMaxScreenRect(LAppDefine.ViewLogicalMaxLeft, LAppDefine.ViewLogicalMaxRight, LAppDefine.ViewLogicalMaxBottom, LAppDefine.ViewLogicalMaxTop);
    }
    /**
     * 解放する
     */
    release() {
        this._viewMatrix = null;
        this._touchManager = null;
        this._deviceToScreen = null;
        this._gear.release();
        this._gear = null;
        this._back.release();
        this._back = null;
        this._subdelegate.getGlManager().getGl().deleteProgram(this._programId);
        this._programId = null;
    }
    /**
     * 描画する。
     */
    render() {
        this._subdelegate.getGlManager().getGl().useProgram(this._programId);
        if (this._back) {
            this._back.render(this._programId);
        }
        if (this._gear) {
            this._gear.render(this._programId);
        }
        this._subdelegate.getGlManager().getGl().flush();
        const lapplive2dmanager = this._subdelegate.getLive2DManager();
        if (lapplive2dmanager != null) {
            lapplive2dmanager.setViewMatrix(this._viewMatrix);
            lapplive2dmanager.onUpdate();
        }
    }
    /**
     * 画像の初期化を行う。
     */
    initializeSprite() {
        const width = this._subdelegate.getCanvas().width;
        const height = this._subdelegate.getCanvas().height;
        const textureManager = this._subdelegate.getTextureManager();
        const resourcesPath = LAppDefine.ResourcesPath;
        let imageName = '';
        // 背景画像初期化
        imageName = LAppDefine.BackImageName;
        // 非同期なのでコールバック関数を作成
        const initBackGroundTexture = (textureInfo) => {
            const x = width * 0.5;
            const y = height * 0.5;
            const fwidth = textureInfo.width * 2.0;
            const fheight = height * 0.95;
            this._back = new lappsprite_1.LAppSprite(x, y, fwidth, fheight, textureInfo.id);
            this._back.setSubdelegate(this._subdelegate);
        };
        textureManager.createTextureFromPngFile(resourcesPath + imageName, false, initBackGroundTexture);
        // 歯車画像初期化
        imageName = LAppDefine.GearImageName;
        const initGearTexture = (textureInfo) => {
            const x = width - textureInfo.width * 0.5;
            const y = height - textureInfo.height * 0.5;
            const fwidth = textureInfo.width;
            const fheight = textureInfo.height;
            this._gear = new lappsprite_1.LAppSprite(x, y, fwidth, fheight, textureInfo.id);
            this._gear.setSubdelegate(this._subdelegate);
        };
        textureManager.createTextureFromPngFile(resourcesPath + imageName, false, initGearTexture);
        // シェーダーを作成
        if (this._programId == null) {
            this._programId = this._subdelegate.createShader();
        }
    }
    /**
     * タッチされた時に呼ばれる。
     *
     * @param pointX スクリーンX座標
     * @param pointY スクリーンY座標
     */
    onTouchesBegan(pointX, pointY) {
        this._touchManager.touchesBegan(pointX * window.devicePixelRatio, pointY * window.devicePixelRatio);
    }
    /**
     * タッチしているときにポインタが動いたら呼ばれる。
     *
     * @param pointX スクリーンX座標
     * @param pointY スクリーンY座標
     */
    onTouchesMoved(pointX, pointY) {
        const posX = pointX * window.devicePixelRatio;
        const posY = pointY * window.devicePixelRatio;
        const lapplive2dmanager = this._subdelegate.getLive2DManager();
        const viewX = this.transformViewX(this._touchManager.getX());
        const viewY = this.transformViewY(this._touchManager.getY());
        this._touchManager.touchesMoved(posX, posY);
        lapplive2dmanager.onDrag(viewX, viewY);
    }
    /**
     * タッチが終了したら呼ばれる。
     *
     * @param pointX スクリーンX座標
     * @param pointY スクリーンY座標
     */
    onTouchesEnded(pointX, pointY) {
        const posX = pointX * window.devicePixelRatio;
        const posY = pointY * window.devicePixelRatio;
        const lapplive2dmanager = this._subdelegate.getLive2DManager();
        // タッチ終了
        lapplive2dmanager.onDrag(0.0, 0.0);
        // シングルタップ
        const x = this.transformViewX(posX);
        const y = this.transformViewY(posY);
        if (LAppDefine.DebugTouchLogEnable) {
            lapppal_1.LAppPal.printMessage(`[APP]touchesEnded x: ${x} y: ${y}`);
        }
        lapplive2dmanager.onTap(x, y);
        // 歯車にタップしたか
        if (this._gear.isHit(posX, posY)) {
            lapplive2dmanager.nextScene();
        }
    }
    /**
     * X座標をView座標に変換する。
     *
     * @param deviceX デバイスX座標
     */
    transformViewX(deviceX) {
        const screenX = this._deviceToScreen.transformX(deviceX); // 論理座標変換した座標を取得。
        return this._viewMatrix.invertTransformX(screenX); // 拡大、縮小、移動後の値。
    }
    /**
     * Y座標をView座標に変換する。
     *
     * @param deviceY デバイスY座標
     */
    transformViewY(deviceY) {
        const screenY = this._deviceToScreen.transformY(deviceY); // 論理座標変換した座標を取得。
        return this._viewMatrix.invertTransformY(screenY);
    }
    /**
     * X座標をScreen座標に変換する。
     * @param deviceX デバイスX座標
     */
    transformScreenX(deviceX) {
        return this._deviceToScreen.transformX(deviceX);
    }
    /**
     * Y座標をScreen座標に変換する。
     *
     * @param deviceY デバイスY座標
     */
    transformScreenY(deviceY) {
        return this._deviceToScreen.transformY(deviceY);
    }
}
exports.LAppView = LAppView;
