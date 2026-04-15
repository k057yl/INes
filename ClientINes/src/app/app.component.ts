import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HeaderComponent } from './shared/components/header/header.component';
import { FooterComponent } from './shared/components/footer/footer.component';
import { LocalizationService } from './shared/services/localization.service';

import { DashboardModalService } from './features/dashboard/dashboard.modal.service';
import { InestModalComponent } from './shared/components/modals/inest-modal/inest-modal.component';
import { SellModalComponent } from './shared/components/sell-modal/sell-modal.component';
import { LendItemModalComponent } from './shared/components/modals/lend-modal/lend-item-modal.component';
import { ItemFormModalComponent } from './shared/components/modals/item-form-modal/item-form-modal.component';
import { LocationFormModalComponent } from './shared/components/modals/location-form-modal/location-form-modal.component';


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
    LendItemModalComponent,
    ItemFormModalComponent, 
    LocationFormModalComponent
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  public modal = inject(DashboardModalService);

  constructor(private loc: LocalizationService) {}

  changeLang(lang: string) {
    this.loc.setLanguage(lang);
  }
}