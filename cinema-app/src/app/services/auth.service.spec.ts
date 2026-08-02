import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { AuthService } from './auth.service';
import { LoginResponse } from '../models/cinema.models';
import { environment } from '../../environments/environment';

// Token JWT falso com exp no futuro (ano 3000) para isLoggedIn() passar
const FAKE_JWT = [
  btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' })),
  btoa(JSON.stringify({ exp: 32503680000, email: 'admin@cinema.com', role: 'Admin' })),
  'fake-signature',
].join('.');

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  const loginUrl = `${environment.apiUrl}/auth/login`;

  beforeEach(() => {
    localStorage.clear();

    TestBed.configureTestingModule({
      providers: [
        AuthService,
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
      ],
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  it('deve armazenar token e user_info no login bem-sucedido', () => {
    const mockResponse: LoginResponse = {
      token: FAKE_JWT,
      email: 'admin@cinema.com',
      name: 'Admin',
      role: 'Admin',
    };

    service.login('admin@cinema.com', 'admin').subscribe((res) => {
      expect(res.token).toBe(FAKE_JWT);
      expect(res.role).toBe('Admin');
    });

    const req = httpMock.expectOne(loginUrl);
    expect(req.request.method).toBe('POST');
    req.flush(mockResponse);

    expect(localStorage.getItem('jwt_token')).toBe(FAKE_JWT);
    expect(service.isLoggedIn()).toBeTrue();
    expect(service.isAdmin()).toBeTrue();
  });

  it('isLoggedIn deve retornar false sem token', () => {
    expect(service.isLoggedIn()).toBeFalse();
  });

  it('logout deve limpar storage e redirecionar', () => {
    localStorage.setItem('jwt_token', 'abc');
    localStorage.setItem('user_info', JSON.stringify({ name: 'X', email: 'x@x.com', role: 'User' }));

    service.logout();

    expect(localStorage.getItem('jwt_token')).toBeNull();
    expect(localStorage.getItem('user_info')).toBeNull();
    expect(service.isLoggedIn()).toBeFalse();
  });

  it('getUserInfo deve retornar null com storage corrompido', () => {
    localStorage.setItem('user_info', '{corrompido}');
    const info = service.getUserInfo();
    expect(info).toBeNull();
    // Deve ter limpado a chave corrompida
    expect(localStorage.getItem('user_info')).toBeNull();
  });

  it('getToken deve retornar null se não houver token', () => {
    expect(service.getToken()).toBeNull();
  });
});
