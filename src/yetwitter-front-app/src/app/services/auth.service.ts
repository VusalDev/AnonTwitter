import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from 'rxjs';
// import 'rxjs/add/operator/do';
// import 'rxjs/add/operator/shareReplay';
import * as moment from "moment";
import { environment } from "src/environments/environment";
import { IdentityServiceClient } from "../httpclients/identity-service";

const tokenKey: string = 'ACCESS_TOKEN';
const tokenExpirationKey: string = 'ACCESS_TOKEN_EXPIRES';

@Injectable()
export class AuthService {

  httpClient:IdentityServiceClient.Client;

  constructor(private http: HttpClient) {
    this.httpClient = new IdentityServiceClient.Client(environment.apiBaseUrl);
  }

  login(username: string, password: string, rememberMe: boolean) {
    // return this.http.post<UserLogin>(this.buildUrl('/v1/auth/login'), { username, password, rememberMe })
    //   .do(res => this.setSession)
    //   .shareReplay();
    return this.httpClient.login(new IdentityServiceClient.LoginModel({ username, password, rememberMe }));
  }

  logout() {
    localStorage.removeItem(tokenKey);
    localStorage.removeItem(tokenExpirationKey);
  }

  register(username: string, password: string) {
    return this.httpClient.register(new IdentityServiceClient.RegisterModel({ username, password }));
  }

  public isLoggedIn() {
    return moment().isBefore(this.getExpiration());
  }


  getExpiration() {
    const expiration = localStorage.getItem(tokenExpirationKey) ?? "";
    const expiresAt = JSON.parse(expiration);
    return moment(expiresAt);
  }


  private setSession(authResult: IdentityServiceClient.TokenDataModel) {
    const expiresAt = moment().add(authResult.validTo.getTime(), 'second');

    localStorage.setItem(tokenKey, authResult.token);
    localStorage.setItem(tokenExpirationKey, JSON.stringify(expiresAt.valueOf()));
  }

  private buildUrl(path:string) {
    return environment.apiBaseUrl + path;
  }
}