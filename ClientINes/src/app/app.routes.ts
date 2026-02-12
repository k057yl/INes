import { Routes } from '@angular/router';
import { LoginComponent } from './pages/login/login.component';
import { HomeComponent } from './pages/home/home.component';
import { RegisterComponent } from './pages/register/register.component';
import { ConfirmRegisterComponent } from './pages/register/confirm.register.component';
import { LocationCreateComponent } from './pages/location/location-create.component';
import { ItemCreateComponent } from './pages/item/item-create.component';
import { CategoryCreateComponent } from './pages/category/category-create.component';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'confirm-register', component: ConfirmRegisterComponent },
  { path: 'home', component: HomeComponent },
  { path: 'location-create', component: LocationCreateComponent },
  { path: 'create-item', component: ItemCreateComponent},
  { path: 'create-category', component: CategoryCreateComponent}
];