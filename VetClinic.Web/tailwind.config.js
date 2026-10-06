/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        botanical: {
          linen: "#FBF9F5",      // Fondo cálido descansado
          surface: "#FFFFFF",    // Superficie blanca tarjetas
          stone: "#E7E5E4",      // Bordes piedra sutiles
          forest: "#166534",     // Verde Bosque Eucalipto (Sidebar/Brand)
          forestDark: "#14532D", // Verde Bosque Profundo
          emerald: "#059669",    // Esmeralda Botánico (Acciones/Botones)
          emeraldHover: "#047857",
          amber: "#F59E0B",      // Ámbar Miel (Vacunas próximas/Alertas)
          amberDark: "#D97706",
          coral: "#DC2626",      // Coral Alerta (Vacunas vencidas/Peligro)
          graphite: "#1C1917",   // Texto primario de alto contraste
          muted: "#57534E",      // Texto secundario cálido
          subtle: "#78716C",     // Texto terciario
        }
      },
      fontFamily: {
        sans: [
          "Inter",
          "-apple-system",
          "BlinkMacSystemFont",
          "'Segoe UI'",
          "Roboto",
          "Oxygen",
          "Ubuntu",
          "Cantarell",
          "sans-serif"
        ]
      },
      boxShadow: {
        'card': '0 1px 3px 0 rgba(28, 25, 23, 0.05), 0 1px 2px -1px rgba(28, 25, 23, 0.05)',
        'card-hover': '0 4px 6px -1px rgba(28, 25, 23, 0.08), 0 2px 4px -2px rgba(28, 25, 23, 0.05)',
        'modal': '0 20px 25px -5px rgba(28, 25, 23, 0.1), 0 8px 10px -6px rgba(28, 25, 23, 0.1)',
      }
    },
  },
  plugins: [],
}
