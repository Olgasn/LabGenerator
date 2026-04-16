'use client';

import { useTheme } from '@/shared/theme-context'
import { useEffect, useRef } from 'react'

interface Particle {
  x: number;
  y: number;
  vx: number;
  vy: number;
  radius: number;
  opacity: number;
  colorIdx: number;
  pulsePhase: number;
}

const DARK_COLORS: [number, number, number][] = [
  [168, 85, 247],
  [99, 102, 241],
  [139, 92, 246],
  [192, 132, 252],
  [148, 163, 184],
];

const LIGHT_COLORS: [number, number, number][] = [
  [147, 51, 234],
  [79, 70, 229],
  [124, 58, 237],
  [168, 85, 247],
  [99, 102, 241],
];

const MAX_DIST = 180;
const PARTICLE_COUNT_PER_10K = 7;

function createParticles(w: number, h: number, colorCount: number): Particle[] {
  const count = Math.min(140, Math.max(50, Math.floor((w * h) / 10000) * PARTICLE_COUNT_PER_10K));
  const particles: Particle[] = [];

  for (let i = 0; i < count; i++) {
    const angle = Math.random() * Math.PI * 2;
    const speed = 0.15 + Math.random() * 0.3;
    particles.push({
      x: Math.random() * w,
      y: Math.random() * h,
      vx: Math.cos(angle) * speed,
      vy: Math.sin(angle) * speed,
      radius: 1.2 + Math.random() * 2.8,
      opacity: 0.4 + Math.random() * 0.5,
      colorIdx: Math.floor(Math.random() * colorCount),
      pulsePhase: Math.random() * Math.PI * 2,
    });
  }

  return particles;
}

export function NeuralBackground() {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const { theme } = useTheme();

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const rawContext = canvas.getContext('2d');
    if (!rawContext) return;
    const context: CanvasRenderingContext2D = rawContext;

    const isLight = theme === 'light';
    const colors = isLight ? LIGHT_COLORS : DARK_COLORS;

    const lineOpacityMultiplier = isLight ? 0.38 : 0.22;
    const lineWidth = isLight ? 0.9 : 0.65;
    const glowOpacityMultiplier = isLight ? 0.22 : 0.18;
    const glowRadiusMultiplier = isLight ? 5 : 4;
    const glowThreshold = isLight ? 2.0 : 2.2;

    let animationFrameId = 0;
    let particles: Particle[] = [];
    let width = 0;
    let height = 0;
    let time = 0;

    function resize() {
      const currentCanvas = canvasRef.current;
      if (!currentCanvas) return;

      width = window.innerWidth;
      height = window.innerHeight;
      currentCanvas.width = width;
      currentCanvas.height = height;
      particles = createParticles(width, height, colors.length);
    }

    function draw() {
      context.clearRect(0, 0, width, height);
      time += 0.008;

      for (let i = 0; i < particles.length; i++) {
        const particle = particles[i];

        particle.x += particle.vx;
        particle.y += particle.vy;

        if (particle.x < -20) particle.x = width + 20;
        if (particle.x > width + 20) particle.x = -20;
        if (particle.y < -20) particle.y = height + 20;
        if (particle.y > height + 20) particle.y = -20;

        for (let j = i + 1; j < particles.length; j++) {
          const neighbor = particles[j];
          const dx = particle.x - neighbor.x;
          const dy = particle.y - neighbor.y;
          const dist = Math.sqrt(dx * dx + dy * dy);

          if (dist < MAX_DIST) {
            const lineOpacity = (1 - dist / MAX_DIST) * lineOpacityMultiplier;
            const [r1, g1, b1] = colors[particle.colorIdx];
            const [r2, g2, b2] = colors[neighbor.colorIdx];

            const gradient = context.createLinearGradient(particle.x, particle.y, neighbor.x, neighbor.y);
            gradient.addColorStop(0, `rgba(${r1},${g1},${b1},${lineOpacity})`);
            gradient.addColorStop(1, `rgba(${r2},${g2},${b2},${lineOpacity})`);

            context.beginPath();
            context.strokeStyle = gradient;
            context.lineWidth = lineWidth;
            context.moveTo(particle.x, particle.y);
            context.lineTo(neighbor.x, neighbor.y);
            context.stroke();
          }
        }

        const pulse = Math.sin(time * 1.5 + particle.pulsePhase) * 0.15;
        const nodeOpacity = Math.max(0, Math.min(1, particle.opacity + pulse));
        const [r, g, b] = colors[particle.colorIdx];

        if (particle.radius > glowThreshold) {
          context.beginPath();
          const glow = context.createRadialGradient(
            particle.x,
            particle.y,
            0,
            particle.x,
            particle.y,
            particle.radius * glowRadiusMultiplier
          );
          glow.addColorStop(0, `rgba(${r},${g},${b},${nodeOpacity * glowOpacityMultiplier})`);
          glow.addColorStop(1, `rgba(${r},${g},${b},0)`);
          context.fillStyle = glow;
          context.arc(particle.x, particle.y, particle.radius * glowRadiusMultiplier, 0, Math.PI * 2);
          context.fill();
        }

        context.beginPath();
        context.fillStyle = `rgba(${r},${g},${b},${nodeOpacity})`;
        context.arc(particle.x, particle.y, particle.radius, 0, Math.PI * 2);
        context.fill();
      }

      animationFrameId = requestAnimationFrame(draw);
    }

    resize();
    draw();

    const resizeObserver = new ResizeObserver(resize);
    resizeObserver.observe(document.documentElement);

    return () => {
      cancelAnimationFrame(animationFrameId);
      resizeObserver.disconnect();
    };
  }, [theme]);

  return (
    <canvas
      ref={canvasRef}
      className="fixed inset-0 pointer-events-none"
      style={{ zIndex: 0, opacity: 0.6 }}
    />
  );
}
