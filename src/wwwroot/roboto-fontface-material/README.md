# roboto-fontface-material
Font library for local install of Roboto variants used by Angular Material.

[![version](https://img.shields.io/npm/v/roboto-fontface-material.svg)](https://www.npmjs.com/package/roboto-fontface-material)
[![downloads](https://img.shields.io/npm/dt/roboto-fontface-material.svg)](https://www.npmjs.com/package/roboto-fontface-material)
[![MIT License](https://img.shields.io/github/license/bjowes/roboto-fontface-material.svg)](https://github.com/bjowes/roboto-fontface-material/blob/master/LICENSE)


## Motivation

When using the Roboto fonts in applications I have been developing, I found existing libraries for local installation of the fonts didn't fit my purposes:
* I wanted to use the fonts in both library modules and applications containing such modules. When combining these the font paths were messed up.
* I wanted a streamlined package with only the fonts required for Angular Material to reduce the npm install burden

Hence I made this library. Just install it and reference it from `angular.json` and you are good to go!

### Details

Only the woff and woff2 versions are included since they are sufficient to support every modern browser.
Also, only the fontweights and styling used by Material is included. This takes us down to three fonts (six font files).

## Installation

```
npm install roboto-fontface-material
```

## Usage
In your main angular project, add this line to the scripts section in `angular.json`:
```json
"node_modules/roboto-fontface-material/fonts/roboto-fontface-material.css"
```
This ensures that the font files are included in your assets when building your application, and
Material will automatically use your local versions.

## License notes
The MIT license applies to this compilation of the Roboto fonts for Angular Material.
The fonts themselves are created by Christian Robertson and are released under the Apache license.
(Google's font page)[https://fonts.google.com/specimen/Roboto]
(Roboto font license)[http://www.apache.org/licenses/LICENSE-2.0]