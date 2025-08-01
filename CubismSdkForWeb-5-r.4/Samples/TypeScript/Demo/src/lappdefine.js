"use strict";
/**
 * Copyright(c) Live2D Inc. All rights reserved.
 *
 * Use of this source code is governed by the Live2D Open Software license
 * that can be found at https://www.live2d.com/eula/live2d-open-software-license-agreement_en.html.
 */
Object.defineProperty(exports, "__esModule", { value: true });
exports.RenderTargetHeight = exports.RenderTargetWidth = exports.CubismLoggingLevel = exports.DebugTouchLogEnable = exports.DebugLogEnable = exports.MotionConsistencyValidationEnable = exports.MOCConsistencyValidationEnable = exports.PriorityForce = exports.PriorityNormal = exports.PriorityIdle = exports.PriorityNone = exports.HitAreaNameBody = exports.HitAreaNameHead = exports.MotionGroupTapBody = exports.MotionGroupIdle = exports.ModelDirSize = exports.ModelDir = exports.PowerImageName = exports.GearImageName = exports.BackImageName = exports.ResourcesPath = exports.ViewLogicalMaxTop = exports.ViewLogicalMaxBottom = exports.ViewLogicalMaxRight = exports.ViewLogicalMaxLeft = exports.ViewLogicalTop = exports.ViewLogicalBottom = exports.ViewLogicalRight = exports.ViewLogicalLeft = exports.ViewMinScale = exports.ViewMaxScale = exports.ViewScale = exports.CanvasNum = exports.CanvasSize = void 0;
const live2dcubismframework_1 = require("@framework/live2dcubismframework");
/**
 * Sample Appで使用する定数
 */
// Canvas width and height pixel values, or dynamic screen size ('auto').
exports.CanvasSize = 'auto';
// キャンバスの数
exports.CanvasNum = 1;
// 画面
exports.ViewScale = 1.0;
exports.ViewMaxScale = 2.0;
exports.ViewMinScale = 0.8;
exports.ViewLogicalLeft = -1.0;
exports.ViewLogicalRight = 1.0;
exports.ViewLogicalBottom = -1.0;
exports.ViewLogicalTop = 1.0;
exports.ViewLogicalMaxLeft = -2.0;
exports.ViewLogicalMaxRight = 2.0;
exports.ViewLogicalMaxBottom = -2.0;
exports.ViewLogicalMaxTop = 2.0;
// 相対パス
exports.ResourcesPath = '../../Resources/';
// モデルの後ろにある背景の画像ファイル
exports.BackImageName = 'back_class_normal.png';
// 歯車
exports.GearImageName = 'icon_gear.png';
// 終了ボタン
exports.PowerImageName = 'CloseNormal.png';
// モデル定義---------------------------------------------
// モデルを配置したディレクトリ名の配列
// ディレクトリ名とmodel3.jsonの名前を一致させておくこと
exports.ModelDir = [
    'IceGirl',
    'Haru',
    'Hiyori',
    'Mark',
    'Natori',
    'Rice',
    'Mao',
    'Wanko',
    'miara_pro_t03',
    'miku'
];
exports.ModelDirSize = exports.ModelDir.length;
// 外部定義ファイル（json）と合わせる
exports.MotionGroupIdle = 'Idle'; // アイドリング
exports.MotionGroupTapBody = 'TapBody'; // 体をタップしたとき
// 外部定義ファイル（json）と合わせる
exports.HitAreaNameHead = 'Head';
exports.HitAreaNameBody = 'Body';
// モーションの優先度定数
exports.PriorityNone = 0;
exports.PriorityIdle = 1;
exports.PriorityNormal = 2;
exports.PriorityForce = 3;
// MOC3の整合性検証オプション
exports.MOCConsistencyValidationEnable = true;
// motion3.jsonの整合性検証オプション
exports.MotionConsistencyValidationEnable = true;
// デバッグ用ログの表示オプション
exports.DebugLogEnable = true;
exports.DebugTouchLogEnable = false;
// Frameworkから出力するログのレベル設定
exports.CubismLoggingLevel = live2dcubismframework_1.LogLevel.LogLevel_Verbose;
// デフォルトのレンダーターゲットサイズ
exports.RenderTargetWidth = 1900;
exports.RenderTargetHeight = 1000;
