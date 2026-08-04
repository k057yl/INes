import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/components/login/login.component';
import { RegisterComponent } from './features/auth/components/register/register.component';
import { ConfirmRegisterComponent } from './features/auth/components/confirm/confirm.register.component';
import { DashboardComponent } from './features/dashboard/components/dashboard/dashboard.component';
import { LocationDetailComponent } from './features/location/components/details/location-detail.component';
import { SalesListComponent } from './features/sales/components/sales-list/sales-list.component';
import { SettingsComponent } from './features/setting/components/setting/settings.component';
import { ItemDetailComponent } from './features/item/components/details/item-detail.component';
import { ItemsListComponent } from './features/item/components/items-list/items-list.component';
import { AdminFeedbackComponent } from './features/admin/components/admin-feedback.component';
import { authGuard, guestGuard } from './core/guards/auth.guard';
import { locationResolver } from './features/location/components/location.resolver';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  { path: 'login', component: LoginComponent, canActivate: [guestGuard] },
  { path: 'register', component: RegisterComponent, canActivate: [guestGuard] },
  { path: 'confirm-register', component: ConfirmRegisterComponent, canActivate: [guestGuard] },
  
  { path: 'dashboard', component: DashboardComponent, canActivate: [authGuard] },
  { 
    path: 'location/:id', 
    component: LocationDetailComponent, 
    canActivate: [authGuard],
    resolve: { locationData: locationResolver }
  },
  { path: 'sales', component: SalesListComponent, canActivate: [authGuard] },
  { path: 'settings', component: SettingsComponent, canActivate: [authGuard] },
  { path: 'item/:id', component: ItemDetailComponent, canActivate: [authGuard] },
  { path: 'items-list', component: ItemsListComponent, canActivate: [authGuard] },
  { path: 'admin/feedback', component: AdminFeedbackComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: 'dashboard' }
];