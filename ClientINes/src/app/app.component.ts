import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HeaderComponent } from './shared/header.component';
import { FooterComponent } from './shared/footer.component';
import { LocalizationService } from './services/localization.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, CommonModule, HeaderComponent, FooterComponent],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  constructor(private loc: LocalizationService) {}

  changeLang(lang: string) {
    this.loc.setLanguage(lang);
}
}