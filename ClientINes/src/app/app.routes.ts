import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { ConfirmRegisterComponent } from './pages/register/confirm.register.component';
import { MainPageComponent } from './pages/main/main-page.component';
import { LocationCreateComponent } from './pages/location/location-create.component';
import { ItemCreateComponent } from './pages/item/create/item-create.component';
import { CategoryListComponent } from './pages/category/category-list.component';
import { LocationDetailComponent } from './pages/location/location-detail.component';
import { SalesListComponent } from './pages/sales/sales-list.component';
import { SettingsComponent } from './pages/setting/settings.component';
import { ItemDetailComponent } from './pages/item/details/item-detail.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'confirm-register', component: ConfirmRegisterComponent },
  
  // Защищенные роуты (добавляем canActivate)
  { path: 'main', component: MainPageComponent, canActivate: [authGuard] }, 
  { path: 'location-create', component: LocationCreateComponent, canActivate: [authGuard] },
  { path: 'create-item', component: ItemCreateComponent, canActivate: [authGuard] },
  { path: 'category', component: CategoryListComponent, canActivate: [authGuard] },
  { path: 'location/:id', component: LocationDetailComponent, canActivate: [authGuard] },
  { path: 'sales', component: SalesListComponent, canActivate: [authGuard] },
  { path: 'settings', component: SettingsComponent, canActivate: [authGuard] },
  { path: 'item/:id', component: ItemDetailComponent, canActivate: [authGuard] },
];