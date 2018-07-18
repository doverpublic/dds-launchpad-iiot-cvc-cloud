import { Component, OnInit } from '@angular/core';
import { FormGroup, FormControl } from '@angular/forms';
import { MSALService} from '../shared/services/msal.service';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit {
  search: string;
  constructor(public service: MSALService) { }
 /* // Logout Method
 public logout() {
  this.service.logout();
} */
  ngOnInit() {

  }

 public logout() {
    this.service.logout();
  } 
}
