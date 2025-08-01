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
exports.LAppLive2DManager = void 0;
const cubismmatrix44_1 = require("@framework/math/cubismmatrix44");
const csmvector_1 = require("@framework/type/csmvector");
const LAppDefine = __importStar(require("./lappdefine"));
const lappmodel_1 = require("./lappmodel");
const lapppal_1 = require("./lapppal");
/**
 * サンプルアプリケーションにおいてCubismModelを管理するクラス
 * モデル生成と破棄、タップイベントの処理、モデル切り替えを行う。
 */
class LAppLive2DManager {
    /**
     * 現在のシーンで保持しているすべてのモデルを解放する
     */
    releaseAllModel() {
        this._models.clear();
    }
    /**
     * 画面をドラッグした時の処理
     *
     * @param x 画面のX座標
     * @param y 画面のY座標
     */
    onDrag(x, y) {
        const model = this._models.at(0);
        if (model) {
            model.setDragging(x, y);
        }
    }
    /**
     * 画面をタップした時の処理
     *
     * @param x 画面のX座標
     * @param y 画面のY座標
     */
    onTap(x, y) {
        if (LAppDefine.DebugLogEnable) {
            lapppal_1.LAppPal.printMessage(`[APP]tap point: {x: ${x.toFixed(2)} y: ${y.toFixed(2)}}`);
        }
        const model = this._models.at(0);
        if (model.hitTest(LAppDefine.HitAreaNameHead, x, y)) {
            if (LAppDefine.DebugLogEnable) {
                lapppal_1.LAppPal.printMessage(`[APP]hit area: [${LAppDefine.HitAreaNameHead}]`);
            }
            model.setRandomExpression();
        }
        else if (model.hitTest(LAppDefine.HitAreaNameBody, x, y)) {
            if (LAppDefine.DebugLogEnable) {
                lapppal_1.LAppPal.printMessage(`[APP]hit area: [${LAppDefine.HitAreaNameBody}]`);
            }
            model.startRandomMotion(LAppDefine.MotionGroupTapBody, LAppDefine.PriorityNormal, this.finishedMotion, this.beganMotion);
        }
    }
    /**
     * 画面を更新するときの処理
     * モデルの更新処理及び描画処理を行う
     */
    onUpdate() {
        const { width, height } = this._subdelegate.getCanvas();
        const projection = new cubismmatrix44_1.CubismMatrix44();
        const model = this._models.at(0);
        if (model.getModel()) {
            if (model.getModel().getCanvasWidth() > 1.0 && width < height) {
                // 横に長いモデルを縦長ウィンドウに表示する際モデルの横サイズでscaleを算出する
                model.getModelMatrix().setWidth(2.0);
                projection.scale(1.0, width / height);
            }
            else {
                projection.scale(height / width, 1.0);
            }
            // 必要があればここで乗算
            if (this._viewMatrix != null) {
                projection.multiplyByMatrix(this._viewMatrix);
            }
        }
        model.update();
        model.draw(projection); // 参照渡しなのでprojectionは変質する。
    }
    /**
     * 次のシーンに切りかえる
     * サンプルアプリケーションではモデルセットの切り替えを行う。
     */
    nextScene() {
        const no = (this._sceneIndex + 1) % LAppDefine.ModelDirSize;
        this.changeScene(no);
    }
    /**
     * シーンを切り替える
     * サンプルアプリケーションではモデルセットの切り替えを行う。
     * @param index
     */
    changeScene(index) {
        this._sceneIndex = index;
        if (LAppDefine.DebugLogEnable) {
            lapppal_1.LAppPal.printMessage(`[APP]model index: ${this._sceneIndex}`);
        }
        // ModelDir[]に保持したディレクトリ名から
        // model3.jsonのパスを決定する。
        // ディレクトリ名とmodel3.jsonの名前を一致させておくこと。
        const model = LAppDefine.ModelDir[index];
        const modelPath = LAppDefine.ResourcesPath + model + '/';
        let modelJsonName = LAppDefine.ModelDir[index];
        modelJsonName += '.model3.json';
        this.releaseAllModel();
        const instance = new lappmodel_1.LAppModel();
        instance.setSubdelegate(this._subdelegate);
        instance.loadAssets(modelPath, modelJsonName);
        this._models.pushBack(instance);
    }
    setViewMatrix(m) {
        for (let i = 0; i < 16; i++) {
            this._viewMatrix.getArray()[i] = m.getArray()[i];
        }
    }
    /**
     * モデルの追加
     */
    addModel(sceneIndex = 0) {
        this._sceneIndex = sceneIndex;
        this.changeScene(this._sceneIndex);
    }
    /**
     * コンストラクタ
     */
    constructor() {
        // モーション再生開始のコールバック関数
        this.beganMotion = (self) => {
            lapppal_1.LAppPal.printMessage('Motion Began:');
            console.log(self);
        };
        // モーション再生終了のコールバック関数
        this.finishedMotion = (self) => {
            lapppal_1.LAppPal.printMessage('Motion Finished:');
            console.log(self);
        };
        this._subdelegate = null;
        this._viewMatrix = new cubismmatrix44_1.CubismMatrix44();
        this._models = new csmvector_1.csmVector();
        this._sceneIndex = 0;
    }
    /**
     * 解放する。
     */
    release() { }
    /**
     * 初期化する。
     * @param subdelegate
     */
    initialize(subdelegate) {
        this._subdelegate = subdelegate;
        this.changeScene(this._sceneIndex);
    }
}
exports.LAppLive2DManager = LAppLive2DManager;
