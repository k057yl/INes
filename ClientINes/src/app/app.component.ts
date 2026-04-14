import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HeaderComponent } from './shared/components/header/header.component';
import { FooterComponent } from './shared/components/footer/footer.component';
import { LocalizationService } from './shared/services/localization.service';

import { MainPageModalService } from './features/inventory/main/main-page.modal.service';
import { InestModalComponent } from './shared/components/modal/shared-modal/inest-modal.component';
import { SellModalComponent } from './shared/components/sell-modal/sell-modal.component';
import { LendItemModalComponent } from './shared/components/modal/lend-modal/lend-item-modal.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet, 
    CommonModule, 
    HeaderComponent, 
    FooterComponent,
    InestModalComponent, 
    SellModalComponent, 
    LendItemModalComponent 
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  public modal = inject(MainPageModalService);

  constructor(private loc: LocalizationService) {}

  changeLang(lang: string) {
    this.loc.setLanguage(lang);
  }
}