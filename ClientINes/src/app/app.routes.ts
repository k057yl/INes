import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { ConfirmRegisterComponent } from './features/auth/register/confirm.register.component';
import { MainPageComponent } from './features/inventory/main/main-page.component';
import { LocationCreateComponent } from './features/inventory/location/create/location-create.component';
import { ItemCreateComponent } from './features/inventory/item/create/item-create.component';
import { LocationDetailComponent } from './features/inventory/location/details/location-detail.component';
import { SalesListComponent } from './features/sales/sales-list.component';
import { SettingsComponent } from './features/setting/settings.component';
import { ItemDetailComponent } from './features/inventory/item/details/item-detail.component';
import { ItemEditComponent } from './features/inventory/item/edit/Item-edit.component';
import { ItemsExplorerComponent } from './features/inventory/item/explorer/items-explorer.component';
import { authGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'main', pathMatch: 'full' },
  { path: 'login', component: LoginComponent, canActivate: [guestGuard] },
  { path: 'register', component: RegisterComponent, canActivate: [guestGuard] },
  { path: 'confirm-register', component: ConfirmRegisterComponent, canActivate: [guestGuard] },
  
  // 3. Защищенные роуты (Inventory & App)
  { path: 'main', component: MainPageComponent, canActivate: [authGuard] }, 
  { path: 'location-create', component: LocationCreateComponent, canActivate: [authGuard] },
  { path: 'create-item', component: ItemCreateComponent, canActivate: [authGuard] },
  { path: 'location/:id', component: LocationDetailComponent, canActivate: [authGuard] },
  { path: 'sales', component: SalesListComponent, canActivate: [authGuard] },
  { path: 'settings', component: SettingsComponent, canActivate: [authGuard] },
  { path: 'item/:id', component: ItemDetailComponent, canActivate: [authGuard] },
  { path: 'item/edit/:id', component: ItemEditComponent, canActivate: [authGuard] },
  { path: 'explorer', component: ItemsExplorerComponent, canActivate: [authGuard] },
  { path: '**', redirectTo: 'main' }
];