import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { adminGuard } from './admin.guard';

describe('adminGuard', () => {
  function setup(isLoggedIn: boolean, isAdmin: boolean) {
    const authSpy = jasmine.createSpyObj('AuthService', ['isLoggedIn', 'isAdmin']);
    authSpy.isLoggedIn.and.returnValue(isLoggedIn);
    authSpy.isAdmin.and.returnValue(isAdmin);

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: authSpy },
      ],
    });

    return TestBed.runInInjectionContext(() => adminGuard(null!, null!));
  }

  it('deve permitir acesso para Admin logado', () => {
    expect(setup(true, true)).toBeTrue();
  });

  it('deve redirecionar não logado', () => {
    const result = setup(false, false);
    expect(result).not.toBeTrue();
    expect(result?.toString()).toBe('/');
  });

  it('deve redirecionar usuário comum (não-Admin)', () => {
    const result = setup(true, false);
    expect(result).not.toBeTrue();
    expect(result?.toString()).toBe('/');
  });
});
