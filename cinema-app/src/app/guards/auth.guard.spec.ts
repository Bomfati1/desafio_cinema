import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { authGuard } from './auth.guard';

describe('authGuard', () => {
  function setup(isLoggedIn: boolean) {
    const authSpy = jasmine.createSpyObj('AuthService', ['isLoggedIn']);
    authSpy.isLoggedIn.and.returnValue(isLoggedIn);

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authSpy },
      ],
    });

    return TestBed.runInInjectionContext(() => authGuard(null!, null!));
  }

  it('deve permitir acesso quando usuário está logado', () => {
    expect(setup(true)).toBeTrue();
  });

  it('deve redirecionar para /login quando não logado', () => {
    const result = setup(false);
    expect(result).not.toBeTrue();
    expect(result?.toString()).toBe('/login');
  });
});
