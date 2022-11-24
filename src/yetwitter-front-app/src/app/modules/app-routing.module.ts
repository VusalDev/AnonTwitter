import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { NotFoundComponent } from '../components/not-found/not-found.component';

const appRoutes: Routes = [
  //{ path: '', redirectTo: "feed", pathMatch: 'full' }, //TODO

  // { path: "about", component: AboutComponent, data: { title: 'About' } }, //TODO

  { path: '*', component: NotFoundComponent, data: { title: '404 - Not Found' } },
];



@NgModule({
  imports: [
    RouterModule.forRoot(appRoutes, {
      onSameUrlNavigation: 'reload'
    }),
    // RouterModule.forChild(childRoutes),
  ],
  exports: [
    RouterModule
  ],
  declarations: [

  ]
})
export class AppRoutingModule { }