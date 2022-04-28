import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, NgForm, Validators } from '@angular/forms';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { faXmark } from '@fortawesome/free-solid-svg-icons';
import { ProblemDetails } from 'src/app/models/problem-details';
import { AuthService } from 'src/app/services/auth.service';

@Component({
  selector: 'app-login-popup',
  templateUrl: './login-popup.component.html',
  styleUrls: ['./login-popup.component.less']
})
export class LoginPopupComponent implements OnInit {
  faXmark = faXmark;
  form: FormGroup;
  errorText: string = "";

  constructor(
    public dialogRef: MatDialogRef<LoginPopupComponent>,
    private authService: AuthService,
    private formBuilder: FormBuilder) {

    this.form = this.formBuilder.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });
  }

  ngOnInit() {
    //this.form.reset(); 
  }

  login() {
    const val = this.form.value;

    if (val.username && val.password) {
      this.authService.login(val.username, val.password, true)
        .subscribe(
          data => {
            console.log("User is logged in");
            this.dialogRef.close();
          },
          error => {
            console.log(error);
            let details = error.error as ProblemDetails
            this.errorText = details.title;
          });
    }
  }

}
