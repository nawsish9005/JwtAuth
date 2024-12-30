import { Component, inject } from '@angular/core';
import { RoleFormComponent } from "../../components/role-form/role-form.component";
import { RoleService } from '../../services/role.service';
import { RoleCreateRequest } from '../../interfaces/role-create-request';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-role',
  imports: [RoleFormComponent, MatSnackBarModule],
  templateUrl: './role.component.html',
  styleUrl: './role.component.css'
})
export class RoleComponent {
  roleService=inject(RoleService);
  errorMessage='';
  role: RoleCreateRequest ={} as RoleCreateRequest;
  snackBar=inject(MatSnackBar);

  createRole(role:RoleCreateRequest){
    this.roleService.createRole(role).subscribe({
      next: (response: {message: string })=> {
        this.snackBar.open('Role Created Successfully', 'Ok', {
          duration: 3000,
        });
      },
      error:(error:HttpErrorResponse)=> {
        if(error.status==400){
          this.errorMessage=error.error;
        }
      },
    });
  }
}
