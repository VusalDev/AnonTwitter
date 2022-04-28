import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from 'rxjs';
import 'rxjs/add/operator/do';
import 'rxjs/add/operator/shareReplay';
import * as moment from "moment";
import { UserLogin } from "../models/user-login";
import { environment } from "src/environments/environment";

const tokenKey: string = 'ACCESS_TOKEN';
const tokenExpirationKey: string = 'ACCESS_TOKEN_EXPIRES';

@Injectable()
export class AuthService {

  constructor(private http: HttpClient) {

  }

  login(username: string, password: string, rememberMe: boolean) {
    return this.http.post<UserLogin>(this.buildUrl('/v1/auth/login'), { username, password, rememberMe })
      .do(res => this.setSession)
      .shareReplay();
  }

  private setSession(authResult: TokenData) {
    const expiresAt = moment().add(authResult.expiration, 'second');

    localStorage.setItem(tokenKey, authResult.token);
    localStorage.setItem(tokenExpirationKey, JSON.stringify(expiresAt.valueOf()));
  }

  logout() {
    localStorage.removeItem(tokenKey);
    localStorage.removeItem(tokenExpirationKey);
  }

  public isLoggedIn() {
    return moment().isBefore(this.getExpiration());
  }


  getExpiration() {
    const expiration = localStorage.getItem(tokenExpirationKey) ?? "";
    const expiresAt = JSON.parse(expiration);
    return moment(expiresAt);
  }

  buildUrl(path:string) {
    return environment.apiBaseUrl + path;
  }
}

class TokenData {
  token!: string;
  expiration!: string;
}