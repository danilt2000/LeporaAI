"use strict";
/**
 * Copyright(c) Live2D Inc. All rights reserved.
 *
 * Use of this source code is governed by the Live2D Open Software license
 * that can be found at https://www.live2d.com/eula/live2d-open-software-license-agreement_en.html.
 */
Object.defineProperty(exports, "__esModule", { value: true });
const lappdelegate_1 = require("./lappdelegate");
/**
 * ブラウザロード後の処理
 */
window.addEventListener('load', () => {
    // Initialize WebGL and create the application instance
    if (!lappdelegate_1.LAppDelegate.getInstance().initialize()) {
        return;
    }
    lappdelegate_1.LAppDelegate.getInstance().run();
}, { passive: true });
/**
 * 終了時の処理
 */
window.addEventListener('beforeunload', () => lappdelegate_1.LAppDelegate.releaseInstance(), { passive: true });
