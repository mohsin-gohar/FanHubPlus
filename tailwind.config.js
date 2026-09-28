/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Views/**/*.cshtml',
    './Areas/Admin/Views/**/*.cshtml',
    './wwwroot/js/**/*.js',
  ],
  // Preflight is off on purpose: site.css + Bootstrap already own the reset,
  // and Tailwind is only here to supply the utilities the markup is written in.
  corePlugins: { preflight: false },
  // Class names that are assembled in Razor expressions (string interpolation
  // or a C# ternary) cannot be found by scanning the markup, so list them.
  safelist: [
    { pattern: /^(bg|text|border|hover:bg|hover:text|hover:border)-(success|warning|danger|primary|secondary|navy-black|body)(\/\d+)?$/ },
    { pattern: /^(group-hover:)?(opacity|visible|hidden|scale-1(0[0-9]|1[0-9]))$/ },
  ],
  theme: {
    extend: {
      colors: {
        // The template's dark palette (mirrors the --fhp-* tokens in site.css).
        primary: '#6d5dfc',
        'primary-2': '#4f8cff',
        success: '#22c55e',
        warning: '#ffea00',
        danger: '#f41b3b',
        'navy-black': '#0f0f17',
        body: '#666666',
        secondary: '#ae99fa',
      },
      fontFamily: {
        body: ['Montserrat', 'sans-serif'],
        heading: ['Montserrat', 'sans-serif'],
        secondary: ['Montserrat', 'sans-serif'],
      },
      // The markup also uses the template's own non-scale type sizes
      // (text-13 / text-15) and a .with-border button modifier.
      fontSize: {
        13: ['13px', { lineHeight: '20px' }],
        15: ['15px', { lineHeight: '22px' }],
      },
      opacity: {
        8: '0.08', 12: '0.12', 17: '0.17', 27: '0.27',
      },
      zIndex: {
        1001: '1001', 1002: '1002', 1003: '1003',
        1005: '1005', 1006: '1006', 1007: '1007', 1008: '1008', 1009: '1009',
      },
    },
  },
  plugins: [
    // .with-border is the template's outlined variant of .primary-btn.
    function ({ addUtilities }) {
      addUtilities({
        '.with-border': {
          background: 'transparent',
          border: '1px solid currentColor',
          color: 'inherit',
        },
      });
    },
  ],
};
