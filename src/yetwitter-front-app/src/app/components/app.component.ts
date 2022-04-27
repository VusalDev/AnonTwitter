import { Component } from '@angular/core';
import { environment } from 'src/environments/environment';
import { faBars, faEllipsis } from '@fortawesome/free-solid-svg-icons';
import {MatDialog} from '@angular/material/dialog';
import { RegisterPopupComponent } from './register-popup/register-popup.component';
import { LoginPopupComponent } from './login-popup/login-popup.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.less']
})
export class AppComponent {
  title = environment.title;
  faBars = faBars;
  faEllipsis=faEllipsis;

  dialogOptions = {
    panelClass: 'full-screen-modal',
  };

  constructor(public registerDialog: MatDialog, public loginDialog: MatDialog) {}

  openRegisterDialog() {
    const dialogRef = this.registerDialog.open(RegisterPopupComponent, this.dialogOptions);

    dialogRef.afterClosed().subscribe(result => {
      console.log(`Dialog result: ${result}`);
    });
  }

  openLoginDialog() {
    const dialogRef = this.registerDialog.open(LoginPopupComponent, this.dialogOptions);

    dialogRef.afterClosed().subscribe(result => {
      console.log(`Dialog result: ${result}`);
    });
  }
}
