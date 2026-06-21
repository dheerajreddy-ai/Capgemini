import { Injectable } from '@angular/core';
import { School } from '../models/models';

/**
 * Applies per-tenant white-label branding by overriding CSS custom
 * properties at runtime and updating the favicon / document title.
 */
@Injectable({ providedIn: 'root' })
export class BrandingService {
  apply(school: School): void {
    if (school.primaryColor) {
      const root = document.documentElement;
      const rgb = this.hexToRgb(school.primaryColor);
      root.style.setProperty('--ev-primary', school.primaryColor);
      if (rgb) root.style.setProperty('--ev-primary-rgb', `${rgb.r}, ${rgb.g}, ${rgb.b}`);
      root.style.setProperty('--ev-grad-primary',
        `linear-gradient(135deg, ${this.lighten(school.primaryColor, 14)} 0%, ${school.primaryColor} 60%, ${this.darken(school.primaryColor, 12)} 100%)`);
      root.style.setProperty('--ev-primary-soft', this.tint(school.primaryColor, 0.07));
      root.style.setProperty('--ev-primary-light', this.tint(school.primaryColor, 0.16));
    }

    if (school.logoUrl) {
      const link = (document.querySelector("link[rel~='icon']") as HTMLLinkElement) ?? document.createElement('link');
      link.rel = 'icon';
      link.href = school.logoUrl;
      document.head.appendChild(link);
    }

    document.title = `${school.name} · EduVoice`;
  }

  private hexToRgb(hex: string): { r: number; g: number; b: number } | null {
    const m = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return m ? { r: parseInt(m[1], 16), g: parseInt(m[2], 16), b: parseInt(m[3], 16) } : null;
  }

  private lighten(hex: string, amt: number): string { return this.shift(hex, amt); }
  private darken(hex: string, amt: number): string { return this.shift(hex, -amt); }

  private shift(hex: string, amt: number): string {
    const rgb = this.hexToRgb(hex);
    if (!rgb) return hex;
    const c = (v: number) => Math.max(0, Math.min(255, Math.round(v + (amt / 100) * 255)));
    return `#${[c(rgb.r), c(rgb.g), c(rgb.b)].map((v) => v.toString(16).padStart(2, '0')).join('')}`;
  }

  private tint(hex: string, alpha: number): string {
    const rgb = this.hexToRgb(hex);
    return rgb ? `rgba(${rgb.r}, ${rgb.g}, ${rgb.b}, ${alpha})` : hex;
  }
}
