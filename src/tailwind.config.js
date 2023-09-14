module.exports = {
  content: [ 
    './Views/**/*.{cshtml,html,js}',
    './Helpers/**/*.cs'
  ],
  theme: {
    extend: {},
  },
  variants: {
    extend: {},
  },
  plugins: [
    require('@tailwindcss/forms'),
  ],
}
