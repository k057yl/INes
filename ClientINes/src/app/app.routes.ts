import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { ConfirmRegisterComponent } from './features/auth/register/confirm.register.component';
import { MainPageComponent } from './features/inventory/main/main-page.component';
import { LocationCreateComponent } from './features/inventory/location/location-create.component';
import { ItemCreateComponent } from './features/inventory/item/create/item-create.component';
import { LocationDetailComponent } from './features/inventory/location/location-detail.component';
import { SalesListComponent } from './features/sales/sales-list.component';
import { SettingsComponent } from './features/setting/settings.component';
import { ItemDetailComponent } from './features/inventory/item/details/item-detail.component';
import { authGuard } from './core/guards/auth.guard';
import { AdminPanelComponent } from './features/admin/admin-panel.component';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'confirm-register', component: ConfirmRegisterComponent },
  
  // Защищенные роуты (добавляем canActivate)
  { path: 'main', component: MainPageComponent, canActivate: [authGuard] }, 
  { path: 'location-create', component: LocationCreateComponent, canActivate: [authGuard] },
  { path: 'create-item', component: ItemCreateComponent, canActivate: [authGuard] },
  { path: 'location/:id', component: LocationDetailComponent, canActivate: [authGuard] },
  { path: 'sales', component: SalesListComponent, canActivate: [authGuard] },
  { path: 'settings', component: SettingsComponent, canActivate: [authGuard] },
  { path: 'item/:id', component: ItemDetailComponent, canActivate: [authGuard] },
  { path: 'admin', component: AdminPanelComponent, canActivate: [authGuard] },
];