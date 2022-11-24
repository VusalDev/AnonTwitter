import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { environment } from "src/environments/environment";
import { AuthClient, LoginModel, RegisterModel, TokenDataModel } from "../httpclients/identity-service";

const tokenKey: string = 'ACCESS_TOKEN';
const tokenExpirationKey: string = 'ACCESS_TOKEN_EXPIRES';

@Injectable()
export class AuthService {

  authClient: AuthClient;

  constructor(private http: HttpClient) {
    this.authClient = new AuthClient(environment.apiBaseUrl);
  }

  login(username: string, password: string, rememberMe: boolean) {
    return this.authClient.login(new LoginModel({ username, password, rememberMe }))
    .then(data => {
      this.setSession(data);
    });
  }

  logout() {
    localStorage.removeItem(tokenKey);
    localStorage.removeItem(tokenExpirationKey);
  }

  register(username: string, password: string) {
    return this.authClient.register(new RegisterModel({ username, password }));
  }

  public isLoggedIn() {
    return new Date() < this.getExpiration();
  }


  getExpiration() {
    const expiration = Number(localStorage.getItem(tokenExpirationKey)) ?? 0;
    return new Date(expiration);
  }


  private setSession(authResult: TokenDataModel) {
    const validTo =authResult.validTo.getTime();
    localStorage.setItem(tokenKey, authResult.token);
    localStorage.setItem(tokenExpirationKey, validTo.toString());
  }
}