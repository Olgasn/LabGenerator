import type { Config } from 'tailwindcss';
import tailwindcssAnimate from 'tailwindcss-animate';

const config: Config = {
  content: ['./src/**/*.{ts,tsx}'],
  theme: {
    container: {
      center: true,
      padding: '2rem',
      screens: {
        '2xl': '1400px',
      },
    },
    extend: {
      fontFamily: {
        sans: ['Inter', 'system-ui', 'sans-serif'],
        mono: ['Geist Mono', 'monospace'],
      },
      colors: {
        border: 'hsl(var(--border))',
        input: 'hsl(var(--input))',
        ring: 'hsl(var(--ring))',
        background: 'hsl(var(--background))',
        foreground: 'hsl(var(--foreground))',
        primary: {
          DEFAULT: 'hsl(var(--primary))',
          foreground: 'hsl(var(--primary-foreground))',
        },
        secondary: {
          DEFAULT: 'hsl(var(--secondary))',
          foreground: 'hsl(var(--secondary-foreground))',
        },
        destructive: {
          DEFAULT: 'hsl(var(--destructive))',
          foreground: 'hsl(var(--destructive-foreground))',
        },
        muted: {
          DEFAULT: 'hsl(var(--muted))',
          foreground: 'hsl(var(--muted-foreground))',
        },
        accent: {
          DEFAULT: 'hsl(var(--accent))',
          foreground: 'hsl(var(--accent-foreground))',
        },
        popover: {
          DEFAULT: 'hsl(var(--popover))',
          foreground: 'hsl(var(--popover-foreground))',
        },
        card: {
          DEFAULT: 'hsl(var(--card))',
          foreground: 'hsl(var(--card-foreground))',
        },
        surface: {
          primary: 'hsl(var(--surface-primary))',
          secondary: 'hsl(var(--surface-secondary))',
          tertiary: 'hsl(var(--surface-tertiary))',
          card: 'hsl(var(--surface-card))',
          hover: 'hsl(var(--surface-hover))',
        },
        purple: {
          DEFAULT: 'hsl(var(--accent-primary))',
          hover: 'hsl(var(--accent-primary-hover))',
          muted: 'hsl(var(--accent-primary-muted))',
        },
        indigo: {
          DEFAULT: 'hsl(var(--accent-secondary))',
        },
        success: {
          DEFAULT: 'hsl(var(--accent-success))',
        },
        warning: {
          DEFAULT: 'hsl(var(--accent-warning))',
        },
        fg: {
          primary: 'hsl(var(--fg-primary))',
          secondary: 'hsl(var(--fg-secondary))',
          muted: 'hsl(var(--fg-muted))',
          inverse: 'hsl(var(--fg-inverse))',
        },
      },
      borderRadius: {
        '3xl': '1.5rem',
        '2xl': '1rem',
        xl: '0.75rem',
        lg: 'var(--radius)',
        md: 'calc(var(--radius) - 2px)',
        sm: 'calc(var(--radius) - 4px)',
      },
      keyframes: {
        'accordion-down': {
          from: { height: '0' },
          to: { height: 'var(--radix-accordion-content-height)' },
        },
        'accordion-up': {
          from: { height: 'var(--radix-accordion-content-height)' },
          to: { height: '0' },
        },
        shimmer: {
          '0%': { backgroundPosition: '-200% 0' },
          '100%': { backgroundPosition: '200% 0' },
        },
        aurora: {
          '0%, 100%': { opacity: '0.4', transform: 'translateX(-5%) translateY(-5%) scale(1.1) rotate(-3deg)' },
          '33%': { opacity: '0.6', transform: 'translateX(5%) translateY(3%) scale(1.2) rotate(3deg)' },
          '66%': { opacity: '0.35', transform: 'translateX(-3%) translateY(7%) scale(1.05) rotate(-1deg)' },
        },
        float: {
          '0%, 100%': { transform: 'translateY(0px)' },
          '50%': { transform: 'translateY(-6px)' },
        },
        'pulse-glow': {
          '0%, 100%': { boxShadow: '0 0 0 0 hsl(var(--accent-primary) / 0)' },
          '50%': { boxShadow: '0 0 24px 4px hsl(var(--accent-primary) / 0.2)' },
        },
        'gradient-shift': {
          '0%': { backgroundPosition: '0% 50%' },
          '50%': { backgroundPosition: '100% 50%' },
          '100%': { backgroundPosition: '0% 50%' },
        },
        'spin-slow': {
          from: { transform: 'rotate(0deg)' },
          to: { transform: 'rotate(360deg)' },
        },
        'border-glow': {
          '0%, 100%': { borderColor: 'hsl(var(--accent-primary) / 0.2)' },
          '50%': { borderColor: 'hsl(var(--accent-primary) / 0.5)' },
        },
        'slide-in-left': {
          from: { opacity: '0', transform: 'translateX(-16px)' },
          to: { opacity: '1', transform: 'translateX(0)' },
        },
        'count-up': {
          from: { opacity: '0', transform: 'translateY(8px)' },
          to: { opacity: '1', transform: 'translateY(0)' },
        },
        'fade-up': {
          from: { opacity: '0', transform: 'translateY(16px)' },
          to: { opacity: '1', transform: 'translateY(0)' },
        },
      },
      animation: {
        'accordion-down': 'accordion-down 0.2s ease-out',
        'accordion-up': 'accordion-up 0.2s ease-out',
        shimmer: 'shimmer 2.2s linear infinite',
        aurora: 'aurora 8s ease-in-out infinite',
        float: 'float 3s ease-in-out infinite',
        'pulse-glow': 'pulse-glow 2.5s ease-in-out infinite',
        'gradient-shift': 'gradient-shift 4s ease infinite',
        'spin-slow': 'spin-slow 2s linear infinite',
        'spin-slower': 'spin-slow 4s linear infinite',
        'border-glow': 'border-glow 2.5s ease-in-out infinite',
        'slide-in-left': 'slide-in-left 0.3s ease-out',
        'count-up': 'count-up 0.4s ease-out',
        'fade-up': 'fade-up 0.5s ease-out',
      },
      backgroundImage: {
        'gradient-radial': 'radial-gradient(var(--tw-gradient-stops))',
        'gradient-conic': 'conic-gradient(from 180deg at 50% 50%, var(--tw-gradient-stops))',
        'shimmer-gradient': 'linear-gradient(90deg, transparent 25%, hsl(var(--surface-hover)) 50%, transparent 75%)',
      },
      boxShadow: {
        'glow-purple': '0 0 20px hsl(var(--accent-primary) / 0.3), 0 0 40px hsl(var(--accent-primary) / 0.1)',
        'glow-purple-sm': '0 0 10px hsl(var(--accent-primary) / 0.25)',
        'glow-success': '0 0 20px hsl(var(--accent-success) / 0.3)',
        'card-hover': '0 8px 32px hsl(0 0% 0% / 0.4), 0 2px 8px hsl(0 0% 0% / 0.2)',
        'inner-glow': 'inset 0 1px 0 hsl(var(--fg-primary) / 0.05)',
      },
    },
  },
  plugins: [tailwindcssAnimate],
};

export default config;
