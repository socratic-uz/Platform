import resolve from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';

export default [
  // Material Color Utilities (Theme & dynamic palettes)
  {
    input: 'node_modules/@material/material-color-utilities/index.js',
    output: {
      file: 'wwwroot/js/material-color-utilities.bundle.js',
      format: 'esm',
      name: 'MaterialColorUtils'
    },
    plugins: [resolve(), commonjs()]
  },

  // Material Web Components (@material/web) with Lit SSR hydration support
  {
    input: 'src/js/material-web-entry.js',
    output: {
      file: 'wwwroot/js/material-web.bundle.js',
      format: 'iife',
      name: 'MaterialWeb'
    },
    plugins: [resolve(), commonjs()]
  }
];
