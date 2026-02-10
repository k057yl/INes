import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <header class="header">
      <h2>INest</h2>

      <nav>
        <a routerLink="/home">Home</a>
        <a routerLink="/login">Login</a>
        <a routerLink="/register">Register</a>
      </nav>
    </header>
  `,
  styles: [`
    .header {
      display: flex;
      justify-content: space-between;
      padding: 15px;
      background: #222;
      color: white;
    }

    nav a {
      margin-left: 15px;
      color: white;
      text-decoration: none;
    }
  `]
})
export class HeaderComponent {}