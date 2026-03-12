import { Component } from '@angular/core';

@Component({
  selector: 'app-footer',
  standalone: true,
  template: `
    <footer class="footer">
      <div class="footer-content">
        <div class="footer-left">
          <span class="copyright">© INest 2026</span>
          <div class="status-indicator">
            <span class="dot"></span>
            <small>Stable</small>
          </div>
        </div>
        
        <div class="footer-right">
          <small class="version">v1.2.12</small>
          <div class="footer-nav">
            <a href="#">Support</a>
            <a href="#">Privacy</a>
          </div>
        </div>
      </div>
    </footer>
  `,
  styles: [`
    .footer {
      padding: 1.5rem 1.5rem;
      background: var(--bg-card);
      color: var(--text-muted);
      border-top: 1px solid var(--border-color);
      margin-top: auto;
      transition: background 0.3s ease;
    }

    .footer-content {
      max-width: 1400px;
      margin: 0 auto;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .footer-left, .footer-right {
      display: flex;
      align-items: center;
      gap: 15px;
    }

    .copyright {
      font-weight: 700;
      color: var(--text-main);
      letter-spacing: 0.5px;
    }

    .status-indicator {
      display: flex;
      align-items: center;
      gap: 6px;
      padding: 4px 10px;
      background: rgba(0, 184, 148, 0.05);
      border-radius: 20px;
      
      .dot {
        width: 6px;
        height: 6px;
        background: var(--accent-color);
        border-radius: 50%;
        box-shadow: 0 0 8px var(--accent-color);
      }

      small {
        font-size: 0.7rem;
        text-transform: uppercase;
        font-weight: 800;
        color: var(--accent-color);
      }
    }

    .version {
      font-family: 'Courier New', monospace;
      opacity: 0.6;
      font-size: 0.75rem;
    }

    .footer-nav {
      display: flex;
      gap: 15px;
      
      a {
        color: var(--text-muted);
        text-decoration: none;
        font-size: 0.85rem;
        transition: color 0.2s;

        &:hover {
          color: var(--accent-color);
        }
      }
    }

    @media (max-width: 600px) {
      .footer-content {
        flex-direction: column;
        gap: 20px;
        text-align: center;
      }
      .footer-left, .footer-right {
        flex-direction: column;
        gap: 10px;
      }
    }
  `]
})
export class FooterComponent {}