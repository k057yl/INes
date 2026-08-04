import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { HeaderComponent } from './shared/components/header/header.component';
import { FooterComponent } from './shared/components/footer/footer.component';
import { LocalizationService } from './core/services/localization.service';
import { AuthService } from './features/auth/services/auth.service';

import { DashboardModalService } from './features/dashboard/components/dashboard/dashboard.modal.service';
import { InestModalComponent } from './shared/components/inest-modal/inest-modal.component';
import { SellModalComponent } from './features/sales/components/sell-modal/sell-modal.component';
import { LendItemModalComponent } from './features/lending/components/lend-modal/lend-item-modal.component';
import { ItemFormModalComponent } from './features/item/components/item-form-modal/item-form-modal.component';
import { LocationFormModalComponent } from './features/location/components/location-form-modal/location-form-modal.component';
import { FeedbackModalComponent } from './features/feedback/components/feedback-modal/feedback-modal.component';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    HeaderComponent,
    FooterComponent,
    InestModalComponent,
    SellModalComponent,
    LendItemModalComponent,
    ItemFormModalComponent,
    LocationFormModalComponent,
    FeedbackModalComponent
],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  public modal = inject(DashboardModalService);
  private authService = inject(AuthService);

  constructor(private loc: LocalizationService) {}

  ngOnInit() {
    this.authService.checkAuth().subscribe({
      next: (user) => {
        if (user) {
          console.log('Сессия восстановлена, добро пожаловать обратно', user.email);
        } else {
          console.log('Сессии нет, ты гость');
        }
      },
      error: () => console.log('Бэкенд послал нас')
    });
  }

  changeLang(lang: string) {
    this.loc.setLanguage(lang);
  }
}