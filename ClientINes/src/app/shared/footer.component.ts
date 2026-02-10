import { Component } from '@angular/core';

@Component({
  selector: 'app-footer',
  standalone: true,
  template: `
    <footer class="footer">
      © INest 2026
    </footer>
  `,
  styles: [`
    .footer {
      padding: 10px;
      text-align: center;
      background: #222;
      color: white;
    }
  `]
})
export class FooterComponent {}