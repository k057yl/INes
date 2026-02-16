import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { ConfirmRegisterComponent } from './pages/register/confirm.register.component';
import { LocationBoardComponent } from './pages/location/location-board/location-board.component';
import { LocationCreateComponent } from './pages/location/location-create.component';
import { ItemCreateComponent } from './pages/item/item-create.component';
import { CategoryListComponent } from './pages/category/category-list.component';
import { LocationDetailComponent } from './pages/location/location-detail.component';
import { SalesListComponent } from './pages/sales/sales-list.component';
import { SettingsComponent } from './pages/setting/settings.component';
import { ItemDetailComponent } from './pages/item/item-detail.component';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'confirm-register', component: ConfirmRegisterComponent },
  { path: 'main', component: LocationBoardComponent }, 
  { path: 'location-create', component: LocationCreateComponent },
  { path: 'create-item', component: ItemCreateComponent },
  { path: 'category', component: CategoryListComponent },
  { path: 'location/:id', component: LocationDetailComponent },
  { path: 'sales', component: SalesListComponent },
  { path: 'settings', component: SettingsComponent },
  { path: 'item/:id', component: ItemDetailComponent },
];