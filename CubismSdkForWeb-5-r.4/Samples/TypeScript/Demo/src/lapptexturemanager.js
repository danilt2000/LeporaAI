"use strict";
/**
 * Copyright(c) Live2D Inc. All rights reserved.
 *
 * Use of this source code is governed by the Live2D Open Software license
 * that can be found at https://www.live2d.com/eula/live2d-open-software-license-agreement_en.html.
 */
Object.defineProperty(exports, "__esModule", { value: true });
exports.TextureInfo = exports.LAppTextureManager = void 0;
const csmvector_1 = require("@framework/type/csmvector");
/**
 * テクスチャ管理クラス
 * 画像読み込み、管理を行うクラス。
 */
class LAppTextureManager {
    /**
     * コンストラクタ
     */
    constructor() {
        this._textures = new csmvector_1.csmVector();
    }
    /**
     * 解放する。
     */
    release() {
        for (let ite = this._textures.begin(); ite.notEqual(this._textures.end()); ite.preIncrement()) {
            this._glManager.getGl().deleteTexture(ite.ptr().id);
        }
        this._textures = null;
    }
    /**
     * 画像読み込み
     *
     * @param fileName 読み込む画像ファイルパス名
     * @param usePremultiply Premult処理を有効にするか
     * @return 画像情報、読み込み失敗時はnullを返す
     */
    createTextureFromPngFile(fileName, usePremultiply, callback) {
        // search loaded texture already
        for (let ite = this._textures.begin(); ite.notEqual(this._textures.end()); ite.preIncrement()) {
            if (ite.ptr().fileName == fileName &&
                ite.ptr().usePremultply == usePremultiply) {
                // 2回目以降はキャッシュが使用される(待ち時間なし)
                // WebKitでは同じImageのonloadを再度呼ぶには再インスタンスが必要
                // 詳細：https://stackoverflow.com/a/5024181
                ite.ptr().img = new Image();
                ite
                    .ptr()
                    .img.addEventListener('load', () => callback(ite.ptr()), {
                    passive: true
                });
                ite.ptr().img.src = fileName;
                return;
            }
        }
        // データのオンロードをトリガーにする
        const img = new Image();
        img.addEventListener('load', () => {
            // テクスチャオブジェクトの作成
            const tex = this._glManager.getGl().createTexture();
            // テクスチャを選択
            this._glManager
                .getGl()
                .bindTexture(this._glManager.getGl().TEXTURE_2D, tex);
            // テクスチャにピクセルを書き込む
            this._glManager
                .getGl()
                .texParameteri(this._glManager.getGl().TEXTURE_2D, this._glManager.getGl().TEXTURE_MIN_FILTER, this._glManager.getGl().LINEAR_MIPMAP_LINEAR);
            this._glManager
                .getGl()
                .texParameteri(this._glManager.getGl().TEXTURE_2D, this._glManager.getGl().TEXTURE_MAG_FILTER, this._glManager.getGl().LINEAR);
            // Premult処理を行わせる
            if (usePremultiply) {
                this._glManager
                    .getGl()
                    .pixelStorei(this._glManager.getGl().UNPACK_PREMULTIPLY_ALPHA_WEBGL, 1);
            }
            // テクスチャにピクセルを書き込む
            this._glManager
                .getGl()
                .texImage2D(this._glManager.getGl().TEXTURE_2D, 0, this._glManager.getGl().RGBA, this._glManager.getGl().RGBA, this._glManager.getGl().UNSIGNED_BYTE, img);
            // ミップマップを生成
            this._glManager
                .getGl()
                .generateMipmap(this._glManager.getGl().TEXTURE_2D);
            // テクスチャをバインド
            this._glManager
                .getGl()
                .bindTexture(this._glManager.getGl().TEXTURE_2D, null);
            const textureInfo = new TextureInfo();
            if (textureInfo != null) {
                textureInfo.fileName = fileName;
                textureInfo.width = img.width;
                textureInfo.height = img.height;
                textureInfo.id = tex;
                textureInfo.img = img;
                textureInfo.usePremultply = usePremultiply;
                if (this._textures != null) {
                    this._textures.pushBack(textureInfo);
                }
            }
            callback(textureInfo);
        }, { passive: true });
        img.src = fileName;
    }
    /**
     * 画像の解放
     *
     * 配列に存在する画像全てを解放する。
     */
    releaseTextures() {
        for (let i = 0; i < this._textures.getSize(); i++) {
            this._glManager.getGl().deleteTexture(this._textures.at(i).id);
            this._textures.set(i, null);
        }
        this._textures.clear();
    }
    /**
     * 画像の解放
     *
     * 指定したテクスチャの画像を解放する。
     * @param texture 解放するテクスチャ
     */
    releaseTextureByTexture(texture) {
        for (let i = 0; i < this._textures.getSize(); i++) {
            if (this._textures.at(i).id != texture) {
                continue;
            }
            this._glManager.getGl().deleteTexture(this._textures.at(i).id);
            this._textures.set(i, null);
            this._textures.remove(i);
            break;
        }
    }
    /**
     * 画像の解放
     *
     * 指定した名前の画像を解放する。
     * @param fileName 解放する画像ファイルパス名
     */
    releaseTextureByFilePath(fileName) {
        for (let i = 0; i < this._textures.getSize(); i++) {
            if (this._textures.at(i).fileName == fileName) {
                this._glManager.getGl().deleteTexture(this._textures.at(i).id);
                this._textures.set(i, null);
                this._textures.remove(i);
                break;
            }
        }
    }
    /**
     * setter
     * @param glManager
     */
    setGlManager(glManager) {
        this._glManager = glManager;
    }
}
exports.LAppTextureManager = LAppTextureManager;
/**
 * 画像情報構造体
 */
class TextureInfo {
    constructor() {
        this.id = null; // テクスチャ
        this.width = 0; // 横幅
        this.height = 0; // 高さ
    }
}
exports.TextureInfo = TextureInfo;
