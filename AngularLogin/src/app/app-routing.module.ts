import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EmployeeListComponent } from './Employee/employee-list/employee-list.component';

const routes: Routes = [
  { path: '', redirectTo: 'employee', pathMatch: 'full' },
  {
    path: 'employee',
    component: EmployeeListComponent,
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
