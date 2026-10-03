import resolve from '@rollup/plugin-node-resolve';
import commonjs from '@rollup/plugin-commonjs';

export default [
    // Material Color Utilities
    {
        input: 'node_modules/@material/material-color-utilities/index.js',
        output: {
            file: 'wwwroot/material-color-utilities.bundle.js',
            format: 'esm',
            name: 'MaterialColorUtils'
        },
        plugins: [resolve(), commonjs()]
    },

    // Material Web Components
    {
        input: 'node_modules/@material/web/all.js',
        output: {
            file: 'wwwroot/material-web.bundle.js',
            format: 'iife',
            name: 'MaterialWeb'
        },
        plugins: [resolve(), commonjs()]
    }
];
