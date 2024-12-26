import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Role } from '../interfaces/role';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class RoleService {
  apiUrl=environment.apiUrl;
  constructor(private http:HttpClient) { }

  getRoles = () : Observable <Role[]> =>
    this.http.get<Role[]>(`${this.apiUrl}roles`)  
}
