import { Component } from '@angular/core';

@Component({
  selector: 'app-footer',
  standalone: true,
  template: `
    <footer class="footer">
      <div class="footer-content">
        <span class="copyright">© INest 2026</span>
        <div class="footer-links">
          <small>v1.0.4-stable</small>
        </div>
      </div>
    </footer>
  `,
  styles: [`
    .footer {
      padding: 1.5rem;
      background: #0b132b;
      color: #94a3b8;
      border-top: 1px solid #1c2541;
      margin-top: auto;
    }
    .footer-content {
      max-width: 1400px;
      margin: 0 auto;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .copyright {
      font-weight: 600;
      letter-spacing: 1px;
    }
    @media (max-width: 480px) {
      .footer-content {
        flex-direction: column;
        gap: 10px;
      }
    }
  `]
})
export class FooterComponent {}