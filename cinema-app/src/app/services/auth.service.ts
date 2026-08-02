import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { LoginRequest, LoginResponse, RefreshTokenRequest } from '../models/cinema.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private baseUrl = environment.apiUrl;

  login(email: string, password: string): Observable<LoginResponse> {
    const body: LoginRequest = { email, password };
    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/login`, body).pipe(
      tap((response) => {
        this.storeToken(response.token);
        this.storeRefreshToken(response.refreshToken);
        this.storeUserInfo(response);
      })
    );
  }

  logout(): void {
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('user_info');
    this.router.navigate(['/login']);
  }

  /** Tenta renovar o token JWT usando o refresh token armazenado. */
  refreshToken(): Observable<LoginResponse> {
    const refreshToken = this.getRefreshToken();
    const body: RefreshTokenRequest = { refreshToken: refreshToken ?? '' };
    return this.http.post<LoginResponse>(`${this.baseUrl}/auth/refresh`, body).pipe(
      tap((response) => {
        this.storeToken(response.token);
        this.storeRefreshToken(response.refreshToken);
        this.storeUserInfo(response);
      })
    );
  }

  private storeToken(token: string): void {
    localStorage.setItem('jwt_token', token);
  }

  getToken(): string | null {
    return localStorage.getItem('jwt_token');
  }

  private storeRefreshToken(token: string): void {
    localStorage.setItem('refresh_token', token);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refresh_token');
  }

  private storeUserInfo(response: LoginResponse): void {
    localStorage.setItem('user_info', JSON.stringify({
      name: response.name,
      email: response.email,
      role: response.role
    }));
  }

  getUserInfo(): { name: string; email: string; role: string } | null {
    const info = localStorage.getItem('user_info');
    if (!info) return null;
    try {
      return JSON.parse(info);
    } catch {
      localStorage.removeItem('user_info');
      return null;
    }
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    if (!token) return false;

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const exp = payload.exp * 1000;
      return Date.now() < exp;
    } catch {
      return false;
    }
  }

  isAdmin(): boolean {
    const user = this.getUserInfo();
    return user?.role === 'Admin';
  }

  getUserName(): string {
    return this.getUserInfo()?.name ?? '';
  }
}
