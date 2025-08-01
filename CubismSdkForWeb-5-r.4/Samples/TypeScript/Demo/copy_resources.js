"use strict";
const fs = require('fs');
const path = require('path');

const publicResources = [
  {
    src: path.join(__dirname, '..', '..', '..', 'Core'),
    dst: path.join(__dirname, 'public', 'Core'),
  },
  {
    src: path.join(__dirname, '..', '..', 'Resources'),
    dst: path.join(__dirname, 'public', 'Resources'),
  }
];

publicResources.forEach(({ src, dst }) => {
  if (fs.existsSync(dst)) fs.rmSync(dst, { recursive: true });
  fs.cpSync(src, dst, { recursive: true });
});
