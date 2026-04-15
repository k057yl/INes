import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { ConfirmRegisterComponent } from './features/auth/register/confirm.register.component';
import { DashboardComponent } from './features/dashboard/dashboard.component';
import { LocationDetailComponent } from './features/inventory/location/details/location-detail.component';
import { SalesListComponent } from './features/sales/sales-list.component';
import { SettingsComponent } from './features/setting/settings.component';
import { ItemDetailComponent } from './features/inventory/item/details/item-detail.component';
import { ItemsExplorerComponent } from './features/inventory/item/explorer/items-explorer.component';
import { authGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'login', component: LoginComponent, canActivate: [guestGuard] },
  { path: 'register', component: RegisterComponent, canActivate: [guestGuard] },
  { path: 'confirm-register', component: ConfirmRegisterComponent, canActivate: [guestGuard] },
  
  // Защищенные роуты (Inventory & App)
  { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] }, 
  { path: 'location/:id', component: LocationDetailComponent, canActivate: [authGuard] },
  { path: 'sales', component: SalesListComponent, canActivate: [authGuard] },
  { path: 'settings', component: SettingsComponent, canActivate: [authGuard] },
  { path: 'item/:id', component: ItemDetailComponent, canActivate: [authGuard] },
  { path: 'explorer', component: ItemsExplorerComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: 'dashboard' }
];