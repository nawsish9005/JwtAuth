import { Component, OnInit, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import {MatSelectModule} from "@angular/material/select";
import { RoleService } from '../../services/role.service';
import { Observable } from 'rxjs';
import { Role } from '../../interfaces/role';
import { AsyncPipe, CommonModule } from '@angular/common';


@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    MatInputModule,
    RouterLink,
    MatSnackBarModule,
    MatIconModule,
    ReactiveFormsModule,
    MatSelectModule,
    AsyncPipe,
    CommonModule
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent implements OnInit {
  roleService=inject(RoleService);
  roles$!:Observable<Role[]>;
  fb = inject(FormBuilder);
  registerForm!: FormGroup;
  authService = inject(AuthService);
  matSnackBar = inject(MatSnackBar);
  router = inject(Router);
  hide = true;
  passwordHide!: boolean;
  confirmPasswordHide!: boolean;

  register() {
    this.authService.register(this.registerForm.value).subscribe({
      next: (response) => {
        this.matSnackBar.open(response.message, 'Close', {
          duration: 5000,
          horizontalPosition: 'center',
        });
        this.router.navigate(['/']);
      },
      error: (error) => {
        this.matSnackBar.open(error.error.message, 'Close', {
          duration: 5000,
          horizontalPosition: 'center',
        });
      },
    });
  }

  ngOnInit(): void {
    this.registerForm = this.fb.group({
      Email: ['', [Validators.required, Validators.email]],
      FullName: ['', Validators.required],
      Roles: [''],
      Password: ['', Validators.required],
      confirmPassword: ['', Validators.required]
    },
  {
    validator: this.passwordMatchValidator,
  });

    this.roles$=this.roleService.getRoles();
  }
  private passwordMatchValidator(
    control: AbstractControl
  ): { [key:string]: boolean } | null {
    const password=control.get('Password') ?.value;
    const confirmPassword=control.get('confirmPassword') ?.value;

    if(password !== confirmPassword){
      return {passwordMismatch: true};
    }
    return null;
  }
}
