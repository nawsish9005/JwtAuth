import { Component, inject } from '@angular/core';
import { RoleFormComponent } from "../../components/role-form/role-form.component";
import { RoleService } from '../../services/role.service';
import { RoleCreateRequest } from '../../interfaces/role-create-request';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { HttpErrorResponse } from '@angular/common/http';
import { RoleListComponent } from '../../components/role-list/role-list.component';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-role',
  imports: [RoleFormComponent, RoleListComponent, AsyncPipe, MatSnackBarModule],
  templateUrl: './role.component.html',
  styleUrl: './role.component.css'
})
export class RoleComponent {
  roleService=inject(RoleService);
  errorMessage='';
  role: RoleCreateRequest ={} as RoleCreateRequest;
  roles$=this.roleService.getRoles();
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

  deleteRole(id: string){
    this.roleService.delete(id).subscribe({
      next:(response)=>{
        this.roles$=this.roleService.getRoles();
        this.snackBar.open("Role Deleted Successfully", "Close", {
          duration: 3000,
        });
      },
      error:(error:HttpErrorResponse)=>{
        this.snackBar.open(error.message, "Close", {
          duration: 3000,
        });
      }
    });
  }
}
